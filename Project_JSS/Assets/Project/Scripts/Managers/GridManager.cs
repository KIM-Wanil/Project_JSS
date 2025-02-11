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
    [SerializeField] private GameObject tilePrefab; // 타일 프리팹 추가

    [SerializeField] private MergeableItem[,] grid;
    //public List<ItemOrdered> itemOrdereds = new List<ItemOrdered>();
    public List<Guest> currentGuests = new List<Guest>();
    private Vector2 gridStartPosition = new Vector2(0f, 0f);
    private GameObject[,] tiles; // 타일 배열 추가
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
        GenerateTiles(); // 타일 생성 호출

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

        // mergeBoard의 크기 가져오기
        RectTransform mergeBoardRect = mergeBoard.GetComponent<RectTransform>();
        boardWidth = mergeBoardRect.rect.width;
        boardHeight = mergeBoardRect.rect.height;

        // 타일 크기 계산
        float totalSpacingX = (Width + 1) * spacing;
        float totalSpacingY = (Height + 1) * spacing;
        float tileSizeX = (boardWidth - totalSpacingX) / Width;
        float tileSizeY = (boardHeight - totalSpacingY) / Height;
        tileSize = Mathf.Min(tileSizeX, tileSizeY);

        // 그리드 시작 위치 계산 (가운데 정렬)
        float startX = -boardWidth / 2 + spacing + tileSize / 2;
        float startY = boardHeight / 2 - spacing - tileSize / 2; // 위쪽에서 시작

        // 여백 계산
        float extraSpaceX = (boardWidth - (Width * tileSize + (Width + 1) * spacing)) / 2;
        float extraSpaceY = (boardHeight - (Height * tileSize + (Height + 1) * spacing)) / 2;

        gridStartPosition = new Vector2(startX + extraSpaceX, startY - extraSpaceY); // 위쪽에서 시작

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Vector3 position = new Vector3(i * (tileSize + spacing), -j * (tileSize + spacing), 0) + (Vector3)gridStartPosition; // y 좌표를 반대로 계산
                GameObject tileObject = Instantiate(tilePrefab, position, Quaternion.identity, mergeBoard.transform);
                tileObject.name = $"Tile_{i}_{j}";
                // 타일 크기 조정
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

                //타일 체크무늬로 보이게 띄엄띄엄 표시
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
                    Managers.Game.ReturnItemToPool(item.gameObject);
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

        // 빈 자리가 없으면 null 반환
        return null;
    }
    // 그리드 위치를 월드 좌표로 변환
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        float x = gridStartPosition.x + gridPosition.x * (tileSize + spacing);
        float y = gridStartPosition.y - gridPosition.y * (tileSize + spacing); // y 좌표를 반대로 계산
        return new Vector3(x, y, 0);
    }


    // 월드 좌표를 그리드 위치로 변환
    public Vector2Int? GetGridPosition(Vector3 worldPosition)
    {
        Vector2 localPosition = (Vector2)worldPosition - gridStartPosition;

        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt(localPosition.x / (tileSize + spacing)),
            Mathf.RoundToInt(-localPosition.y / (tileSize + spacing)) // y 좌표를 반대로 계산
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
        //if (!IsValidPosition(position) || !IsEmptyPosition(position))
        //{
        //    Debug.Log("배치 불가");
        //    return false;
        //}

        // 아이템을 타일의 자식으로 배치
        item.transform.SetParent(tiles[position.x, position.y].transform);

        // 아이템의 위치 설정
        item.transform.localPosition = Vector3.zero;

        item.GetComponent<RectTransform>().localScale = Vector3.one;

        // 아이템의 Sibling Index를 타일보다 앞에 배치
        item.transform.SetSiblingIndex(tiles[position.x, position.y].transform.GetSiblingIndex() + 1);

        // 그리드에 아이템 배치
        grid[position.x, position.y] = item;
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
        if (IsValidPosition(position))
        {
            MergeableItem neighbor = grid[position.x, position.y];
            //Debug.Log($"{position.x},{position.y}위치 : {neighbor}");
            //Debug.Log(item.CanMergeWith(neighbor));
            if (neighbor != null && item.CanMergeWith(neighbor))
            {
                return neighbor;
            }
        }

        //Vector2Int[] directions = new Vector2Int[]
        //{
        //    new Vector2Int(0, 1),   // 상
        //    new Vector2Int(1, 0),   // 우
        //    new Vector2Int(0, -1),  // 하
        //    new Vector2Int(-1, 0)   // 좌
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
    // 모든 제너레이터 찾기
    // 그리드에서 모든 Generator 컴포넌트를 찾는 메서드 추가
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
    // ItemOrdered를 추가하는 메서드
    public void AddGuest(Guest guest)
    {
        if (!currentGuests.Contains(guest))
        {
            currentGuests.Add(guest);
            CheckGuestsOrder();
        }
    }
    // ItemOrdered를 제거하는 메서드
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
    // 새로운 함수 추가
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
                    // 그리드에서 아이템 제거
                    grid[x, y] = null;
                    Destroy(mergeableItem.gameObject);
                    return;
                }
            }
        }
    }

    public Vector2Int GetNearestEmptyPosition(Vector2Int targetGridPos)
    {

        // 시작 위치가 빈 위치라면 바로 반환
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
            new Vector2Int(0, 1),   // 아래
            new Vector2Int(1, 0),   // 오른쪽
            new Vector2Int(0, -1),  // 위
            new Vector2Int(-1, 0)   // 왼쪽
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

        // 빈 위치를 찾지 못한 경우, 기본값 반환 (이 경우는 거의 발생하지 않음)
        return targetGridPos;
    }
    private void OnDrawGizmos()
    {
        if (tiles == null || mergeBoard == null) return;

        // MergeBoard의 RectTransform 가져오기
        RectTransform mergeBoardRect = mergeBoard.GetComponent<RectTransform>();
        Vector3 mergeBoardPosition = mergeBoard.transform.position;

        // 그리드 시작 위치 계산
        float startX = mergeBoardPosition.x - mergeBoardRect.rect.width / 2 + spacing + tileSize / 2;
        float startY = mergeBoardPosition.y + mergeBoardRect.rect.height / 2 - spacing - tileSize / 2;

        // 그리드 라인 그리기
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

        // 셀 중심점 및 아이템 표시
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector3 center = new Vector3(startX + x * (tileSize + spacing), startY - y * (tileSize + spacing), 0f);
                if (grid[x, y] != null)
                {
                    Gizmos.color = Color.green; // 아이템이 있는 경우 초록색
                    Gizmos.DrawSphere(center, tileSize / 4);
                }
                else
                {
                    Gizmos.color = Color.red; // 아이템이 없는 경우 빨간색
                    Gizmos.DrawWireSphere(center, tileSize / 4);
                }
            }
        }
    }
    // 디버그 그리드 그리기
    //private void OnDrawGizmos()
    //{
    //    if (!showDebugGrid) return;

    //    // 그리드 라인 그리기
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

    //    // 셀 중심점 표시
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