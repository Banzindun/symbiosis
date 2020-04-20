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
        MusicManager.Instance.Play("SpitterDeath");
    }

    protected override void _OnDamage()
    {

    }

    protected override void _OnAnimationEvent(string name)
    {

    }
}