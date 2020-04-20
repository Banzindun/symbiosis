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
        if (IsAbove(playerPosition, 0.05f, 1.05f))
        {
            Debug.Log("Above!");
            attackPosition = transform.position + new Vector3(0, 1f, 0);
        }
        else if (IsBelow(playerPosition, 0.05f, 1.05f))
        {
            Debug.Log("Below!");
            attackPosition = transform.position + new Vector3(0, -1f, 0);
        }
        else if (IsRight(playerPosition, 0.05f, 1.05f))
        {
            Debug.Log("Right!");
            attackPosition = transform.position + new Vector3(1f, 0, 0);
        }
        else if (IsLeft(playerPosition, 0.05f, 1.05f))
        {
            Debug.Log("Left!");
            attackPosition = transform.position + new Vector3(-1f, 0, 0);
        }
        else
        {
            Debug.LogError("Didn't find player around me but I should be close enough!");
            return;
        }

        Debug.Log("Attacking " + attackPosition);
        tilePosition = groundTilemap.WorldToCell(attackPosition);
        Debug.Log("Attacking tile " + tilePosition);
        groundTilemap.SetTileFlags(tilePosition, TileFlags.None);
        groundTilemap.SetColor(tilePosition, attackColor);
        GameManager.Instance.AddAttackedTile(tilePosition);
    }

    protected override bool PrepareAction()
    {
        if (tilePosition.x != -1000)
        {
            animator.SetTrigger("Attack");
            return true;
        }

        return false;
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
}