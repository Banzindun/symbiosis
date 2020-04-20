using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Toolbox;
using UnityEngine.UI;

public class SpitterController : MonsterController
{
    [SerializeField] private float range;

    [SerializeField] private Sprite horizontalLine;
    [SerializeField] private Sprite verticalLine;
    [SerializeField] private Sprite verticalDownEnd;
    [SerializeField] private Sprite verticalUpEnd;
    [SerializeField] private Sprite horizontalLeftEnd;
    [SerializeField] private Sprite horizontalRightEnd;

    [SerializeField] private Sprite initialHorizontalRight;
    [SerializeField] private Sprite initialHorizontalLeft;
    [SerializeField] private Sprite initialVerticalUp;
    [SerializeField] private Sprite initialVerticalDown;

    [SerializeField] private List<GameObject> ray;

    private List<Vector3Int> attackedTiles = new List<Vector3Int>();

    protected override void ScheduleAction()
    {
        player = GameManager.Instance.playerBehaviour;
        bool playerInRange = Vector3.Distance(transform.position, player.transform.position) < range;

        FacePlayer();

        if (!playerInRange)
        {
            return;
        }

        Debug.Log("SPITTER: Player in range!");

        Vector3 playerPosition = player.transform.position;

        attackDirection = Vector3.zero;
        // First check around the positions 
        if (IsAbove(playerPosition, 0.05f, range))
        {
            attackDirection = new Vector3(0, 1f, 0);
        }
        else if (IsBelow(playerPosition, 0.05f, range))
        {
            attackDirection = new Vector3(0, -1f, 0);
        }
        else if (IsRight(playerPosition, 0.05f, range))
        {
            attackDirection = new Vector3(1f, 0, 0);
        }
        else if (IsLeft(playerPosition, 0.05f, range))
        {
            attackDirection = new Vector3(-1f, 0, 0);
        }

        Debug.Log("SPITTER: attack direction " + attackDirection);

        //else
        //{
        //    Vector3[] choices = { new Vector3(1f, 0, 0), new Vector3(0, 1f, 0),
        //        new Vector3(-1f, 0, 0), new Vector3(0, -1f, 0) };
        //
        //    int index = Random.Range(0, 4);
        //    addition = choices[index];
        //}

        if (attackDirection == Vector3.zero)
        {
            return;
        }

        List<Vector3> attackPositions = new List<Vector3>();
        Vector3 position = transform.position;

        if (ray != null)
        {
            for (int i = 0; i < ray.Count; i++)
            {
                ray[i].SetActive(false);
                Destroy(ray[i]);
            }
            ray.Clear();
        }

        ray = new List<GameObject>();
        AddInitialRay(transform.position); // Initnial ray

        bool playerReached = false;
        for (int i = 0; i < range; i++)
        {
            position += attackDirection;

            Debug.Log("Position: " + position);

            if (!TilemapExtensions.IsCellEmpty(pathfindingTilemap, position))
            {
                if (!playerReached)
                {
                    Debug.Log("SPITTER: Clearing ray because player not reached.");
                    attackPositions.Clear();
                }

                Destroy(ray[ray.Count - 1]);
                ray.RemoveAt(ray.Count - 1);
                AddRay(position - attackDirection, true);
                break;
            }

            attackPositions.Add(position);

            if (Vector3.Distance(position, playerPosition) < 0.05f)
            {
                playerReached = true;
            }

            // Add the ray:
            if (i == range - 1)
            {
                AddRay(position, true);
            }
            else
            {
                AddRay(position, false);
            }
        }

        for (int i = 0; i < attackPositions.Count; i++)
        {
            Vector3Int tilePosition = groundTilemap.WorldToCell(attackPositions[i]);
            groundTilemap.SetTileFlags(tilePosition, TileFlags.None);
            groundTilemap.SetColor(tilePosition, attackColor);

            attackedTiles.Add(tilePosition);
            GameManager.Instance.AddAttackedTile(tilePosition);
        }
    }

    private void AddRay(Vector3 position, bool ending)
    {
        if (attackDirection.x == 1f)
        {
            GameObject newRay = new GameObject();
            newRay.transform.parent = transform;
            newRay.transform.position = position;
            newRay.SetActive(false);
            SpriteRenderer spriteRenderer = newRay.AddComponent<SpriteRenderer>();

            if (ending)
            {
                spriteRenderer.sprite = horizontalRightEnd;
            }
            else
            {
                spriteRenderer.sprite = horizontalLine;
            }

            spriteRenderer.sortingLayerName = "Front";

            ray.Add(newRay);
        }
        else if (attackDirection.x == -1f)
        {
            GameObject newRay = new GameObject();
            newRay.SetActive(false);
            newRay.transform.parent = transform;
            newRay.transform.position = position;
            SpriteRenderer spriteRenderer = newRay.AddComponent<SpriteRenderer>();

            if (ending)
            {
                spriteRenderer.sprite = horizontalLeftEnd;
            }
            else
            {
                spriteRenderer.sprite = horizontalLine;
            }

            spriteRenderer.sortingLayerName = "Front";

            ray.Add(newRay);
        }
        else if (attackDirection.y == 1f)
        {
            GameObject newRay = new GameObject();
            newRay.transform.parent = transform;
            newRay.transform.position = position;
            newRay.SetActive(false);
            SpriteRenderer spriteRenderer = newRay.AddComponent<SpriteRenderer>();

            if (ending)
            {
                spriteRenderer.sprite = verticalUpEnd;
            }
            else
            {
                spriteRenderer.sprite = verticalLine;
            }

            spriteRenderer.sortingLayerName = "Front";

            ray.Add(newRay);
        }
        if (attackDirection.y == -1f)
        {
            GameObject newRay = new GameObject();
            newRay.transform.parent = transform;
            newRay.transform.position = position;
            newRay.SetActive(false);
            SpriteRenderer spriteRenderer = newRay.AddComponent<SpriteRenderer>();

            if (ending)
            {
                spriteRenderer.sprite = verticalDownEnd;
            }
            else
            {
                spriteRenderer.sprite = verticalLine;
            }

            spriteRenderer.sortingLayerName = "Front";

            ray.Add(newRay);
        }
    }

    private void CreateRayPart(Vector3 position, Sprite sprite)
    {
        GameObject newRay = new GameObject();
        newRay.transform.parent = transform;
        newRay.transform.position = position;
        newRay.SetActive(false);
        SpriteRenderer spriteRenderer = newRay.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = "Front";
        ray.Add(newRay);
    }

    private void AddInitialRay(Vector3 position)
    {
        if (attackDirection.x == 1f)
        {
            CreateRayPart(position, initialHorizontalRight);
        }
        else if (attackDirection.x == -1f)
        {
            CreateRayPart(position, initialHorizontalLeft);
        }
        else if (attackDirection.y == 1f)
        {
            CreateRayPart(position, initialVerticalUp);
        }
        if (attackDirection.y == -1f)
        {
            CreateRayPart(position, initialVerticalDown);
        }
    }

    protected override bool PrepareAction()
    {
        if (attackedTiles.Count > 0)
        {
            //FaceVector(attackDirection);
            animator.SetTrigger("Attack");
            return true;
        }

        return false;
    }

    protected override void PerformAction()
    {
        for (int i = 0; i < attackedTiles.Count; i++)
        {
            DoDamageOnTile(attackedTiles[i]);
        }

        attackedTiles = new List<Vector3Int>();
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
        for (int i = 0; i < attackedTiles.Count; i++)
        {
            ClearTile(attackedTiles[i]);
        }
        attackedTiles = new List<Vector3Int>();

        MusicManager.Instance.Play("SpitterDeath");
    }

    protected override void _OnDamage()
    {
        MusicManager.Instance.Play("SpitterAttack");
    }

    protected override void _OnAnimationEvent(string name)
    {
        switch (name)
        {
            case "RayOpen":
                {
                    if (attackedTiles.Count > 0)
                    {
                        for (int i = 0; i < ray.Count; i++)
                        {
                            ray[i].SetActive(true);
                        }
                    }
                    MusicManager.Instance.Play("SplitterAttack");
                }
                break;
            case "RayClose":
                {
                    for (int i = 0; i < ray.Count; i++)
                    {
                        ray[i].SetActive(false);
                        Destroy(ray[i]);
                    }
                    ray.Clear();
                }
                break;
            default:
                break;
        }
    }
}