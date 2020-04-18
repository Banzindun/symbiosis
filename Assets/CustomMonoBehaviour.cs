using UnityEngine;

public abstract class CustomMonoBehaviour : MonoBehaviour
{
    public abstract void OnTurnStart();
    public abstract void OnPlayerTurn();
    public abstract void OnEnemyAttack();
    public abstract void OnEnemyMove();
    public abstract void OnTurnEnd();
}