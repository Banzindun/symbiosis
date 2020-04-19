using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Toolbox;

public class PlayerController : CustomMonoBehaviour
{
    [System.Serializable]
    public enum ActionType
    {
        ATTACK,
        FEED,
        SHOOT,
        BOMB,
        NONE
    }

    [SerializeField] private Animator animator;
    [SerializeField] Transform movementPoint;
    [SerializeField] LayerMask collisionLayers;
    [SerializeField] LayerMask actionDirectionCollisionLayers;
    [SerializeField] GridLayout gridLayout;
    private PlayerHealth health;

    [Header("Constants:")]
    [SerializeField] private float moveSpeed;
    [SerializeField] int movesPerTurn = 2;
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private int bombCooldown = 20;
    private int currentBombCooldown;
    [SerializeField] private int shootCooldown = 12;
    private int currentShootCooldown;

    [Header("UI:")]
    [SerializeField] private Button nextTurnButton;
    [SerializeField] private SlotHolder attackSlotHolder;
    [SerializeField] private SlotHolder shootSlotHolder;
    [SerializeField] private SlotHolder bombSlotHolder;
    [SerializeField] private SlotHolder feedSlotHolder;

    [Header("Debug:")]
    [SerializeField] private ActionType performedAction;
    [SerializeField] private Vector3 actionDirection;

    bool isPlayerTurn;

    int currentMoves;

    private bool bombAvailable;
    private bool shootAvailable;

    private float energy;
    public float Energy
    {
        get => energy;
        set
        {
            energy = value;

            if (energy > 1)
            {
                energy = 1;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        movementPoint.parent = null;

        health = GetComponent<PlayerHealth>();
        health.OnDeathDelegate += OnDeath;

        GameManager.Instance.playerBehaviour = this;

        // TODO: Uncomment when ready
        // currentBombCooldown = bombCooldown;
        // currentShootCooldown = shootCooldown;

        // TODO: Comment when ready
        bombAvailable = true;
        shootAvailable = true;
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

        currentShootCooldown--;
        if (currentShootCooldown == 0)
        {
            shootAvailable = true;
        }

        UpdateUI();
    }

    public override void OnPlayerTurn()
    {
        nextTurnButton.interactable = true;
        isPlayerTurn = true;
        currentMoves = movesPerTurn;

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
        transform.position = Vector3.MoveTowards(transform.position, movementPoint.position, moveSpeed * Time.deltaTime);

        if (!isPlayerTurn || GameManager.Instance.state == GameManager.GameState.PAUSED)
        {
            return;
        }

        if (performedAction == ActionType.NONE)
        {
            bool movementReceived = false;
            // Handle movement
            if (Vector3.Distance(transform.position, movementPoint.position) < 0.05f)
            {
                movementReceived = HandleMovementInput();
            }

            if (!movementReceived)
            {
                HandleActionInput();
            }

            UpdateUI();
        }
        else
        {
            if (actionDirection == Vector3.zero)
            {
                HandleActionDirectionInput();
            }
            else
            {
                bool actionPerformed = PerformAction();

                if (actionPerformed)
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

    private void UpdateUI()
    {
        attackSlotHolder.Toggle(IsActionAvailable(ActionType.ATTACK));
        shootSlotHolder.Toggle(IsActionAvailable(ActionType.SHOOT));
        bombSlotHolder.Toggle(IsActionAvailable(ActionType.BOMB));
        feedSlotHolder.Toggle(IsActionAvailable(ActionType.FEED));

        bombSlotHolder.UpdateCooldown(currentBombCooldown);
        shootSlotHolder.UpdateCooldown(currentShootCooldown);

        // TODO: Update energy
        // TODO: Update health
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
                if (!Physics2D.OverlapCircle(movementPoint.position + new Vector3(horizontal, 0f, 0f), .2f, collisionLayers))
                {
                    movementPoint.position += new Vector3(horizontal, 0f, 0f);
                    currentMoves--;
                    FaceVector(new Vector3(horizontal, 0f, 0f));
                    return true;
                }
            }
            else if (vertical != 0)
            {
                if (!Physics2D.OverlapCircle(movementPoint.position + new Vector3(0f, vertical, 0f), .2f, collisionLayers))
                {
                    movementPoint.position += new Vector3(0, vertical, 0f);
                    currentMoves--;

                    return true;
                }
            }
        }

        return false;
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

    private void HandleActionInput()
    {
        if (IsActionAvailable(ActionType.ATTACK) && Input.GetKeyDown(KeyCode.E))
        {
            performedAction = ActionType.ATTACK;
        }
        else if (IsActionAvailable(ActionType.FEED) && Input.GetKeyDown(KeyCode.F))
        {
            performedAction = ActionType.FEED;

            // TODO: Play the feed animation, when done end the turn
            // TODO: Hide the body in the middle of the animation after the bite phase
            MonsterController enemyController = Utils.GetComponentAtPosition2D<MonsterController>(transform.position);

            float healthGain = enemyController.Heal;
            health.CurrentValue += healthGain;
            Energy += enemyController.Nutrition;

            Destroy(enemyController.gameObject);

            // TODO; Do after animation done
            EndTurn();
            return;
        }
        else if (IsActionAvailable(ActionType.SHOOT) && Input.GetKeyDown(KeyCode.R))
        {
            performedAction = ActionType.SHOOT;
        }
        else if (IsActionAvailable(ActionType.BOMB) && Input.GetKeyDown(KeyCode.G))
        {
            performedAction = ActionType.BOMB;
        }

        // TODO: Show the tooltip 
        ShowDirectionSelectionTooltip();
    }

    private void ShowDirectionSelectionTooltip()
    {
        Vector3Int[] directions = Utils.FourDirections;

        for (int i = 0; i < directions.Length; i++)
        {
            if (Physics2D.OverlapCircle(transform.position + directions[i], .2f, actionDirectionCollisionLayers))
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
        switch (type)
        {
            case ActionType.ATTACK:
                return !AllFourDirectionsBlocked();
                break;
            case ActionType.BOMB:
                return bombAvailable;
            case ActionType.FEED:
                MonsterController enemyController = Utils.GetComponentAtPosition2D<MonsterController>(transform.position);
                return enemyController != null;
            case ActionType.SHOOT:
                return shootAvailable;
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
        else if (performedAction == ActionType.SHOOT)
        {
            FaceVector(actionDirection);

            // TODO: Play that shoot animation when the projectile hits, then next turn

            shootAvailable = false;
            currentShootCooldown = shootCooldown;
            return true;
        }
        else if (performedAction == ActionType.BOMB)
        {
            FaceVector(actionDirection);
            // TODO: Place the bomb directly from here

            bombAvailable = false;
            currentBombCooldown = bombCooldown;
            return true;
        }

        return false;
    }

    public void EndTurn()
    {
        if (isPlayerTurn)
        {
            isPlayerTurn = false;
            nextTurnButton.interactable = false;
            GameManager.Instance.OnPlayerTurnEnd();
        }
    }

    public void OnDeath()
    {
        animator.SetTrigger("Die");
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
        }
    }
}
