using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Toolbox;

public class MonsterController : CustomMonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private float moveSpeed;

    [SerializeField] Transform movementPoint;

    [SerializeField] Tilemap pathfindingTilemap;

    [SerializeField] LayerMask collisionLayers;

    private CustomMonoBehaviour player;

    private bool isMyTurn;

    private void OnDestroy()
    {
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
        isMyTurn = false;
        GameManager.Instance.OnEnemyTurnEnd();
    }

    public override void OnEnemyMove()
    {
        isMyTurn = true;
        Move();
    }

    public override void OnTurnEnd()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        movementPoint.parent = null;
        GameManager.Instance.RegisterEnemy(this);
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, movementPoint.position, moveSpeed * Time.deltaTime);

        if (isMyTurn)
        {
            if (Vector3.Distance(transform.position, movementPoint.position) < 0.05f)
            {
                GameManager.Instance.OnEnemyTurnEnd();
                isMyTurn = false;
            }
        }
    }

    void Move()
    {
        Vector3 nextPosition = DoPathfinding(movementPoint.position);
        movementPoint.position = nextPosition;
    }

    private Vector3 DoPathfinding(Vector3 oldPosition)
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
            return GetFirstTile(path);
        }

        for (int i = 0; i < 4; i++)
        {
            if (IsOccupied(advancedPositions[i]))
            {
                continue;
            }

            List<Vector3> path = AStar.FindPath(pathfindingTilemap, transform.position, advancedPositions[i]);
            return GetFirstTile(path);
        }

        return oldPosition;
    }

    private Vector3 GetFirstTile(List<Vector3> path)
    {
        Vector3 result = Vector3.zero;

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

        // In case its not line 
        if (path.Count > 1)
        {
            return path[1];
        }

        // Otherwise just stand where you are? 
        return path[0];
    }

    private bool IsOccupied(Vector3 position)
    {
        if (Physics2D.OverlapCircle(position, .2f, collisionLayers))
        {
            return false;
        }

        List<CustomMonoBehaviour> enemyBehaviours = GameManager.Instance.EnemyBehaviours;

        foreach (CustomMonoBehaviour enemy in enemyBehaviours)
        {
            if (enemy != this && Vector3.Distance(position, enemy.transform.position) <= 0.5f)
            {
                return true;
            }
        }

        return false;
    }
}
