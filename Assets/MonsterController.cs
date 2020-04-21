using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Toolbox;

public abstract class MonsterController : CustomMonoBehaviour
{
    [SerializeField] protected Animator animator;

    [SerializeField] protected float movementTime;
    [SerializeField] protected float damage;
    [SerializeField] protected Tile occupiedTile;
    protected float currentTime;

    [SerializeField] private Vector3 nextPosition;
    [SerializeField] protected Vector3 lastPosition;

    [SerializeField] protected Tilemap pathfindingTilemap;
    [SerializeField] protected Tilemap groundTilemap;

    [SerializeField] LayerMask collisionLayers;

    [SerializeField] protected Color attackColor;

    [SerializeField] private int moves;
    [SerializeField] private int currentMove;

    protected CustomMonoBehaviour player;
    protected BoxCollider2D collider2D;
    protected Vector3 attackDirection;

    private Health health;

    protected bool isMyTurn;
    protected bool moveDone;

    public float Heal;

    [SerializeField] private bool inAir;

    protected void OnDestroy()
    {
        Vector3Int tile = pathfindingTilemap.WorldToCell(lastPosition);
        pathfindingTilemap.SetTile(tile, null);

        tile = pathfindingTilemap.WorldToCell(transform.position);
        pathfindingTilemap.SetTile(tile, null);

        GameManager.Instance.RemoveEnemy(this);
    }

    public override void OnTurnStart()
    {

    }

    public override void OnPlayerTurn()
    {

    }

    public override void OnEnemyAttack()
    {
        isMyTurn = true;
        bool actionPrepared = PrepareAction();

        if (!actionPrepared)
        {
            EndTurn();
        }
    }

    public override void OnEnemyMove()
    {
        moveDone = false;
        currentMove = moves;
        isMyTurn = true;
        Move();
    }

    public override void OnTurnEnd()
    {

    }

    protected abstract void ScheduleAction();
    protected abstract bool PrepareAction();
    protected abstract void PerformAction();

    // Start is called before the first frame update
    protected void Start()
    {
        //movementPoint.parent = null;
        GameManager.Instance.RegisterEnemy(this);
        lastPosition = transform.position;
        nextPosition = transform.position;

        health = GetComponent<Health>();
        health.OnDeathDelegate += OnDeath;

        collider2D = GetComponent<BoxCollider2D>();
    }

    protected void Update()
    {
        if (inAir)
        {
            currentTime += Time.deltaTime / movementTime;
            transform.position = Vector3.Lerp(lastPosition, nextPosition, currentTime);
        }

        if (!isMyTurn || GameManager.Instance.state != GameManager.GameState.ENEMY_MOVE)
        {
            return;
        }

        //transform.position = Vector3.MoveTowards(transform.position, movementPoint.position, moveSpeed * Time.deltaTime);

        if (isMyTurn && !moveDone)
        {
            if (Vector3.Distance(transform.position, nextPosition) < 0.05f)
            {
                MoveDone();
            }
        }
    }

    protected void MoveDone()
    {
        if (currentMove <= 0)
        {
            ScheduleAction();
            EndTurn();
            moveDone = true;
        }
        else
        {
            Move();
        }
    }


    protected void UpdateRotation()
    {
        float horizontal = nextPosition.x - lastPosition.x;

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

    protected void FacePlayer()
    {
        float horizontal = player.transform.position.x - transform.position.x;

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

    protected virtual void Move()
    {
        nextPosition = DoPathfinding(nextPosition);
        currentMove--;

        if (Vector3.Distance(nextPosition, lastPosition) > 0.05f)
        {

            inAir = false;
            animator.SetTrigger("Jump");
            _OnMove();

            Vector3Int tile = pathfindingTilemap.WorldToCell(lastPosition);
            pathfindingTilemap.SetTile(tile, null);

            tile = pathfindingTilemap.WorldToCell(nextPosition);
            pathfindingTilemap.SetTile(tile, occupiedTile);
        }
        else
        {
            currentMove = 0;
            MoveDone();
        }

        lastPosition = transform.position;
        currentTime = 0;
        UpdateRotation();
    }

    protected abstract void _OnMove();

    protected Vector3 DoPathfinding(Vector3 oldPosition)
    {
        player = GameManager.Instance.playerBehaviour;

        Debug.Log("Finding path between " + transform.position + " and " + player.transform.position);
        Vector3 playerPosition = player.transform.position;

        List<Vector3> trialPositions = new List<Vector3>();
        trialPositions.Add(playerPosition + new Vector3(1, 0, 0));
        trialPositions.Add(playerPosition + new Vector3(0, 1, 0));
        trialPositions.Add(playerPosition + new Vector3(-1, 0, 0));
        trialPositions.Add(playerPosition + new Vector3(0, -1, 0));

        trialPositions.Sort(delegate (Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, transform.position)
            .CompareTo(
                Vector3.Distance(b, transform.position));
        });

        List<Vector3> advancedPositions = new List<Vector3>();
        advancedPositions.Add(playerPosition + new Vector3(1, 1, 0));
        advancedPositions.Add(playerPosition + new Vector3(-1, 1, 0));
        advancedPositions.Add(playerPosition + new Vector3(-1, 1, 0));
        advancedPositions.Add(playerPosition + new Vector3(-1, -1, 0));

        for (int i = 0; i < 4; i++)
        {
            if (IsOccupied(trialPositions[i]))
            {
                continue;
            }

            List<Vector3> path = AStar.FindPath(pathfindingTilemap, transform.position, trialPositions[i]);

            if (path == null)
            {
                // No path returned, stay
                return transform.position;
            }

            return GetFirstTile(path);
        }

        for (int i = 0; i < 4; i++)
        {
            if (IsOccupied(advancedPositions[i]))
            {
                continue;
            }

            List<Vector3> path = AStar.FindPath(pathfindingTilemap, transform.position, advancedPositions[i]);
            if (path == null)
            {
                // No path returned, stay
                return transform.position;
            }

            return GetFirstTile(path);
        }

        return oldPosition;
    }

    protected Vector3 GetFirstTile(List<Vector3> path)
    {
        Vector3 result = Vector3.zero;

        // In case its not line 
        if (path.Count > 1)
        {
            // Check x line
            if (path[0].x == path[1].x)
            {
                result = path[0];
                result.y = result.y - Mathf.Sign(path[0].y - path[1].y);
                return result;
            }

            // Check y line
            if (path[0].y == path[1].y)
            {
                result = path[0];
                result.x = result.x - Mathf.Sign(path[0].x - path[1].x);
                return result;
            }

            return path[1];
        }

        // Otherwise just stand where you are? 
        return path[0];
    }

    protected bool IsOccupied(Vector3 position)
    {
        Collider2D occupier = Physics2D.OverlapCircle(position, .2f, collisionLayers);
        if (occupier != null && occupier.gameObject != gameObject)
        {
            return true;
        }

        return false;
    }

    protected bool IsBlocked(Vector3 position)
    {

        if (Physics2D.OverlapCircle(position, .2f, collisionLayers))
        {
            return true;
        }

        return false;

    }

    protected bool IsAbove(Vector3 position, float horizontalDistance, float distance)
    {
        if (Mathf.Abs(position.x - transform.position.x) > horizontalDistance)
        {
            return false;
        }

        float diff = transform.position.y - position.y;

        if (diff <= 0)
        {
            return Mathf.Abs(diff) < distance;
        }

        return false;
    }

    protected bool IsBelow(Vector3 position, float horizontalDistance, float distance)
    {
        if (Mathf.Abs(position.x - transform.position.x) > horizontalDistance)
        {
            return false;
        }

        float diff = transform.position.y - position.y;

        if (diff >= 0)
        {
            return Mathf.Abs(diff) < distance;
        }

        return false;
    }

    protected bool IsLeft(Vector3 position, float verticalDistance, float distance)
    {
        if (Mathf.Abs(position.y - transform.position.y) > verticalDistance)
        {
            return false;
        }

        float diff = transform.position.x - position.x;

        if (diff >= 0)
        {
            return Mathf.Abs(diff) < distance;
        }

        return false;
    }

    protected bool IsRight(Vector3 position, float verticalDistance, float distance)
    {
        if (Mathf.Abs(position.y - transform.position.y) > verticalDistance)
        {
            return false;
        }

        float diff = transform.position.x - position.x;

        if (diff <= 0)
        {
            return Mathf.Abs(diff) < distance;
        }

        return false;
    }

    protected void DoDamageOnTile(Vector3Int tile)
    {
        bool removedLast = GameManager.Instance.RemoveAttackedTile(tile);

        if (removedLast)
        {
            groundTilemap.SetTileFlags(tile, TileFlags.None);
            groundTilemap.SetColor(tile, Color.white);
        }

        Vector3 worldPosition = groundTilemap.GetCellCenterLocal(tile);//groundTilemap.CellToWorld(tile));
        Debug.Log("Looking for player health on tile " + worldPosition);
        Collider2D collider = Physics2D.OverlapCircle(worldPosition, .2f, this.collisionLayers);
        if (collider != null)
        {
            PlayerHealth playerHealth = collider.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.CurrentValue -= damage;
                _OnDamage();
            }
        }
    }

    protected void ClearTile(Vector3Int tile)
    {
        bool removedLast = GameManager.Instance.RemoveAttackedTile(tile);

        if (removedLast)
        {
            groundTilemap.SetTileFlags(tile, TileFlags.None);
            groundTilemap.SetColor(tile, Color.white);
        }
    }

    protected void FaceVector(Vector3 position)
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

    public void OnDeath()
    {
        animator.SetTrigger("Die");
        enabled = false;

        Vector3 euler = transform.eulerAngles;
        euler.y = 0;
        transform.eulerAngles = euler;

        _OnDeath();
    }

    protected void HandleDeath()
    {
        Vector3Int tile = pathfindingTilemap.WorldToCell(lastPosition);
        pathfindingTilemap.SetTile(tile, null);

        tile = pathfindingTilemap.WorldToCell(transform.position);
        pathfindingTilemap.SetTile(tile, null);

        gameObject.layer = 0;

        //collider2D.enabled = false;
        if (isMyTurn)
        {
            EndTurn();
        }
    }

    protected abstract void _OnDeath();
    protected abstract void _OnDamage();

    protected void EndTurn()
    {
        if (isMyTurn)
        {
            isMyTurn = false;
            GameManager.Instance.OnEnemyTurnEnd();
        }
    }

    public override void OnAnimationEvent(string name)
    {
        bool handled = true;
        switch (name)
        {
            case "Died":
                HandleDeath();
                break;
            case "Hit":
                PerformAction();
                break;
            case "ActionDone":
                EndTurn();
                break;
            case "Air":
                inAir = true;
                break;
            default:
                handled = false;
                break;
        }

        if (!handled)
        {
            _OnAnimationEvent(name);
        }
    }

    protected abstract void _OnAnimationEvent(string name);
}

