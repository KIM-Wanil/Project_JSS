using UnityEngine;
using System.Collections.Generic;

public class GridManager : BaseManager
{
    [Header("Grid Settings")]
    [SerializeField] private Vector2 cellSize = new Vector2(1f, 1f);
    [SerializeField] private Vector2 gridOffset = Vector2.zero;
    [SerializeField] private bool showDebugGrid = true;
    [SerializeField] private GameObject tilePrefab; // Ÿ�� ������ �߰�

    private MergeableItem[,] grid;
    private Vector2 gridStartPosition;
    private GameObject[,] tiles; // Ÿ�� �迭 �߰�

    public int Width => GameManager.GRID_WIDTH;
    public int Height => GameManager.GRID_HEIGHT;

    public override void Init()
    {
        InitializeGrid();
        GenerateTiles(); // Ÿ�� ���� ȣ��
    }

    private void InitializeGrid()
    {
        grid = new MergeableItem[Width, Height];

        // �׸����� ���� ��ġ�� ��� (���ϴ� ����)
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

    // �׸����� Ư�� ��ġ�� �ִ� ������ ��ȯ
    public MergeableItem GetItemAt(Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            return grid[position.x, position.y];
        }
        return null;
    }

    // �׸��� ��ü ����
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

    // �׸����� ��� ������ ��������
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

    // ������ �̵�
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

    // �׸��尡 ���� á���� Ȯ��
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

    // ������ �� ��ġ ã��
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

    // �׸��� ��ġ�� ���� ��ǥ�� ��ȯ
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            gridStartPosition.x + gridPosition.x * cellSize.x + cellSize.x / 2f,
            gridStartPosition.y + gridPosition.y * cellSize.y + cellSize.y / 2f,
            0f
        );
    }

    // ���� ��ǥ�� �׸��� ��ġ�� ��ȯ
    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        Vector2 localPosition = (Vector2)worldPosition - gridStartPosition;
        return new Vector2Int(
            Mathf.FloorToInt(localPosition.x / cellSize.x),
            Mathf.FloorToInt(localPosition.y / cellSize.y)
        );
    }

    // �ش� �׸��� ��ġ�� ��ȿ���� Ȯ��
    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < Width &&
               position.y >= 0 && position.y < Height;
    }

    // �ش� �׸��� ��ġ�� ����ִ��� Ȯ��
    public bool IsEmptyPosition(Vector2Int position)
    {
        return IsValidPosition(position) && grid[position.x, position.y] == null;
    }

    // �������� �׸��忡 ��ġ
    public bool PlaceItem(MergeableItem item, Vector2Int position)
    {
        if (!IsValidPosition(position) || !IsEmptyPosition(position))
            return false;

        grid[position.x, position.y] = item;
        item.transform.position = GetWorldPosition(position);
        item.SetGridPosition(position);
        return true;
    }

    // �������� �׸��忡�� ����
    public void RemoveItem(Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            grid[position.x, position.y] = null;
        }
    }

    // ���� ������ �����۵� ã��
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
                    // �����ʰ� ���ʸ� �˻� (�ߺ� ����)
                    Vector2Int[] checkDirections = new Vector2Int[]
                    {
                        new Vector2Int(1, 0),  // ������
                        new Vector2Int(0, 1)   // ����
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

    // Ư�� ��ġ �ֺ��� ���� ������ ������ ã��
    public MergeableItem FindMergeableNeighbor(Vector2Int position, MergeableItem item)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // ��
            new Vector2Int(1, 0),   // ��
            new Vector2Int(0, -1),  // ��
            new Vector2Int(-1, 0)   // ��
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
    // ����� �׸��� �׸���
    private void OnDrawGizmos()
    {
        if (!showDebugGrid) return;

        // �׸��� ���� �׸���
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

        // �� �߽��� ǥ��
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