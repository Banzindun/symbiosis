using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public enum GameState
    {
        OTHER,
        PLAYER_TURN,
        ENEMY_ATTACK,
        ENEMY_MOVE
    }

    [SerializeField]
    private GameState state;

    private static GameManager instance;
    public static GameManager Instance => instance;

    private List<CustomMonoBehaviour> enemyBehaviours = new List<CustomMonoBehaviour>();

    public List<CustomMonoBehaviour> EnemyBehaviours => enemyBehaviours;

    public CustomMonoBehaviour playerBehaviour;

    private int handledEnemyIndex = 0;

    public void RegisterEnemy(CustomMonoBehaviour enemy)
    {
        enemyBehaviours.Add(enemy);
    }

    public void RemoveEnemy(CustomMonoBehaviour enemy)
    {
        enemyBehaviours.Remove(enemy);
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    [SerializeField]
    private int turnNumber;

    // Start is called before the first frame update
    void Start()
    {
        StartTurn();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartTurn()
    {
        Debug.Log("Starting turn!");

        state = GameState.OTHER;

        playerBehaviour.OnTurnStart();

        foreach (CustomMonoBehaviour enemy in enemyBehaviours)
        {
            enemy.OnTurnStart();
        }

        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        Debug.Log("Starting player turn!");

        state = GameState.PLAYER_TURN;

        playerBehaviour.OnPlayerTurn();

        foreach (CustomMonoBehaviour enemy in enemyBehaviours)
        {
            enemy.OnPlayerTurn();
        }
    }

    public void OnPlayerTurnEnd()
    {
        Debug.Log("Player turn ended!");
        StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        Debug.Log("Starting enemy turn!");

        handledEnemyIndex = 0;
        state = GameState.ENEMY_ATTACK;

        playerBehaviour.OnEnemyAttack();

        // Sort enemies by distance from player
        enemyBehaviours.Sort(delegate (CustomMonoBehaviour a, CustomMonoBehaviour b)
        {
            return Vector3.Distance(a.transform.position, playerBehaviour.transform.position)
            .CompareTo(
                Vector3.Distance(b.transform.position, playerBehaviour.transform.position));
        });

        DoEnemyAttack();
    }

    private void DoEnemyAttack()
    {
        CustomMonoBehaviour enemy = enemyBehaviours[handledEnemyIndex];
        enemy.OnEnemyAttack();
    }

    private void DoEnemyMove()
    {
        CustomMonoBehaviour enemy = enemyBehaviours[handledEnemyIndex];
        enemy.OnEnemyMove();
    }

    public void OnEnemyTurnEnd()
    {
        handledEnemyIndex++;

        if (handledEnemyIndex == enemyBehaviours.Count)
        {
            if (state == GameState.ENEMY_ATTACK)
            {
                handledEnemyIndex = 0;
                state = GameState.ENEMY_MOVE;
                playerBehaviour.OnEnemyMove();
            }
            else
            {
                handledEnemyIndex = 0;
                OnEnemiesTurnEnd();
                return;
            }
        }

        if (state == GameState.ENEMY_ATTACK)
        {
            DoEnemyAttack();
        }
        else
        {
            DoEnemyMove();
        }
    }

    private void OnEnemiesTurnEnd()
    {
        Debug.Log("Enemies turn ended.");
        handledEnemyIndex = 0;
        EndTurn();
    }

    public void EndTurn()
    {
        Debug.Log("Ending turn.");
        state = GameState.OTHER;

        playerBehaviour.OnTurnEnd();

        foreach (CustomMonoBehaviour enemy in enemyBehaviours)
        {
            enemy.OnTurnEnd();
        }

        StartTurn();

    }

    public void PlayerDied()
    {

    }
}
