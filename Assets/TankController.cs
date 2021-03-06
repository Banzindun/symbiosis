using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TankController : MonsterController
{

    private List<Vector3Int> attackedTiles = new List<Vector3Int>();

    protected override void ScheduleAction()
    {
        bool playerInRange = Vector3.Distance(transform.position, player.transform.position) < 2f;

        FacePlayer();

        if (!playerInRange)
        {
            return;
        }


        Vector3 playerPosition = player.transform.position;
        List<Vector3> attackPositions = new List<Vector3>();
        attackDirection = Vector3.zero;
        if (IsAbove(playerPosition, 0.05f, 1.05f))
        {
            attackDirection = new Vector3(0, 1f, 0);
            attackPositions.Add(transform.position + new Vector3(0, 1f, 0));
            attackPositions.Add(transform.position + new Vector3(1f, 1f, 0));
            attackPositions.Add(transform.position + new Vector3(-1f, 1f, 0));
        }
        else if (IsBelow(playerPosition, 0.05f, 1.05f))
        {
            attackDirection = new Vector3(0, -1f, 0);
            attackPositions.Add(transform.position + new Vector3(0, -1f, 0));
            attackPositions.Add(transform.position + new Vector3(1f, -1f, 0));
            attackPositions.Add(transform.position + new Vector3(-1f, -1f, 0));
        }
        else if (IsRight(playerPosition, 0.05f, 1.05f))
        {
            attackDirection = new Vector3(1f, 0f, 0);
            attackPositions.Add(transform.position + new Vector3(1f, 0f, 0));
            attackPositions.Add(transform.position + new Vector3(1f, 1f, 0));
            attackPositions.Add(transform.position + new Vector3(1f, -1f, 0));
        }
        else if (IsLeft(playerPosition, 0.05f, 1.05f))
        {
            attackDirection = new Vector3(-1f, 0f, 0);
            attackPositions.Add(transform.position + new Vector3(-1f, 0f, 0));
            attackPositions.Add(transform.position + new Vector3(-1f, 1f, 0));
            attackPositions.Add(transform.position + new Vector3(-1f, -1f, 0));
        }
        // Then check the rest, starting with right/left
        else if (IsRight(playerPosition, 1.05f, 1.05f))
        {
            attackDirection = new Vector3(1f, 0f, 0);
            attackPositions.Add(transform.position + new Vector3(1f, 0f, 0));
            attackPositions.Add(transform.position + new Vector3(1f, 1f, 0));
            attackPositions.Add(transform.position + new Vector3(1f, -1f, 0));
        }
        else if (IsLeft(playerPosition, 1.05f, 1.05f))
        {
            attackDirection = new Vector3(-1f, 0f, 0);
            attackPositions.Add(transform.position + new Vector3(-1f, 0f, 0));
            attackPositions.Add(transform.position + new Vector3(-1f, 1f, 0));
            attackPositions.Add(transform.position + new Vector3(-1f, -1f, 0));
        }
        else if (IsAbove(playerPosition, 1.05f, 1.05f))
        {
            attackDirection = new Vector3(0, 1f, 0);
            attackPositions.Add(transform.position + new Vector3(0, 1f, 0));
            attackPositions.Add(transform.position + new Vector3(1f, 1f, 0));
            attackPositions.Add(transform.position + new Vector3(-1f, 1f, 0));
        }
        else if (IsBelow(playerPosition, 1.05f, 1.05f))
        {
            attackDirection = new Vector3(0, -1f, 0);
            attackPositions.Add(transform.position + new Vector3(0, -1f, 0));
            attackPositions.Add(transform.position + new Vector3(1f, -1f, 0));
            attackPositions.Add(transform.position + new Vector3(-1f, -1f, 0));
        }
        else
        {
            Debug.LogError("Didn't find player around me but I should be close enough!");
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

    protected override void _OnMove()
    {
        MusicManager.Instance.Play("TankMove");
    }


    protected override bool PrepareAction()
    {
        if (attackedTiles.Count > 0)
        {
            FaceVector(attackDirection);

            if (attackDirection.y == 0)
            {
                animator.SetTrigger("Attack");
            }
            else
            {
                if (attackDirection.y > 0)
                {
                    animator.SetTrigger("AttackUp");
                }
                else
                {
                    animator.SetTrigger("AttackDown");
                }
            }

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

    protected override void _OnDeath()
    {
        for (int i = 0; i < attackedTiles.Count; i++)
        {
            ClearTile(attackedTiles[i]);
        }
        attackedTiles = new List<Vector3Int>();

        MusicManager.Instance.Play("TankDeath");
    }

    protected override void _OnDamage()
    {
        MusicManager.Instance.Play("TankAttack");
    }

    protected override void _OnAnimationEvent(string name)
    {
        switch (name)
        {
            default:

                break;
        }
    }
}