using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Toolbox;
using TMPro;

public class PlayerController : CustomMonoBehaviour
{
    [System.Serializable]
    public enum ActionType
    {
        ATTACK,
        FEED,
        BOMB,
        NONE
    }

    [SerializeField] private Animator animator;
    [SerializeField] Transform movementPoint;
    [SerializeField] LayerMask collisionLayers;
    [SerializeField] LayerMask actionDirectionCollisionLayers;
    [SerializeField] LayerMask deadEnemyCollisionLayer;
    [SerializeField] GridLayout gridLayout;
    private PlayerHealth health;

    [SerializeField] private GameObject minePrefab;

    [Header("Constants:")]

    [SerializeField] private float moveSpeed;
    [SerializeField] int movesPerTurn = 2;
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private int bombCooldown = 20;
    private int currentBombCooldown;

    [SerializeField] private int eatenEnemiesToWinGame = 1;
    [SerializeField] private int maxEatenEnemies;


    [Header("UI:")]
    [SerializeField] private Button nextTurnButton;
    [SerializeField] private SlotHolder attackSlotHolder;
    [SerializeField] private SlotHolder bombSlotHolder;
    [SerializeField] private SlotHolder feedSlotHolder;
    [SerializeField] private Image healthIndicator;
    [SerializeField] private TextMeshProUGUI healthValue;
    [SerializeField] private Image feastIndicator;
    [SerializeField] private TextMeshProUGUI feastValue;
    [SerializeField] private Image feastPointIndicator;
    [SerializeField] private TextMeshProUGUI stepsText;

    [SerializeField] private Button wButton;
    [SerializeField] private Button sButton;
    [SerializeField] private Button aButton;
    [SerializeField] private Button dButton;


    [Header("Debug:")]
    [SerializeField] private ActionType performedAction;
    [SerializeField] private Vector3 actionDirection;
    [SerializeField] private bool inAir;
    [SerializeField] private bool unlimitedMovement;

    bool isPlayerTurn;

    int currentMoves;

    private bool bombAvailable;

    private bool performingAction = false;

    [SerializeField]
    private int eatenEnemies;

    // Start is called before the first frame update
    void Start()
    {
        movementPoint.parent = null;

        health = GetComponent<PlayerHealth>();
        health.OnDeathDelegate += OnDeath;

        GameManager.Instance.playerBehaviour = this;
        currentBombCooldown = bombCooldown;
    }

    private void OnDestroy()
    {
        GameManager.Instance.playerBehaviour = null;
    }

    public override void OnTurnStart()
    {
        currentBombCooldown--;
        if (currentBombCooldown == 0)
        {
            bombAvailable = true;
        }

        UpdateUI();
    }

    public override void OnPlayerTurn()
    {
        nextTurnButton.interactable = true;
        isPlayerTurn = true;

        currentMoves = movesPerTurn;

        bool someEnemiesActive = GameManager.Instance.IsEnemyActive();
        if (someEnemiesActive)
        {
            unlimitedMovement = false;
        }
        else
        {
            unlimitedMovement = true;
        }

        performedAction = ActionType.NONE;
        actionDirection = Vector3.zero;
    }

    public override void OnEnemyAttack()
    {

    }

    public override void OnEnemyMove()
    {

    }

    public override void OnTurnEnd()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (inAir)
        {
            transform.position = Vector3.MoveTowards(transform.position, movementPoint.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, movementPoint.position) < 0.05f)
            {
                inAir = false;
                if (unlimitedMovement)
                {
                    unlimitedMovement = !GameManager.Instance.IsEnemyActive();
                }
                else
                {
                    currentMoves--;
                    if (currentMoves <= 0)
                    {
                        EndTurn();
                    }
                }
            }

            return;
        }

        if (currentMoves <= 0 || !isPlayerTurn || GameManager.Instance.state != GameManager.GameState.PLAYER_TURN)
        {
            return;
        }

        if (performedAction == ActionType.NONE)
        {
            // Handle movement
            bool movementReceived = false;

            if (Vector3.Distance(transform.position, movementPoint.position) < 0.05f)
            {
                movementReceived = HandleMovementInput();
            }

            if (movementReceived)
            {
                HandleMovementReceived();
            }
            else
            {
                HandleActionInput();
            }

            UpdateUI();
        }
        else if (!performingAction)
        {
            if (actionDirection == Vector3.zero)
            {
                HandleActionDirectionInput();
            }
            else
            {
                performingAction = PerformAction();

                if (performingAction)
                {
                    UpdateUI();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }
    }

    private void HandleMovementReceived()
    {
        inAir = false;
        animator.SetTrigger("Jump");
        MusicManager.Instance.Play("PlayerMove");
    }

    private void UpdateUI()
    {
        attackSlotHolder.Toggle(IsActionAvailable(ActionType.ATTACK));
        bombSlotHolder.Toggle(IsActionAvailable(ActionType.BOMB));
        feedSlotHolder.Toggle(IsActionAvailable(ActionType.FEED));

        bombSlotHolder.UpdateCooldown(currentBombCooldown);

        healthIndicator.fillAmount = health.CurrentValue;
        healthValue.text = (int)(health.CurrentValue * 100f) + "";

        feastIndicator.fillAmount = Mathf.Clamp((float)eatenEnemies / maxEatenEnemies, 0f, 1f);
        feastValue.text = eatenEnemies + "";
        feastPointIndicator.fillAmount = (float)eatenEnemiesToWinGame / maxEatenEnemies;

        if (unlimitedMovement)
        {
            stepsText.text = "-";
        }
        else
        {
            stepsText.text = currentMoves + "";
        }

        // When selecting action
        if (performedAction != ActionType.NONE && actionDirection == Vector3.zero)
        {
            wButton.interactable = HandleDirectionSelect(new Vector3(0f, 1f, 0f));
            sButton.interactable = HandleDirectionSelect(new Vector3(0f, -1f, 0f));
            aButton.interactable = HandleDirectionSelect(new Vector3(-1f, 0f, 0f));
            dButton.interactable = HandleDirectionSelect(new Vector3(1f, 0f, 0f));
            actionDirection = Vector3.zero;
        }
        else
        {
            wButton.interactable = IsMoveAvailable(new Vector3(0f, 1f, 0f));
            sButton.interactable = IsMoveAvailable(new Vector3(0f, -1f, 0f));
            aButton.interactable = IsMoveAvailable(new Vector3(-1f, 0f, 0f));
            dButton.interactable = IsMoveAvailable(new Vector3(1f, 0f, 0f));
        }
    }

    private bool HandleMovementInput()
    {
        if (currentMoves > 0)
        {
            float horizontal = 0f;
            if (Input.GetKeyDown(KeyCode.A))
            {
                horizontal = -1f;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                horizontal = 1f;
            }

            float vertical = 0f;
            if (Input.GetKeyDown(KeyCode.S))
            {
                vertical = -1f;
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                vertical = 1f;
            }

            if (horizontal != 0)
            {
                return MoveHorizontal(horizontal);
            }
            else if (vertical != 0)
            {
                return MoveVertical(vertical);
            }
        }

        return false;
    }

    public void MoveHorizontalUI(float horizontal)
    {
        if (inAir || !isPlayerTurn || GameManager.Instance.state == GameManager.GameState.PAUSED)
        {
            return;
        }

        if (performedAction == ActionType.NONE)
        {
            MoveHorizontal(horizontal);
            HandleMovementReceived();
        }
        else if (actionDirection == Vector3.zero)
        {
            HandleDirectionSelect(new Vector3(horizontal, 0f, 0f));
        }
    }

    protected bool MoveHorizontal(float horizontal)
    {
        if (IsMoveAvailable(new Vector3(horizontal, 0f, 0f)))
        {
            movementPoint.position += new Vector3(horizontal, 0f, 0f);
            FaceVector(new Vector3(horizontal, 0f, 0f));
            return true;
        }

        return false;
    }

    public void MoveVerticalUI(float vertical)
    {
        if (inAir || !isPlayerTurn || GameManager.Instance.state == GameManager.GameState.PAUSED)
        {
            return;
        }

        if (performedAction == ActionType.NONE)
        {
            MoveVertical(vertical);
            HandleMovementReceived();
        }
        else if (actionDirection == Vector3.zero)
        {
            HandleDirectionSelect(new Vector3(0f, vertical, 0f));
        }
    }

    protected bool MoveVertical(float vertical)
    {
        if (IsMoveAvailable(new Vector3(0f, vertical, 0f)))
        {
            movementPoint.position += new Vector3(0, vertical, 0f);
            return true;
        }
        return false;
    }

    public bool IsMoveAvailable(Vector3Int move)
    {
        return IsMoveAvailable(new Vector3((float)move.x, (float)move.y, (float)move.z));
    }

    public bool IsMoveAvailable(Vector3 move)
    {
        if (currentMoves <= 0 || inAir || !isPlayerTurn || GameManager.Instance.state == GameManager.GameState.PAUSED)
        {
            return false;
        }

        return !Physics2D.OverlapCircle(movementPoint.position + move, .2f, collisionLayers);
    }

    private void FaceVector(Vector3 position)
    {
        Vector3 angle = transform.eulerAngles;
        if (position.x < 0)
        {
            angle.y = 0;
        }
        else if (position.x > 0)
        {
            angle.y = 180;
        }

        transform.eulerAngles = angle;
    }

    public void HandleActionInputUI(string actionType)
    {
        ActionType type = ActionType.NONE;
        switch (actionType)
        {
            case "Attack":
                type = ActionType.ATTACK;
                break;
            case "Feed":
                type = ActionType.FEED;
                break;
            case "Bomb":
                type = ActionType.BOMB;
                break;
        }

        if (IsActionAvailable(type))
        {
            performedAction = type;
            if (type == ActionType.FEED)
            {
                PerformFeedAction();
                return;
            }
        }

        // TODO: Show the tooltip 
        ShowDirectionSelectionTooltip();

        UpdateUI();
    }


    private void HandleActionInput()
    {
        if (IsActionAvailable(ActionType.ATTACK) && Input.GetKeyDown(KeyCode.E))
        {
            performedAction = ActionType.ATTACK;
        }
        else if (IsActionAvailable(ActionType.FEED) && Input.GetKeyDown(KeyCode.F))
        {
            performedAction = ActionType.FEED;
            PerformFeedAction();
            return;
        }
        else if (IsActionAvailable(ActionType.BOMB) && Input.GetKeyDown(KeyCode.R))
        {
            performedAction = ActionType.BOMB;
        }

        // TODO: Show the tooltip 
        ShowDirectionSelectionTooltip();
    }

    private void PerformFeedAction()
    {
        animator.SetTrigger("Feed");
        MusicManager.Instance.Play("PlayerFeed");
    }

    private void ShowDirectionSelectionTooltip()
    {
        Vector3Int[] directions = Utils.FourDirections;

        for (int i = 0; i < directions.Length; i++)
        {
            if (!IsMoveAvailable(directions[i]))
            {
                // TODO: Mark as unavailable
                continue;
            }

            // TODO: Mark as available
        }
    }

    private bool AllFourDirectionsBlocked()
    {
        Vector3Int[] directions = Utils.FourDirections;

        for (int i = 0; i < directions.Length; i++)
        {
            if (!Physics2D.OverlapCircle(transform.position + directions[i], .2f, actionDirectionCollisionLayers))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsActionAvailable(ActionType type)
    {
        if (currentMoves <= 0 || inAir || performingAction || performedAction != ActionType.NONE)
        {
            return false;
        }

        switch (type)
        {
            case ActionType.ATTACK:
                return !AllFourDirectionsBlocked();
                break;
            case ActionType.BOMB:
                return bombAvailable;
            case ActionType.FEED:
                {
                    Collider2D enemyCollider = Physics2D.OverlapCircle(transform.position, .2f, deadEnemyCollisionLayer); // Default layer
                    if (enemyCollider != null)
                    {
                        MonsterController controller = enemyCollider.GetComponent<MonsterController>();
                        return controller != null;
                    }



                    return false;
                }
            default:
                break;
        }

        return false;
    }

    private void HandleActionDirectionInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = Vector3.zero;
        if (horizontal == 1f)
        {
            direction = new Vector3(1f, 0, 0);
        }
        else if (horizontal == -1f)
        {
            direction = new Vector3(-1f, 0, 0);
        }
        else if (vertical == 1f)
        {
            direction = new Vector3(0, 1f, 0);
        }
        else if (vertical == -1f)
        {
            direction = new Vector3(0, -1f, 0);
        }

        if (direction != Vector3.zero)
        {
            HandleDirectionSelect(direction);
        }
    }

    private bool HandleDirectionSelect(Vector3 direction)
    {
        if (Physics2D.OverlapCircle(transform.position + direction, .2f, actionDirectionCollisionLayers))
        {
            return false;
        }

        actionDirection = direction;
        return true;
    }

    private bool PerformAction()
    {

        if (performedAction == ActionType.ATTACK)
        {
            FaceVector(actionDirection);

            // TODO: Play the attack animation, during it damage the opponent, when done finish turn
            if (actionDirection.y == 0)
            {
                animator.SetTrigger("Attack");
            }
            else
            {
                if (actionDirection.y > 0)
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
        else if (performedAction == ActionType.BOMB)
        {
            FaceVector(actionDirection);

            animator.SetTrigger("LayMine");
            return true;
        }

        return false;
    }

    public void EndTurn()
    {
        if (isPlayerTurn && GameManager.Instance.state == GameManager.GameState.PLAYER_TURN)
        {
            performingAction = false;
            performedAction = ActionType.NONE;
            isPlayerTurn = false;
            nextTurnButton.interactable = false;
            GameManager.Instance.OnPlayerTurnEnd();
        }
    }

    public void OnDeath()
    {
        animator.SetTrigger("Die");
    }

    private void HandleFeed()
    {
        Collider2D enemyCollider = Physics2D.OverlapCircle(transform.position, .2f, deadEnemyCollisionLayer);

        if (enemyCollider != null)
        {
            MonsterController enemyController = enemyCollider.GetComponent<MonsterController>();

            if (enemyController != null)
            {
                // TODO: Hide the body in the middle of the animation after the bite phase
                //MonsterController enemyController = Utils.GetComponentAtPosition2D<MonsterController>(transform.position);

                float healthGain = enemyController.Heal;
                health.CurrentValue += healthGain;
                eatenEnemies++;

                Destroy(enemyController.gameObject);
                EndTurn();
                return;
            }
        }

        Debug.LogError("Somehow tried to consume enemy that wasn't there (no collider or enemyController found under the player)!");
        EndTurn();
    }

    public override void OnAnimationEvent(string name)
    {
        switch (name)
        {
            case "Died":
                GameManager.Instance.PlayerDied();
                break;
            case "Hit":
                {
                    MusicManager.Instance.Play("PlayerAttack");

                    Vector3 strikePosition = transform.position + actionDirection;
                    Health enemyHealth = Utils.GetComponentAtPosition2D<Health>(strikePosition);

                    if (enemyHealth != null)
                    {
                        enemyHealth.CurrentValue -= attackDamage;
                    }
                }
                break;
            case "AttackDone":
                EndTurn();
                break;
            case "FeedDone":
                HandleFeed();
                break;
            case "Air":
                inAir = true;
                break;
            case "MinePlaced":
                {
                    GameObject mine = Instantiate(minePrefab);
                    mine.transform.position = transform.position + actionDirection;

                    bombAvailable = false;
                    currentBombCooldown = bombCooldown;
                    EndTurn();
                }
                break;
        }
    }

    public bool HasWon()
    {
        return eatenEnemies >= eatenEnemiesToWinGame;
    }
}
