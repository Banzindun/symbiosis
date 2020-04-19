using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Toolbox;

public class PlayerController : CustomMonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private float moveSpeed;

    [SerializeField] Transform movementPoint;

    [SerializeField] LayerMask collisionLayers;

    [SerializeField] GridLayout gridLayout;

    [SerializeField] int movesPerTurn = 2;

    bool isPlayerTurn;

    int currentMoves;

    // Start is called before the first frame update
    void Start()
    {
        movementPoint.parent = null;
        GetComponent<PlayerHealth>().OnDeathDelegate += OnDeath;
        GameManager.Instance.playerBehaviour = this;
    }

    private void OnDestroy()
    {
        GameManager.Instance.playerBehaviour = null;
    }

    public override void OnTurnStart()
    {

    }

    public override void OnPlayerTurn()
    {
        isPlayerTurn = true;
        currentMoves = movesPerTurn;
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

        // Handle movement
        if (Vector3.Distance(transform.position, movementPoint.position) < 0.05f)
        {
            if (currentMoves > 0)
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");

                if (Mathf.Abs(horizontal) == 1f)
                {
                    if (!Physics2D.OverlapCircle(movementPoint.position + new Vector3(horizontal, 0f, 0f), .2f, collisionLayers))
                    {
                        movementPoint.position += new Vector3(horizontal, 0f, 0f);
                        currentMoves--;

                        Vector3 angle = transform.eulerAngles;
                        if (horizontal < 0)
                        {
                            angle.y = 0;
                        }
                        else
                        {
                            angle.y = 180;
                        }

                        transform.eulerAngles = angle;
                    }
                }
                else if (Mathf.Abs(vertical) == 1f)
                {
                    if (!Physics2D.OverlapCircle(movementPoint.position + new Vector3(0f, vertical, 0f), .2f, collisionLayers))
                    {
                        movementPoint.position += new Vector3(0, vertical, 0f);
                        currentMoves--;
                    }
                }
            }
        }

        // TODO: Handle other actions

        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }
    }

    public void EndTurn()
    {
        isPlayerTurn = false;
        GameManager.Instance.OnPlayerTurnEnd();
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
        }
    }
}
