using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using Toolbox;

public class SoldierController : MonsterController
{

    private Vector3Int tilePosition = new Vector3Int(-1000, 0, 0);

    protected override void ScheduleAction()
    {
        bool playerInRange = Vector3.Distance(transform.position, player.transform.position) < 1.05f;

        FacePlayer();

        if (!playerInRange)
        {
            return;
        }

        Vector3 playerPosition = player.transform.position;
        Vector3 attackPosition;
        attackDirection = Vector3.zero;
        if (IsAbove(playerPosition, 0.05f, 1.05f))
        {
            attackDirection = new Vector3(0, 1f, 0);
        }
        else if (IsBelow(playerPosition, 0.05f, 1.05f))
        {
            attackDirection = new Vector3(0, -1f, 0);
        }
        else if (IsRight(playerPosition, 0.05f, 1.05f))
        {
            attackDirection = new Vector3(1f, 0, 0);
        }
        else if (IsLeft(playerPosition, 0.05f, 1.05f))
        {
            attackDirection = new Vector3(-1f, 0, 0);
        }
        else
        {
            Debug.LogError("Didn't find player around me but I should be close enough!");
            return;
        }

        attackPosition = transform.position + attackDirection;

        tilePosition = groundTilemap.WorldToCell(attackPosition);
        groundTilemap.SetTileFlags(tilePosition, TileFlags.None);
        groundTilemap.SetColor(tilePosition, attackColor);
        GameManager.Instance.AddAttackedTile(tilePosition);
    }

    protected override bool PrepareAction()
    {
        if (tilePosition.x != -1000)
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

    protected override void _OnMove()
    {
        MusicManager.Instance.Play("SoldierMove");
    }

    protected override void PerformAction()
    {
        if (tilePosition.x != -1000)
        {
            DoDamageOnTile(tilePosition);
        }

        tilePosition = new Vector3Int(-1000, 0, 0);
    }

    protected override void _OnDeath()
    {
        if (tilePosition.x != -1000)
        {
            ClearTile(tilePosition);
            tilePosition = new Vector3Int(-1000, 0, 0);
        }

        MusicManager.Instance.Play("SoldierDeath");
    }

    protected override void _OnDamage()
    {
        MusicManager.Instance.Play("SoldierAttack");
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