using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Toolbox;

public class ScientistController : MonsterController
{

    protected override void ScheduleAction()
    {

    }

    protected override bool PrepareAction()
    {
        return false;
    }

    protected override void PerformAction()
    {

    }

    protected override void Move()
    {
        lastPosition = transform.position;
    }

    protected override void _OnMove()
    {

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
        Vector3Int tile = pathfindingTilemap.WorldToCell(lastPosition);
        pathfindingTilemap.SetTile(tile, null);

        tile = pathfindingTilemap.WorldToCell(transform.position);
        pathfindingTilemap.SetTile(tile, null);

        gameObject.layer = 0;

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.size = new Vector2(0.5f, 0.5f);

        //collider2D.enabled = false;
        if (isMyTurn)
        {
            EndTurn();
        }
    }

    protected override void _OnDamage()
    {

    }

    protected override void _OnAnimationEvent(string name)
    {

    }
}