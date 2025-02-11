using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.UIElements;

public class GridManager : BaseManager
{
    private float boardWidth;
    private float boardHeight;
    private float tileSize;
    private float spacing = 0f;
    //[SerializeField] private bool showDebugGrid = true;
    [SerializeField] private GameObject tilePrefab; // Ÿ�� ������ �߰�

    [SerializeField] private MergeableItem[,] grid;
    //public List<ItemOrdered> itemOrdereds = new List<ItemOrdered>();
    public List<Guest> currentGuests = new List<Guest>();
    private Vector2 gridStartPosition = new Vector2(0f, 0f);
    private GameObject[,] tiles; // Ÿ�� �迭 �߰�
    private Vector2[,] tilePositions;
    private GameObject mergeBoard; 
    private RectTransform mergeBoardRectT; 
    public GameObject MergeBoard => mergeBoard;
    public RectTransform MergeBoardRectT => mergeBoardRectT;

    public int Width => GameManager.GRID_WIDTH;
    public int Height => GameManager.GRID_HEIGHT;

    public override void Init()
    {
        InitializeGrid();
        GenerateTiles(); // Ÿ�� ���� ȣ��

        Vector2Int? emptyPosition = GetEmptyPosition();
        if (emptyPosition != null)
        {
            Managers.Game.SpawnGenerator("gen_anvil", 1, (Vector2Int)emptyPosition);
        }

        Vector2Int? emptyPosition2 = GetEmptyPosition();
        if (emptyPosition2 != null)
        {
            Managers.Game.SpawnGenerator("gen_orb", 1, (Vector2Int)emptyPosition2);
        }

        Vector2Int? emptyPosition3 = GetEmptyPosition();
        if (emptyPosition3 != null)
        {
            Managers.Game.SpawnGenerator("gen_pot", 1, (Vector2Int)emptyPosition3);
        }
    }
    private void InitializeGrid()
    {
        grid = new MergeableItem[Width, Height];
    }

    private void GenerateTiles()
    {
        mergeBoard = GameObject.Find("MergeBoard");
        mergeBoardRectT = mergeBoard.GetComponent<RectTransform>();
        if (mergeBoard == null)
        {
            Debug.LogError("MergeBoard not found!");
            return;
        }
        tiles = new GameObject[Width, Height];
        tilePositions = new Vector2[Width, Height];

        // mergeBoard�� ũ�� ��������
        RectTransform mergeBoardRect = mergeBoard.GetComponent<RectTransform>();
        boardWidth = mergeBoardRect.rect.width;
        boardHeight = mergeBoardRect.rect.height;

        // Ÿ�� ũ�� ���
        float totalSpacingX = (Width + 1) * spacing;
        float totalSpacingY = (Height + 1) * spacing;
        float tileSizeX = (boardWidth - totalSpacingX) / Width;
        float tileSizeY = (boardHeight - totalSpacingY) / Height;
        tileSize = Mathf.Min(tileSizeX, tileSizeY);

        // �׸��� ���� ��ġ ��� (��� ����)
        float startX = -boardWidth / 2 + spacing + tileSize / 2;
        float startY = boardHeight / 2 - spacing - tileSize / 2; // ���ʿ��� ����

        // ���� ���
        float extraSpaceX = (boardWidth - (Width * tileSize + (Width + 1) * spacing)) / 2;
        float extraSpaceY = (boardHeight - (Height * tileSize + (Height + 1) * spacing)) / 2;

        gridStartPosition = new Vector2(startX + extraSpaceX, startY - extraSpaceY); // ���ʿ��� ����

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Vector3 position = new Vector3(i * (tileSize + spacing), -j * (tileSize + spacing), 0) + (Vector3)gridStartPosition; // y ��ǥ�� �ݴ�� ���
                GameObject tileObject = Instantiate(tilePrefab, position, Quaternion.identity, mergeBoard.transform);
                tileObject.name = $"Tile_{i}_{j}";
                // Ÿ�� ũ�� ����
                RectTransform tileRect = tileObject.GetComponent<RectTransform>();
                if (tileRect != null)
                {
                    tileRect.sizeDelta = new Vector2(tileSize, tileSize);
                    tileRect.anchoredPosition = position;
                }

                Tile tile = tileObject.GetComponent<Tile>();
                if (tile != null)
                {
                    tile.Initialize(new Vector2Int(i, j));
                }

                tiles[i, j] = tileObject;
                tilePositions[i, j] = tileObject.transform.position;

                //Ÿ�� üũ���̷� ���̰� ������ ǥ��
                if((i+j) %2 ==0)
                {
                    tileObject.transform.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                   tileObject.transform.GetChild(0).gameObject.SetActive(false);
                }
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
                    Managers.Game.ReturnItemToPool(item.gameObject);
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
    public Vector2Int? GetEmptyPosition()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (IsEmptyPosition(new Vector2Int(x, y)))
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        // �� �ڸ��� ������ null ��ȯ
        return null;
    }
    // �׸��� ��ġ�� ���� ��ǥ�� ��ȯ
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        float x = gridStartPosition.x + gridPosition.x * (tileSize + spacing);
        float y = gridStartPosition.y - gridPosition.y * (tileSize + spacing); // y ��ǥ�� �ݴ�� ���
        return new Vector3(x, y, 0);
    }


    // ���� ��ǥ�� �׸��� ��ġ�� ��ȯ
    public Vector2Int? GetGridPosition(Vector3 worldPosition)
    {
        Vector2 localPosition = (Vector2)worldPosition - gridStartPosition;

        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt(localPosition.x / (tileSize + spacing)),
            Mathf.RoundToInt(-localPosition.y / (tileSize + spacing)) // y ��ǥ�� �ݴ�� ���
        );

        if(IsValidPosition(gridPos))
        {
            return gridPos;
        }
        else
        {
            return null;
        }
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
        //if (!IsValidPosition(position) || !IsEmptyPosition(position))
        //{
        //    Debug.Log("��ġ �Ұ�");
        //    return false;
        //}

        // �������� Ÿ���� �ڽ����� ��ġ
        item.transform.SetParent(tiles[position.x, position.y].transform);

        // �������� ��ġ ����
        item.transform.localPosition = Vector3.zero;

        item.GetComponent<RectTransform>().localScale = Vector3.one;

        // �������� Sibling Index�� Ÿ�Ϻ��� �տ� ��ġ
        item.transform.SetSiblingIndex(tiles[position.x, position.y].transform.GetSiblingIndex() + 1);

        // �׸��忡 ������ ��ġ
        grid[position.x, position.y] = item;
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
        if (IsValidPosition(position))
        {
            MergeableItem neighbor = grid[position.x, position.y];
            //Debug.Log($"{position.x},{position.y}��ġ : {neighbor}");
            //Debug.Log(item.CanMergeWith(neighbor));
            if (neighbor != null && item.CanMergeWith(neighbor))
            {
                return neighbor;
            }
        }

        //Vector2Int[] directions = new Vector2Int[]
        //{
        //    new Vector2Int(0, 1),   // ��
        //    new Vector2Int(1, 0),   // ��
        //    new Vector2Int(0, -1),  // ��
        //    new Vector2Int(-1, 0)   // ��
        //};
        //foreach (var direction in directions)
        //{
        //    Vector2Int checkPosition = position + direction;
        //    if (IsValidPosition(checkPosition))
        //    {
        //        MergeableItem neighbor = grid[checkPosition.x, checkPosition.y];
        //        if (neighbor != null && item.CanMergeWith(neighbor))
        //        {
        //            return neighbor;
        //        }
        //    }
        //}
        
        return null;
    }
    // ��� ���ʷ����� ã��
    // �׸��忡�� ��� Generator ������Ʈ�� ã�� �޼��� �߰�
    public List<Generator> FindAllGenerators()
    {
        List<Generator> generators = new List<Generator>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                MergeableItem item = grid[x, y];
                if (item != null)
                {
                    Generator generator = item.GetComponent<Generator>();
                    if (generator != null)
                    {
                        generators.Add(generator);
                    }
                }
            }
        }

        return generators;
    }

    public void CheckGuestsOrder()
    {

       foreach (var guest in currentGuests)
        {
            guest.CheckItemsIsExist();
        }
    }
    // ItemOrdered�� �߰��ϴ� �޼���
    public void AddGuest(Guest guest)
    {
        if (!currentGuests.Contains(guest))
        {
            currentGuests.Add(guest);
            CheckGuestsOrder();
        }
    }
    // ItemOrdered�� �����ϴ� �޼���
    public void RemoveGuest(Guest guest)
    {
        if (currentGuests.Contains(guest))
        {
            currentGuests.Remove(guest);
        }
    }
    public bool DoesItemExist(Item item)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                MergeableItem mergeableItem = grid[x, y];
                if (mergeableItem != null &&
                    mergeableItem.itemData.id == item.id &&
                    mergeableItem.Lv == item.lv)
                {
                    return true;
                }
            }
        }
        return false;
    }
    // ���ο� �Լ� �߰�
    public void RemoveItemFromGrid(Item item)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                MergeableItem mergeableItem = grid[x, y];
                if (mergeableItem != null &&
                    mergeableItem.itemData.id == item.id &&
                    mergeableItem.Lv == item.lv)
                {
                    // �׸��忡�� ������ ����
                    grid[x, y] = null;
                    Destroy(mergeableItem.gameObject);
                    return;
                }
            }
        }
    }

    public Vector2Int GetNearestEmptyPosition(Vector2Int targetGridPos)
    {

        // ���� ��ġ�� �� ��ġ��� �ٷ� ��ȯ
        if (IsEmptyPosition(targetGridPos))
        {
            return targetGridPos;
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(targetGridPos);
        visited.Add(targetGridPos);

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // �Ʒ�
            new Vector2Int(1, 0),   // ������
            new Vector2Int(0, -1),  // ��
            new Vector2Int(-1, 0)   // ����
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (var direction in directions)
            {
                Vector2Int neighbor = current + direction;
                if (IsValidPosition(neighbor) && !visited.Contains(neighbor))
                {
                    if (IsEmptyPosition(neighbor))
                    {
                        return neighbor;
                    }
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }

        // �� ��ġ�� ã�� ���� ���, �⺻�� ��ȯ (�� ���� ���� �߻����� ����)
        return targetGridPos;
    }
    private void OnDrawGizmos()
    {
        if (tiles == null || mergeBoard == null) return;

        // MergeBoard�� RectTransform ��������
        RectTransform mergeBoardRect = mergeBoard.GetComponent<RectTransform>();
        Vector3 mergeBoardPosition = mergeBoard.transform.position;

        // �׸��� ���� ��ġ ���
        float startX = mergeBoardPosition.x - mergeBoardRect.rect.width / 2 + spacing + tileSize / 2;
        float startY = mergeBoardPosition.y + mergeBoardRect.rect.height / 2 - spacing - tileSize / 2;

        // �׸��� ���� �׸���
        Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
        for (int x = 0; x <= Width; x++)
        {
            Vector3 start = new Vector3(startX + x * (tileSize + spacing), startY, 0f);
            Vector3 end = new Vector3(startX + x * (tileSize + spacing), startY - Height * (tileSize + spacing), 0f);
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= Height; y++)
        {
            Vector3 start = new Vector3(startX, startY - y * (tileSize + spacing), 0f);
            Vector3 end = new Vector3(startX + Width * (tileSize + spacing), startY - y * (tileSize + spacing), 0f);
            Gizmos.DrawLine(start, end);
        }

        // �� �߽��� �� ������ ǥ��
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector3 center = new Vector3(startX + x * (tileSize + spacing), startY - y * (tileSize + spacing), 0f);
                if (grid[x, y] != null)
                {
                    Gizmos.color = Color.green; // �������� �ִ� ��� �ʷϻ�
                    Gizmos.DrawSphere(center, tileSize / 4);
                }
                else
                {
                    Gizmos.color = Color.red; // �������� ���� ��� ������
                    Gizmos.DrawWireSphere(center, tileSize / 4);
                }
            }
        }
    }
    // ����� �׸��� �׸���
    //private void OnDrawGizmos()
    //{
    //    if (!showDebugGrid) return;

    //    // �׸��� ���� �׸���
    //    Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
    //    for (int x = 0; x <= Width; x++)
    //    {
    //        Vector3 start = gridStartPosition + new Vector2(x * cellSize.x, 0f);
    //        Vector3 end = start + new Vector3(0f, Height * cellSize.y, 0f);
    //        Gizmos.DrawLine(start, end);
    //    }

    //    for (int y = 0; y <= Height; y++)
    //    {
    //        Vector3 start = gridStartPosition + new Vector2(0f, y * cellSize.y);
    //        Vector3 end = start + new Vector3(Width * cellSize.x, 0f, 0f);
    //        Gizmos.DrawLine(start, end);
    //    }

    //    // �� �߽��� ǥ��
    //    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
    //    for (int x = 0; x < Width; x++)
    //    {
    //        for (int y = 0; y < Height; y++)
    //        {
    //            Vector3 center = GetWorldPosition(new Vector2Int(x, y));
    //            Gizmos.DrawWireSphere(center, 0.1f);
    //        }
    //    }
    //}
}