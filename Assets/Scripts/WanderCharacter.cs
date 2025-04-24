using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WanderCharacter : MonoBehaviour
{
    public float moveInterval = 2f;
    public float moveSpeed = 2f;
    private bool isMoving = false;
    private Vector3 targetPos;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        StartCoroutine(Wander());

        if (OccupiedMapManager.Instance == null)
        {
            Debug.LogError("OccupiedMapManagerがまだ初期化されていません！");
        }        
    }

    void LateUpdate()
    {
        int baseSortingOrder = 1500;

        // スプライトの見た目のY位置を基準に描画順を決定
        float visualY = spriteRenderer.transform.position.y;

        spriteRenderer.sortingOrder = baseSortingOrder - (int)(visualY * 100);
    }

    IEnumerator Wander()
    {
        while (true)
        {
            yield return new WaitForSeconds(moveInterval);

            if (!isMoving)
            {
                Vector3Int currentCell = GetCurrentCell();

                List<Vector3Int> possibleDirections = new List<Vector3Int>();

                foreach (Vector3Int dir in GetAllDirections())
                {
                    Vector3Int nextCell = currentCell + dir;
                    Debug.Log($"候補マス: {nextCell} / Occupied: {OccupiedMapManager.Instance.IsCellOccupied(nextCell)}");

                    if (!OccupiedMapManager.Instance.IsCellOccupied(nextCell))
                    {
                        possibleDirections.Add(nextCell);
                    }
                }

                if (possibleDirections.Count > 0)
                {
                    Vector3Int nextCell = possibleDirections[Random.Range(0, possibleDirections.Count)];
                    targetPos = CellToWorldCenter(nextCell);

                    Vector3 moveDir = (targetPos - transform.position).normalized;
                    UpdateFacingDirection(moveDir);

                    StartCoroutine(MoveTo(targetPos));
                }
            }
        }
    }

    IEnumerator MoveTo(Vector3 target)
    {
        isMoving = true;
        while ((transform.position - target).sqrMagnitude > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        Vector3Int snappedCell = TilemapReference.Instance.tilemap.WorldToCell(target);
        transform.position = TilemapReference.Instance.tilemap.GetCellCenterWorld(snappedCell);

        isMoving = false;
    }

    Vector3Int[] GetAllDirections()
    {
        return new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
        };
    }

    void UpdateFacingDirection(Vector3 dir)
    {
        if (spriteRenderer == null || animator == null) return;

        if (dir.y > 0)
        {
            animator.Play("Worker_LeftUp");
            spriteRenderer.flipX = dir.x > 0;
        }
        else
        {
            animator.Play("Worker_LeftDown");
            spriteRenderer.flipX = dir.x > 0;
        }
    }

    Vector3Int GetCurrentCell()
    {
        return TilemapReference.Instance.tilemap.WorldToCell(transform.position);
    }

    Vector3 CellToWorldCenter(Vector3Int cell)
    {
        return TilemapReference.Instance.tilemap.GetCellCenterWorld(cell);
    }
}
