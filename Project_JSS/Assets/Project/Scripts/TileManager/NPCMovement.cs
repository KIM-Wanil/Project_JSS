using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class NPCMovement : MonoBehaviour
{
    [SerializeField] FloorManager floorManager;
    public float moveSpeed = 2f;
    public float idleTime = 3f;  // 한 위치에서 머무는 시간
    public float randomMoveProbability = 0.7f;  // 랜덤 이동 확률
    [SerializeField] IsoGridFloor isometricGrid;
    [SerializeField] Pathfinder pathfinder;
    private List<Vector3> currentPath;
    private int currentWaypointIndex;
    private bool isMoving = false;
    private float timer = 0f;

    private Vector2 lastDirection;
    [SerializeField] FurnitureInfo[] specialObject;
    [SerializeField] string specialObjectName;
    private bool isSpecial;
    private const string VERTICAL = "Vertical";
    private const string IS_MOVING = "IsMoving";
    private const string IS_SPECIAL = "Special";
    private const string IS_SPECIAL2 = "Special2";
    public bool ChangeSpecial;
    private bool ChangeBool;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        lastDirection = Vector2.down;
    }
    void Update()
    {
        if (isMoving)
        {
            MoveAlongPath();
        }
        else
        {
            // 타이머 증가
            timer += Time.deltaTime;

            // 지정된 시간이 지나면 새로운 이동 시작
            if (timer >= idleTime)
            {
                timer = 0f;

                // 확률에 따라 랜덤 이동 실행
                if (Random.value <= randomMoveProbability)
                {
                    MoveToRandomPosition();
                }
                else
                {
                    MoveToRandomObject();
                }
            }
        }
    }

    // 목표 위치로 이동
    public bool MoveTo(Vector3 targetPosition)
    {
        currentPath = pathfinder.FindPath(this.transform.position, targetPosition);
        if (currentPath != null && currentPath.Count > 0)
        {
            currentWaypointIndex = 0;
            isMoving = true;
            return true;
        }

        return false;
    }
    public bool MoveToRandomObject()
    {
        // 최대 10번 시도
        foreach (GameObject obj in floorManager.availableFurniturePrefabs)
        {
            if (specialObjectName == obj.GetComponent<FurnitureInfo>().name) {
                if (isometricGrid.CanPlaceTile(obj.GetComponent<FurnitureInfo>().GetTargetPosition(0)))
                {
                    Vector3 targetPos = isometricGrid.GridPositionToWorld(obj.GetComponent<FurnitureInfo>().GetTargetPosition(0));
                    isSpecial = true;
                    return MoveTo(targetPos);
                }
            }
        }
        Debug.Log("랜덤 위치를 찾을 수 없습니다!");
        return false;
    }

    // 랜덤 위치로 이동
    public bool MoveToRandomPosition()
    {
        // 최대 10번 시도
        for (int i = 0; i < 10; i++)
        {
            int x = Random.Range(0, 12);
            int y = Random.Range(0, 12);

            
            if (isometricGrid.CanPlaceTile(new Vector2Int(x,y)))
            {
                Vector3 targetPos = isometricGrid.GridPositionToWorld(new Vector2Int( x, y));
                return MoveTo(targetPos);
    
            }
        }

        Debug.Log("랜덤 위치를 찾을 수 없습니다!");
        return false;
    }

    // 경로를 따라 이동
    private void MoveAlongPath()
    {
        if (currentPath == null || currentWaypointIndex >= currentPath.Count)
        {
            isMoving = false;
            return;
        }
     
        Vector3 targetWaypoint = currentPath[currentWaypointIndex];
        Vector3 moveDirection = (targetWaypoint - transform.position).normalized;
        Vector2Int targetGrid = isometricGrid.WorldToGridPosition(targetWaypoint);
       // this.GetComponent<SpriteRenderer>().sortingOrder = 22 - (targetGrid.x+ targetGrid.y);
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        transform.position.Set(transform.position.x, transform.position.y, transform.position.y);
        animator.SetFloat(VERTICAL, moveDirection.y);
        animator.SetBool(IS_MOVING, true);
        this.GetComponent<SpriteRenderer>().flipX =( moveDirection.x > 0);

        // 목표 웨이포인트에 도달했는지 확인
        if (Vector3.Distance(transform.position, targetWaypoint) < 0.1f)
        {
            currentWaypointIndex++;

            // 모든 웨이포인트를 지났는지 확인
            if (currentWaypointIndex >= currentPath.Count)
            {
                isMoving = false;
                if (isSpecial)
                {
                    this.GetComponent<SpriteRenderer>().flipX = false;
                    if (ChangeSpecial)
                    {
                        if (ChangeBool)
                        {
                            animator.SetTrigger(IS_SPECIAL);
                        }
                        else
                        {
                            animator.SetTrigger(IS_SPECIAL2);
                        }
                        ChangeBool = !ChangeBool;
                    }
                    else
                        animator.SetTrigger(IS_SPECIAL);

                    isSpecial = false;
                    idleTime = 9.0f;
                }
                else
                {
                    idleTime = 3.0f;
                }
                animator.SetBool(IS_MOVING, false);
            }
        }
    }

    // 이동 중지
    public void StopMoving()
    {
        isMoving = false;
        currentPath = null;
    }

    // 목적지 도달 가능 여부 확인
    public bool CanReachPosition(Vector3 targetPosition)
    {
        return pathfinder.IsReachable(transform.position, targetPosition);
    }
}
