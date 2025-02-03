using UnityEngine;
using System.Collections.Generic;

public class GridManager : BaseManager
{
    [Header("Grid Settings")]
    [SerializeField] private Vector2 cellSize = new Vector2(1f, 1f);
    [SerializeField] private Vector2 gridOffset = Vector2.zero;
    [SerializeField] private bool showDebugGrid = true;
    [SerializeField] private GameObject tilePrefab; // 타일 프리팹 추가

    private MergeableItem[,] grid;
    private Vector2 gridStartPosition;
    private GameObject[,] tiles; // 타일 배열 추가

    public int Width => GameManager.GRID_WIDTH;
    public int Height => GameManager.GRID_HEIGHT;

    public override void Init()
    {
        InitializeGrid();
        GenerateTiles(); // 타일 생성 호출
    }

    private void InitializeGrid()
    {
        grid = new MergeableItem[Width, Height];

        // 그리드의 시작 위치를 계산 (좌하단 기준)
        gridStartPosition = (Vector2)transform.position -
            new Vector2(Width * cellSize.x / 2f, Height * cellSize.y / 2f) +
            gridOffset;
    }

    private void GenerateTiles()
    {
        tiles = new GameObject[Width, Height];

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Vector3 position = new Vector3(i * cellSize.x, j * cellSize.y, 0) + (Vector3)gridStartPosition;
                GameObject tileObject = Instantiate(tilePrefab, position, Quaternion.identity);
                tileObject.transform.parent = this.transform;

                Tile tile = tileObject.GetComponent<Tile>();
                if (tile != null)
                {
                    tile.Initialize(new Vector2Int(i, j));
                }

                tiles[i, j] = tileObject;
            }
        }
    }

    // 그리드의 특정 위치에 있는 아이템 반환
    public MergeableItem GetItemAt(Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            return grid[position.x, position.y];
        }
        return null;
    }

    // 그리드 전체 비우기
    public void ClearGrid()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (grid[x, y] != null)
                {
                    MergeableItem item = grid[x, y];
                    grid[x, y] = null;
                    GameManager.Instance.ReturnItemToPool(item.gameObject);
                }
            }
        }
    }

    // 그리드의 모든 아이템 가져오기
    public List<MergeableItem> GetAllItems()
    {
        List<MergeableItem> items = new List<MergeableItem>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (grid[x, y] != null)
                {
                    items.Add(grid[x, y]);
                }
            }
        }
        return items;
    }

    // 아이템 이동
    public bool MoveItem(Vector2Int fromPosition, Vector2Int toPosition)
    {
        if (!IsValidPosition(fromPosition) || !IsValidPosition(toPosition))
            return false;

        if (grid[fromPosition.x, fromPosition.y] == null)
            return false;

        if (grid[toPosition.x, toPosition.y] != null)
            return false;

        MergeableItem item = grid[fromPosition.x, fromPosition.y];
        grid[fromPosition.x, fromPosition.y] = null;
        grid[toPosition.x, toPosition.y] = item;
        item.transform.position = GetWorldPosition(toPosition);
        item.SetGridPosition(toPosition);

        return true;
    }

    // 그리드가 가득 찼는지 확인
    public bool IsGridFull()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (grid[x, y] == null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // 무작위 빈 위치 찾기
    public Vector2Int? GetRandomEmptyPosition()
    {
        List<Vector2Int> emptyPositions = new List<Vector2Int>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (IsEmptyPosition(pos))
                {
                    emptyPositions.Add(pos);
                }
            }
        }

        if (emptyPositions.Count > 0)
        {
            return emptyPositions[Random.Range(0, emptyPositions.Count)];
        }

        return null;
    }

    // 그리드 위치를 월드 좌표로 변환
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            gridStartPosition.x + gridPosition.x * cellSize.x + cellSize.x / 2f,
            gridStartPosition.y + gridPosition.y * cellSize.y + cellSize.y / 2f,
            0f
        );
    }

    // 월드 좌표를 그리드 위치로 변환
    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        Vector2 localPosition = (Vector2)worldPosition - gridStartPosition;
        return new Vector2Int(
            Mathf.FloorToInt(localPosition.x / cellSize.x),
            Mathf.FloorToInt(localPosition.y / cellSize.y)
        );
    }

    // 해당 그리드 위치가 유효한지 확인
    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < Width &&
               position.y >= 0 && position.y < Height;
    }

    // 해당 그리드 위치가 비어있는지 확인
    public bool IsEmptyPosition(Vector2Int position)
    {
        return IsValidPosition(position) && grid[position.x, position.y] == null;
    }

    // 아이템을 그리드에 배치
    public bool PlaceItem(MergeableItem item, Vector2Int position)
    {
        if (!IsValidPosition(position) || !IsEmptyPosition(position))
            return false;

        grid[position.x, position.y] = item;
        item.transform.position = GetWorldPosition(position);
        item.SetGridPosition(position);
        return true;
    }

    // 아이템을 그리드에서 제거
    public void RemoveItem(Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            grid[position.x, position.y] = null;
        }
    }

    // 머지 가능한 아이템들 찾기
    public List<(MergeableItem, MergeableItem)> FindAllMergeablePairs()
    {
        List<(MergeableItem, MergeableItem)> mergeablePairs = new List<(MergeableItem, MergeableItem)>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);
                MergeableItem currentItem = GetItemAt(currentPos);

                if (currentItem != null)
                {
                    // 오른쪽과 위쪽만 검사 (중복 방지)
                    Vector2Int[] checkDirections = new Vector2Int[]
                    {
                        new Vector2Int(1, 0),  // 오른쪽
                        new Vector2Int(0, 1)   // 위쪽
                    };

                    foreach (var direction in checkDirections)
                    {
                        Vector2Int neighborPos = currentPos + direction;
                        MergeableItem neighbor = GetItemAt(neighborPos);

                        if (neighbor != null && currentItem.CanMergeWith(neighbor))
                        {
                            mergeablePairs.Add((currentItem, neighbor));
                        }
                    }
                }
            }
        }

        return mergeablePairs;
    }

    // 특정 위치 주변의 머지 가능한 아이템 찾기
    public MergeableItem FindMergeableNeighbor(Vector2Int position, MergeableItem item)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // 상
            new Vector2Int(1, 0),   // 우
            new Vector2Int(0, -1),  // 하
            new Vector2Int(-1, 0)   // 좌
        };

        foreach (var direction in directions)
        {
            Vector2Int checkPosition = position + direction;
            if (IsValidPosition(checkPosition))
            {
                MergeableItem neighbor = grid[checkPosition.x, checkPosition.y];
                if (neighbor != null && item.CanMergeWith(neighbor))
                {
                    return neighbor;
                }
            }
        }

        return null;
    }
    public Vector2Int GetNearestEmptyPosition(Vector3 worldPosition)
    {
        Vector2Int gridPosition = GetGridPosition(worldPosition);
        Vector2Int nearestEmpty = gridPosition;
        float nearestDistance = float.MaxValue;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (IsEmptyPosition(pos))
                {
                    float distance = Vector2.Distance(worldPosition, GetWorldPosition(pos));
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEmpty = pos;
                    }
                }
            }
        }

        return nearestEmpty;
    }
    // 디버그 그리드 그리기
    private void OnDrawGizmos()
    {
        if (!showDebugGrid) return;

        // 그리드 라인 그리기
        Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
        for (int x = 0; x <= Width; x++)
        {
            Vector3 start = gridStartPosition + new Vector2(x * cellSize.x, 0f);
            Vector3 end = start + new Vector3(0f, Height * cellSize.y, 0f);
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= Height; y++)
        {
            Vector3 start = gridStartPosition + new Vector2(0f, y * cellSize.y);
            Vector3 end = start + new Vector3(Width * cellSize.x, 0f, 0f);
            Gizmos.DrawLine(start, end);
        }

        // 셀 중심점 표시
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector3 center = GetWorldPosition(new Vector2Int(x, y));
                Gizmos.DrawWireSphere(center, 0.1f);
            }
        }
    }
}