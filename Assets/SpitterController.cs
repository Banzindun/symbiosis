using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Toolbox;

public class SpitterController : MonsterController
{
    [SerializeField] private float range;

    private List<Vector3Int> attackedIiles = new List<Vector3Int>();

    protected override void ScheduleAction()
    {
        bool playerInRange = Vector3.Distance(transform.position, player.transform.position) < range;

        FacePlayer();

        if (!playerInRange)
        {
            return;
        }

        Vector3 playerPosition = player.transform.position;

        Vector3 addition = Vector3.zero;
        // First check around the positions 
        if (IsAbove(playerPosition, 0.05f, range))
        {
            addition = new Vector3(0, 1f, 0);
        }
        else if (IsBelow(playerPosition, 0.05f, range))
        {
            addition = new Vector3(0, -1f, 0);
        }
        else if (IsRight(playerPosition, 0.05f, range))
        {
            addition = new Vector3(1f, 0, 0);
        }
        else if (IsLeft(playerPosition, 0.05f, range))
        {
            addition = new Vector3(-1f, 0, 0);
        }
        else
        {
            Vector3[] choices = { new Vector3(1f, 0, 0), new Vector3(0, 1f, 0),
                new Vector3(-1f, 0, 0), new Vector3(0, -1f, 0) };

            int index = Random.Range(0, 4);
            addition = choices[index];
        }

        List<Vector3> attackPositions = new List<Vector3>();
        Vector3 position = transform.position;
        bool playerReached = false;
        for (int i = 0; i < range; i++)
        {
            position += addition;

            if (IsBlocked(position) || !TilemapExtensions.IsCellEmpty(pathfindingTilemap, position))
            {
                if (!playerReached)
                {
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

            attackedIiles.Add(tilePosition);
            GameManager.Instance.AddAttackedTile(tilePosition);
        }
    }


    protected override void PerformAction()
    {
        for (int i = 0; i < attackedIiles.Count; i++)
        {
            DoDamageOnTile(attackedIiles[i]);
        }

        attackedIiles = new List<Vector3Int>();
    }

    protected void Move()
    {
        lastPosition = transform.position;
    }

    protected void Update()
    {
        if (isMyTurn)
        {

            MoveDone();
            isMyTurn = false;
        }
    }
}