using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Toolbox;

public class SpitterController : MonsterController
{
    [SerializeField] private float range;

    private List<Vector3Int> attackedTiles = new List<Vector3Int>();

    protected override void ScheduleAction()
    {
        player = GameManager.Instance.playerBehaviour;
        bool playerInRange = Vector3.Distance(transform.position, player.transform.position) < range;

        FacePlayer();

        if (!playerInRange)
        {
            return;
        }

        Debug.Log("SPITTER: Player in range!");

        Vector3 playerPosition = player.transform.position;

        attackDirection = Vector3.zero;
        // First check around the positions 
        if (IsAbove(playerPosition, 0.05f, range))
        {
            attackDirection = new Vector3(0, 1f, 0);
        }
        else if (IsBelow(playerPosition, 0.05f, range))
        {
            attackDirection = new Vector3(0, -1f, 0);
        }
        else if (IsRight(playerPosition, 0.05f, range))
        {
            attackDirection = new Vector3(1f, 0, 0);
        }
        else if (IsLeft(playerPosition, 0.05f, range))
        {
            attackDirection = new Vector3(-1f, 0, 0);
        }

        Debug.Log("SPITTER: attack direction " + attackDirection);

        //else
        //{
        //    Vector3[] choices = { new Vector3(1f, 0, 0), new Vector3(0, 1f, 0),
        //        new Vector3(-1f, 0, 0), new Vector3(0, -1f, 0) };
        //
        //    int index = Random.Range(0, 4);
        //    addition = choices[index];
        //}

        if (attackDirection == Vector3.zero)
        {
            return;
        }

        List<Vector3> attackPositions = new List<Vector3>();
        Vector3 position = transform.position;
        bool playerReached = false;
        for (int i = 0; i < range; i++)
        {
            position += attackDirection;

            Debug.Log("Position: " + position);

            if (!TilemapExtensions.IsCellEmpty(pathfindingTilemap, position))
            {
                if (!playerReached)
                {
                    Debug.Log("SPITTER: Clearing ray because player not reached.");
                    attackPositions.Clear();
                }

                break;
            }

            attackPositions.Add(position);

            if (Vector3.Distance(position, playerPosition) < 0.05f)
            {
                playerReached = true;
            }
        }

        for (int i = 0; i < attackPositions.Count; i++)
        {
            Vector3Int tilePosition = groundTilemap.WorldToCell(attackPositions[i]);
            groundTilemap.SetTileFlags(tilePosition, TileFlags.None);
            groundTilemap.SetColor(tilePosition, attackColor);

            attackedTiles.Add(tilePosition);
            GameManager.Instance.AddAttackedTile(tilePosition);
        }
    }

    protected override bool PrepareAction()
    {
        if (attackedTiles.Count > 0)
        {
            //FaceVector(attackDirection);
            animator.SetTrigger("Attack");
            return true;
        }

        return false;
    }

    protected override void PerformAction()
    {
        for (int i = 0; i < attackedTiles.Count; i++)
        {
            DoDamageOnTile(attackedTiles[i]);
        }

        attackedTiles = new List<Vector3Int>();
    }

    protected override void Move()
    {
        lastPosition = transform.position;
    }

    protected new void Update()
    {
        if (isMyTurn)
        {
            MoveDone();
            EndTurn();
        }
    }

    protected override void _OnDeath()
    {
        for (int i = 0; i < attackedTiles.Count; i++)
        {
            ClearTile(attackedTiles[i]);
        }
        attackedTiles = new List<Vector3Int>();

        MusicManager.Instance.Play("SpitterDeath");
    }

    protected override void _OnDamage()
    {
        MusicManager.Instance.Play("SpitterAttack");
    }

    protected override void _OnAnimationEvent(string name)
    {
        switch (name)
        {
            case "RayOpen":
                break;
            case "RayClose":
                break;
            default:
                break;
        }
    }
}