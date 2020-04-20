using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public enum GameState
    {
        OTHER,
        PLAYER_TURN,
        ENEMY_ATTACK,
        ENEMY_MOVE,
        PAUSED
    }

    [SerializeField] private static bool loading;

    [SerializeField]
    public GameState state;
    private GameState lastState;

    private static GameManager instance;
    public static GameManager Instance => instance;

    private List<CustomMonoBehaviour> enemyBehaviours = new List<CustomMonoBehaviour>();
    public List<CustomMonoBehaviour> EnemyBehaviours => enemyBehaviours;
    private Dictionary<Vector3Int, int> attackedTiles = new Dictionary<Vector3Int, int>();
    public CustomMonoBehaviour playerBehaviour;

    private int handledEnemyIndex = 0;

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject introMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject ui;
    [SerializeField] private GameObject dialog;
    [SerializeField] private GameObject gameOver;

    [SerializeField] private int currentLevelIndex;

    [SerializeField] private Level[] levels;

    public void AddAttackedTile(Vector3Int tile)
    {
        int count = 0;
        if (attackedTiles.ContainsKey(tile))
        {
            count = attackedTiles[tile];
        }

        attackedTiles[tile] = count + 1;
    }

    public bool RemoveAttackedTile(Vector3Int tile)
    {
        int count = 0;
        if (attackedTiles.ContainsKey(tile))
        {
            count = attackedTiles[tile];
        }

        if (count <= 1)
        {
            return attackedTiles.Remove(tile);
        }
        else
        {
            attackedTiles[tile] = count - 1;
        }

        return false;
    }

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


        dialog.SetActive(true);
        ui.SetActive(true);

        if (!loading)
        {
            mainMenu.SetActive(true);
        }
        else
        {
            StartGame();
        }
    }

    [SerializeField]
    private int turnNumber;

    public void StartGame()
    {
        Debug.Log("STARTING GAME");

        currentLevelIndex = 0;

        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        dialog.SetActive(false);
        ui.SetActive(true);

        if (loading)
        {
            // Postpone the load after we have the player etc.
        }
        else
        {
            introMenu.SetActive(true);
        }
    }

    public void OnIntroFinished()
    {
        Debug.Log("INTRO FINISHED");
        Unpause();
        StartTurn();
    }

    public void ToggleUI()
    {
        if (ui.active)
        {
            ui.SetActive(false);
        }
        else
        {
            ui.SetActive(true);
        }
    }

    public void ResetGame()
    {
        enemyBehaviours.Clear();
        attackedTiles.Clear();
        playerBehaviour = null;
        handledEnemyIndex = 0;
    }

    public void RestartGame()
    {
        Debug.Log("RESTARTING GAME");
        loading = true;
        SceneManager.LoadScene("GameScene");
    }

    public void NextLevel()
    {

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (loading && playerBehaviour != null)
        {
            OnIntroFinished();
            loading = false;
        }

        if (!mainMenu.active && !dialog.active)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
    }

    public bool IsEnemyActive()
    {
        for (int i = 0; i < enemyBehaviours.Count; i++)
        {
            if (enemyBehaviours[i].enabled)
            {
                return true;
            }
        }

        return false;
    }

    public void TogglePause()
    {
        if (state == GameState.PAUSED)
        {
            Unpause();
            pauseMenu.SetActive(false);
        }
        else
        {
            Pause();
            pauseMenu.SetActive(true);
        }
    }

    public void Pause()
    {
        lastState = state;
        state = GameState.PAUSED;

        Time.timeScale = 0;
    }

    public void Unpause()
    {
        state = lastState;
        Time.timeScale = 1;
    }

    public void StartTurn()
    {
        Debug.Log("STARTING TURN!");

        state = GameState.OTHER;

        playerBehaviour.OnTurnStart();

        foreach (CustomMonoBehaviour enemy in enemyBehaviours)
        {
            if (enemy.enabled)
            {
                enemy.OnTurnStart();
            }
        }

        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        Debug.Log("STARTING PLAYER TURN!");

        state = GameState.PLAYER_TURN;

        playerBehaviour.OnPlayerTurn();

        foreach (CustomMonoBehaviour enemy in enemyBehaviours)
        {
            if (enemy.enabled)
            {
                enemy.OnPlayerTurn();
            }
        }
    }

    public void OnPlayerTurnEnd()
    {
        Debug.Log("PLAYER TURN ENDED!");
        StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        Debug.Log("ENEMY TURN ENDED!");

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

        if (enemy.enabled)
        {
            enemy.OnEnemyAttack();
            return;
        }

        // If not enabled we just end the turn
        OnEnemyTurnEnd();
    }

    private void DoEnemyMove()
    {
        CustomMonoBehaviour enemy = enemyBehaviours[handledEnemyIndex];
        if (enemy.enabled)
        {
            enemy.OnEnemyMove();
            return;
        }

        // If not enabled we just end the turn
        OnEnemyTurnEnd();
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
            if (enemy.enabled)
            {
                enemy.OnTurnEnd();
            }
        }

        StartTurn();
    }

    public void PlayerDied()
    {
        Pause();
        gameOver.SetActive(true);
    }
}
