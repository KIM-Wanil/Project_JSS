using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridManager : BaseManager
{
    private float boardWidth;
    private float boardHeight;
    public float TileSize { get; private set; }
    private float spacing = 5f;
    //[SerializeField] private bool showDebugGrid = true;
    [SerializeField] private GameObject tilePrefab; // 타일 프리팹 추가

    [SerializeField] private MergeableItem[,] grid;
    //public List<ItemOrdered> itemOrdereds = new List<ItemOrdered>();
    public List<Guest> currentGuests = new List<Guest>();
    private Vector2 gridStartPosition = new Vector2(0f, 0f);
    private GameObject[,] tiles; // 타일 배열 추가
    private Vector2[,] tilePositions;
    public GameObject mergeBoard;
    private RectTransform mergeBoardRectT;
    public GameObject MergeBoard => mergeBoard;
    public RectTransform MergeBoardRectT => mergeBoardRectT;
    public MergeEffect mergeTryEffect;

    public int Width => GameManager.GRID_WIDTH;
    public int Height => GameManager.GRID_HEIGHT;


    public Dictionary<ItemKey, List<Vector2Int>> ownedNormalItems = new Dictionary<ItemKey, List<Vector2Int>>();
    private Dictionary<MergeableItem, Tween> itemTweens = new Dictionary<MergeableItem, Tween>();

    public bool isInit = false;
    //private Coroutine mouseCheckCoroutine;
    //private bool isMouseMoving;
    //private MergeableItem itemToAnnounce1;
    //private MergeableItem itemToAnnounce2;

    //DG.Tweening.Sequence announceMovingSequence;
    //private float mouseIdleTime = 0f;
    //private const float idleThreshold = 2f;
    // OnGUI 메서드 추가

    public override void Init()
    {
        //Debug.Log(SceneManager.GetActiveScene().name);
        //Debug.Log(SceneManager.GetSceneByName("Main").name);
        //if (!SceneManager.GetActiveScene().name.Equals(SceneManager.GetSceneByName("Main").name))
        //    return;
        //Debug.Log("GridManager initialized");
        if (isInit) return; // 이미 초기화된 경우 중복 초기화 방지
        Debug.Log("GridManager initialized");
        base.Init();
        InitializeGrid();
        GenerateTiles(); // 타일 생성 호출
                         //Managers.Game.SpawnGenerator("gen_anvil", 1, (Vector2Int)GetEmptyPosition());
                         //Managers.Game.SpawnGenerator("gen_anvil", 1, (Vector2Int)GetEmptyPosition());
                         //Managers.Game.SpawnGenerator("gen_orb", 1, (Vector2Int)GetEmptyPosition());
                         //Managers.Game.SpawnGenerator("gen_pot", 1, (Vector2Int)GetEmptyPosition());

        //mergeTryEffect = GameObject.Find("MergeTryEffect").GetComponent<MergeEffect>();
        if (mergeTryEffect.IsUnityNull())
        {
            Debug.LogError("MergeEffect not found!");
            return;
        }
        mergeTryEffect.transform.SetAsLastSibling();
        isInit = true;

        //Managers.Game.SpawnItem("N001", 2, new Vector2Int(0,0), true);
        //Managers.Game.SpawnItem("N002", 2, new Vector2Int(1,0), true);
    }

    private void InitializeGrid()
    {
        grid = new MergeableItem[Width, Height];
    }

    private void GenerateTiles()
    {


        //mergeBoard = GameObject.Find("MergeBoard");
        mergeBoardRectT = mergeBoard.GetComponent<RectTransform>();
        if (mergeBoard.IsUnityNull())
        {
            Debug.LogError("MergeBoard not found!");
            return;
        }
        tiles = new GameObject[Width, Height];
        tilePositions = new Vector2[Width, Height];

        // mergeBoard의 크기 가져오기
        RectTransform mergeBoardRect = mergeBoard.GetComponent<RectTransform>();
        boardWidth = mergeBoardRect.rect.width;
        //boardHeight = mergeBoardRect.rect.height;
        boardHeight = boardWidth * 9 / 7;
        Debug.Log($"Board Size: {boardWidth} x {boardHeight}");
        // 타일 크기 계산
        float totalSpacingX = (Width + 1) * spacing;
        float totalSpacingY = (Height + 1) * spacing;
        float tileSizeX = (boardWidth - totalSpacingX) / Width;
        float tileSizeY = (boardHeight - totalSpacingY) / Height;
        TileSize = Mathf.Min(tileSizeX, tileSizeY);

        // 그리드 시작 위치 계산 (가운데 정렬)
        float startX = -boardWidth / 2 + spacing + TileSize / 2;
        float startY = boardHeight / 2 - spacing - TileSize / 2; // 위쪽에서 시작

        // 여백 계산
        float extraSpaceX = (boardWidth - (Width * TileSize + (Width + 1) * spacing)) / 2;
        float extraSpaceY = (boardHeight - (Height * TileSize + (Height + 1) * spacing)) / 2;

        gridStartPosition = new Vector2(startX + extraSpaceX, startY - extraSpaceY); // 위쪽에서 시작

        for (int col = 0; col < Width; col++)
        {
            for (int row = 0; row < Height; row++)
            {
                Vector3 position = new Vector3(col * (TileSize + spacing), -row * (TileSize + spacing), 0) + (Vector3)gridStartPosition; // y 좌표를 반대로 계산
                GameObject tileObject = Instantiate(tilePrefab, position, Quaternion.identity, mergeBoard.transform);
                char c = (char)('A' + col);
                tileObject.name = $"Tile_{c}{row}";
                // 타일 크기 조정
                RectTransform tileRect = tileObject.GetComponent<RectTransform>();
                if (tileRect != null)
                {
                    tileRect.sizeDelta = new Vector2(TileSize, TileSize);
                    tileRect.anchoredPosition = position;
                }

                Tile tile = tileObject.GetComponent<Tile>();
                if (tile != null)
                {
                    tile.Initialize(new Vector2Int(col, row));
                }

                tiles[col, row] = tileObject;
                //tilePositions[i, j] = tileObject.transform.position;
                tilePositions[col, row] = tileRect.anchoredPosition;

                //타일 체크무늬로 보이게 띄엄띄엄 표시
                //if((i+j) %2 ==0)
                //{
                //    tileObject.transform.GetChild(0).gameObject.SetActive(true);
                //}
                //else
                //{
                //   tileObject.transform.GetChild(0).gameObject.SetActive(false);
                //}
            }
        }
    }
    //private void Update()
    //{
    //    //if (Input.GetMouseButtonDown(0))
    //    //{
    //    //    isMouseMoving = true;
    //    //    if (mouseCheckCoroutine != null)
    //    //    {
    //    //        StopCoroutine(mouseCheckCoroutine);
    //    //        itemToAnnounce1 = null;
    //    //        itemToAnnounce2 = null;
    //    //        if (announceMovingSequence != null)
    //    //        {
    //    //            announceMovingSequence.Kill();
    //    //        }
    //    //    }
    //    //    ResetItemsPosition();
    //    //    StartMouseCheck();
    //    //}

    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        isMouseMoving = true;
    //        if (mouseCheckCoroutine != null)
    //        {
    //            StopCoroutine(mouseCheckCoroutine);
    //            itemToAnnounce1 = null;
    //            itemToAnnounce2 = null;
    //            if (announceMovingSequence != null)
    //            {
    //                announceMovingSequence.Kill();
    //            }
    //        }
    //        ResetItemsPosition();
    //        StartMouseCheck();
    //        mouseIdleTime = 0f; // 마우스가 눌렸을 때 타이머 초기화
    //    }
    //    else if (Input.GetMouseButton(0))
    //    {
    //        mouseIdleTime = 0f; // 마우스가 눌린 상태일 때 타이머 초기화
    //    }
    //    else
    //    {
    //        mouseIdleTime += Time.deltaTime; // 마우스가 눌리지 않았을 때 타이머 증가
    //        if (mouseIdleTime >= idleThreshold)
    //        {
    //            if (mouseCheckCoroutine == null)
    //            {
    //                StartMouseCheck();
    //            }
    //        }
    //    }
    //}

    //private void StartMouseCheck()
    //{
    //    isMouseMoving = false;
    //    mouseCheckCoroutine = StartCoroutine(CheckMouseMovement());
    //}

    //private IEnumerator CheckMouseMovement()
    //{
    //    Debug.Log("CheckMouseMovement Start");
    //    yield return new WaitForSeconds(idleThreshold);
    //    if (!isMouseMoving)
    //    {
    //        (Vector2Int, Vector2Int)? mergeablePosPair = FindMergeablePosPair();
    //        //foreach(var a in ownedItemsCanBeMerged)
    //        //{
    //        //   Debug.Log($"key:{a.Key.id} (LV:{a.Key.lv})");
    //        //    foreach(var b in a.Value)
    //        //    {
    //        //        Debug.Log($"value:{b}");
    //        //    }
    //        //}
    //        if (mergeablePosPair.HasValue)
    //        {
    //            Debug.Log("MergeablePosPair Found");
    //            StartCoroutine(PerformMovementEffect(mergeablePosPair.Value.Item1, mergeablePosPair.Value.Item2));
    //        }
    //    }
    //}

    //private IEnumerator PerformMovementEffect(Vector2Int pos1, Vector2Int pos2)
    //{
    //    Debug.Log("PerformMovementEffect");
    //    itemToAnnounce1 = GetItemAt(pos1);
    //    if (itemToAnnounce1 == null)
    //    {
    //        Debug.LogError("itemToAnnounce1 is null");
    //        yield break;
    //    }
    //    itemToAnnounce2 = GetItemAt(pos2);
    //    if (itemToAnnounce2 == null)
    //    {
    //        Debug.LogError("itemToAnnounce2 is null");
    //        yield break;
    //    }

    //    Vector2 tilePos1 = tilePositions[pos1.x, pos1.y];
    //    Vector2 tilePos2 = tilePositions[pos2.x, pos2.y];

    //    Vector2 direction1 = (tilePos2 - tilePos1).normalized * 10f;
    //    Vector2 direction2 = (tilePos1 - tilePos2).normalized * 10f;

    //    Debug.Log($"tilePos1:{tilePos1}/tilePos2:{tilePos2}/direction1:{direction1}/direction2:{direction2}");
    //    if (announceMovingSequence != null)
    //    {
    //        announceMovingSequence.Kill();
    //    }
    //    announceMovingSequence = DG.Tweening.DOTween.Sequence();

    //    announceMovingSequence.Append(itemToAnnounce1.itemRectT.DOAnchorPos(direction1, 0.5f));
    //    announceMovingSequence.Join(itemToAnnounce2.itemRectT.DOAnchorPos(direction2, 0.5f));
    //    announceMovingSequence.Append(itemToAnnounce1.itemRectT.DOAnchorPos(Vector2.one, 0.5f));
    //    announceMovingSequence.Join(itemToAnnounce2.itemRectT.DOAnchorPos(Vector2.one, 0.5f));

    //    announceMovingSequence.Append(itemToAnnounce1.itemRectT.DOAnchorPos(direction1, 0.5f));
    //    announceMovingSequence.Join(itemToAnnounce2.itemRectT.DOAnchorPos(direction2, 0.5f));
    //    announceMovingSequence.Append(itemToAnnounce1.itemRectT.DOAnchorPos(Vector2.one, 0.5f));
    //    announceMovingSequence.Join(itemToAnnounce2.itemRectT.DOAnchorPos(Vector2.one, 0.5f));

    //    announceMovingSequence.AppendInterval(0.5f);
    //    announceMovingSequence.SetLoops(-1);
    //    announceMovingSequence.Play();

    //    while (!isMouseMoving)
    //    {
    //        yield return null;
    //    }

    //    announceMovingSequence.Kill();
    //    ResetItemsPosition();
    //}

    //private void ResetItemsPosition()
    //{
    //    if (itemToAnnounce1 != null && itemToAnnounce2 != null)
    //    {
    //        itemToAnnounce1.itemRectT.anchoredPosition = Vector3.zero;
    //        itemToAnnounce2.itemRectT.anchoredPosition = Vector3.zero;
    //        itemToAnnounce1 = null;
    //        itemToAnnounce2 = null;
    //    }
    //}
    public void PlayMergeEffect(Vector2Int position)
    {
        mergeTryEffect.PlayEffect();
        mergeTryEffect.rectTransform.anchoredPosition = GetTilePosition(position);
    }
    public void StopMergeEffect()
    {
        mergeTryEffect.Init();
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
                    Managers.Game.ReturnItemToPool(item);
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

        //grid[fromPosition.x, fromPosition.y] = null;
        DetatchItemFromGrid(fromPosition);

        //grid[toPosition.x, toPosition.y] = item;
        AttatchItemToGrid(item, toPosition);

        item.transform.position = GetTilePosition(toPosition);
        //item.SetGridPosition(toPosition);

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
    public Vector2Int? GetNearestPosition(Vector2Int startPos)
    {
        // BFS를 위한 큐와 방문 기록
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        // 초기 위치를 큐에 추가
        queue.Enqueue(startPos);
        visited.Add(startPos);

        // 방향 벡터 (상, 좌, 우, 하, 좌상, 우상, 좌하, 우하)
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, 1),   // 상
        new Vector2Int(-1, 0),  // 좌
        new Vector2Int(1, 0),   // 우
        new Vector2Int(0, -1),  // 하
        new Vector2Int(-1, 1),  // 좌상
        new Vector2Int(1, 1),   // 우상
        new Vector2Int(-1, -1), // 좌하
        new Vector2Int(1, -1)   // 우하
        };

        while (queue.Count > 0)
        {
            Vector2Int currentPos = queue.Dequeue();

            // 현재 위치가 유효하고 빈 위치인지 확인
            if (IsValidPosition(currentPos) && IsEmptyPosition(currentPos))
            {
                return currentPos;
            }

            // 인접한 모든 방향을 검사
            foreach (var direction in directions)
            {
                Vector2Int newPos = currentPos + direction;

                // 그리드 범위 내에 있고 방문하지 않은 위치만 큐에 추가
                if (newPos.x >= 0 && newPos.x < Width &&
                    newPos.y >= 0 && newPos.y < Height &&
                    !visited.Contains(newPos))
                {
                    queue.Enqueue(newPos);
                    visited.Add(newPos);
                }
            }
        }
        // 빈 위치를 찾지 못한 경우 null 반환
        return null;
    }
    public void OpenNearBox(Vector2Int startPos)
    {
        // 방향 벡터 (상, 좌, 우, 하)
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // 상
            new Vector2Int(-1, 0),  // 좌
            new Vector2Int(1, 0),   // 우
            new Vector2Int(0, -1),  // 하
        };

        int count = 0;

        // 인접한 모든 방향을 검사
        foreach (var direction in directions)
        {
            Vector2Int newPos = startPos + direction;
            if (IsValidPosition(newPos) && !IsEmptyPosition(newPos))
            {
                MergeableItem item = grid[newPos.x, newPos.y];
                if (item.state == ItemState.InBox)
                {
                    count++;
                    item.Initialize(item.Lv, newPos, ItemState.Locked);
                    AddOwnedItemsCanBeMerged(item);
                }
            }
        }

        if (count > 0)
        {
            ////소리 별로여서 보류
            //Managers.Asset.PlaySound("Box_Crash", SoundType.Effect);
        }
        // 빈 위치를 찾지 못한 경우 null 반환
        return;
    }
    // 그리드 위치를 월드 좌표로 변환
    public Vector3 GetTilePosition(Vector2Int gridPosition)
    {
        //float x = gridStartPosition.x + gridPosition.x * (TileSize + spacing);
        //float y = gridStartPosition.y - gridPosition.y * (TileSize + spacing); // y 좌표를 반대로 계산
        //return new Vector3(x, y, 0);

        return tilePositions[gridPosition.x, gridPosition.y];
    }
    public Vector3 GetTileWorldPosition(Vector2Int gridPosition)
    {
        //float x = gridStartPosition.x + gridPosition.x * (TileSize + spacing);
        //float y = gridStartPosition.y - gridPosition.y * (TileSize + spacing); // y 좌표를 반대로 계산
        //return new Vector3(x, y, 0);

        return tiles[gridPosition.x, gridPosition.y].transform.position;
    }


    // 월드 좌표를 그리드 위치로 변환
    public Vector2Int? GetGridPosition(Vector3 tilePosition)
    {
        Vector2 localPosition = (Vector2)tilePosition - gridStartPosition;

        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt(localPosition.x / (TileSize + spacing)),
            Mathf.RoundToInt(-localPosition.y / (TileSize + spacing)) // y 좌표를 반대로 계산
        );

        if (IsValidPosition(gridPos))
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
    //public bool PlaceMoveItem(MergeableItem item, Vector2 startWorldPosition, Vector2Int targetGridposition)
    //{
    //    // 기존 트윈 애니메이션 중지 및 제거
    //    if (itemTweens == null)
    //    {
    //        itemTweens = new Dictionary<MergeableItem, Tween>();
    //    }
    //    if (itemTweens.ContainsKey(item))
    //    {
    //        itemTweens[item].Kill();
    //        itemTweens.Remove(item);
    //    }

    //    // 애니메이션을 시작하기 전의 현재 실제 위치 저장
    //    Vector2 actualStartPosition = item.itemRectT.anchoredPosition;

    //    // 그리드에 아이템 논리적 배치 (중복 방지를 위해 필요)
    //    AttatchItemToGrid(item, targetGridposition);

    //    Vector2 targetTilePosition = GetTilePosition(targetGridposition);
    //    Debug.Log($"startTilePosition:{actualStartPosition}/targetTilePosition:{targetTilePosition}");

    //    // 실제 현재 위치와 목표 위치 간의 거리로 계산
    //    float distance = Vector2.Distance(actualStartPosition, targetTilePosition);
    //    float moveDuration = distance * 0.1f;
    //    Debug.Log($"distance:{distance}/moveDuration:{moveDuration}");

    //    // 저장된 실제 시작 위치로 아이템 위치 설정 (시각적으로만)
    //    item.itemRectT.anchoredPosition = actualStartPosition;

    //    // 직선 이동 트윈 생성
    //    Tween moveTween = item.itemRectT.DOAnchorPos(targetTilePosition, moveDuration).SetEase(Ease.Linear);

    //    // 트윈 완료 시 실행할 코드
    //    moveTween.OnComplete(() =>
    //    {
    //        // 아이템을 타일의 자식으로 배치
    //        item.transform.SetParent(tiles[targetGridposition.x, targetGridposition.y].transform);
    //        // 아이템의 위치 설정
    //        item.itemRectT.localScale = Vector3.one;
    //        item.itemRectT.anchoredPosition = Vector3.zero;

    //        Managers.Grid.CheckGuestsOrder();
    //        // 트윈 애니메이션 제거
    //        itemTweens.Remove(item);
    //    });

    //    // 트윈 애니메이션 저장
    //    itemTweens[item] = moveTween;
    //    moveTween.Play();

    //    return true;
    //}
    public bool PlaceMoveItem(MergeableItem item, Vector2 startWorldPosition, Vector2Int targetGridposition)
    {
        if (item == null)
        {
            Debug.LogError("item is null in PlaceMoveItem method");
            return false;
        }
        // 기존 트윈 애니메이션 중지 및 제거
        if (itemTweens == null)
        {
            itemTweens = new Dictionary<MergeableItem, Tween>();
        }
        if (itemTweens.ContainsKey(item))
        {
            itemTweens[item].Kill();
            itemTweens.Remove(item);
        }
        item.draggableItem.SetInteractionEnabled(false);

        // 현재 아이템의 실제 월드 위치 저장
        Vector2 actualStartPosition = item.transform.position;

        // 그리드에 아이템 논리적 배치
        AttatchItemToGrid(item, targetGridposition);

        //Vector2 targetTilePosition = GetTilePosition(targetGridposition);
        Vector2 targetTilePosition = GetTileWorldPosition(targetGridposition);
        //Debug.Log($"startTilePosition:{actualStartPosition}/targetTilePosition:{targetTilePosition}");

        // 거리 계산 
        float distance = Vector2.Distance(targetTilePosition, actualStartPosition);

        // 해상도에 따른 정규화
        float normalizedDistance = distance / Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
        float moveDuration = normalizedDistance * 2.0f; // 0.5f는 기준 duration 시간

        //Debug.Log($"distance:{distance}/moveDuration:{moveDuration}");

        // DOMove를 사용 (DOAnchorPos 대신)
        Tween moveTween = item.transform.DOMove(targetTilePosition, moveDuration)
            .SetEase(Ease.OutQuad)
            .SetAutoKill(false)
            .OnComplete(() =>
            {
                item.draggableItem.SetInteractionEnabled(true);
                // 아이템을 타일의 자식으로 배치
                item.transform.SetParent(tiles[targetGridposition.x, targetGridposition.y].transform);
                // 아이템의 위치 설정
                item.rectT.localScale = Vector3.one;
                item.rectT.anchoredPosition = Vector3.zero;

                Managers.Grid.CheckGuestsOrder();
                // 트윈 애니메이션 제거
                itemTweens.Remove(item);
                //Debug.Log("스폰트윈종료");
            }
            );

        // 트윈 애니메이션 저장
        itemTweens[item] = moveTween;
        moveTween.Play();

        return true;
    }
    //public bool PlaceMoveItem(MergeableItem item, Vector2 startTilePosition, Vector2Int targetGridposition)
    //{
    //    item.transform.SetParent(mergeBoard.transform);
    //    item.itemRectT.anchoredPosition = startTilePosition;

    //    Vector2 targetTilePosition = GetTilePosition(targetGridposition);

    //    Debug.Log($"startTilePosition:{startTilePosition}/targetTilePosition:{targetTilePosition}");

    //    // 이동 시간 계산 (거리에 비례)
    //    float distance = Vector2.Distance(startTilePosition, targetTilePosition);
    //    float moveDuration = distance * 0.003f; // 속도 조절을 위해 5로 나눔
    //    Debug.Log($"distance:{distance}/moveDuration:{moveDuration}");

    //    // 1. 직선 벡터 구하기
    //    Vector2 direction = (targetTilePosition - startTilePosition).normalized;
    //    Vector2 perpendicularVector = new Vector2();
    //    if (direction.x > 0)
    //    {
    //        perpendicularVector = new Vector2(-direction.y, direction.x);
    //    }
    //    else
    //    {
    //        perpendicularVector = new Vector2(direction.y, -direction.x);
    //    }

    //    // 4. 벡터 정규화
    //    perpendicularVector = perpendicularVector.normalized;


    //    // 포물선 이동 시퀀스 생성
    //    DG.Tweening.Sequence sequence = DOTween.Sequence();

    //    // 포물선 경로 설정
    //    Vector2 controlPoint = new Vector2();
    //    if (Mathf.Abs(startTilePosition.x - targetTilePosition.x) < TileSize * 0.5f)
    //    {
    //        controlPoint = (startTilePosition + targetTilePosition) * 0.5f;
    //    }
    //    else
    //    {
    //        controlPoint = (startTilePosition + targetTilePosition) * 0.5f + perpendicularVector * distance * 0.2f;
    //    }
    //    Debug.Log($"controlPoint:{controlPoint}");

    //    // 반동 효과를 위한 추가 제어점 설정
    //    Vector2 overshootPoint = targetTilePosition + (direction * 5f); // 
    //    Debug.Log($"overshootPoint:{overshootPoint}");

    //    sequence.Append(item.itemRectT.DOLocalPath(new Vector3[] { startTilePosition, controlPoint, overshootPoint, targetTilePosition }, moveDuration, PathType.CatmullRom, PathMode.Ignore).SetEase(Ease.OutQuart));

    //    // 시퀀스 완료 시 실행할 코드
    //    sequence.OnComplete(() =>
    //    {
    //        // 아이템을 타일의 자식으로 배치
    //        item.transform.SetParent(tiles[targetGridposition.x, targetGridposition.y].transform);

    //        // 아이템의 위치 설정
    //        //item.transform.DOLocalMove(Vector3.zero, 0.5f).OnComplete(() =>

    //        item.itemRectT.localScale = Vector3.one;
    //        item.itemRectT.anchoredPosition = Vector3.zero;

    //        // 아이템의 Sibling Index를 타일보다 앞에 배치
    //        item.transform.SetSiblingIndex(tiles[targetGridposition.x, targetGridposition.y].transform.GetSiblingIndex() + 1);


    //    });
    //    sequence.Play();
    //    // 그리드에 아이템 배치
    //    AttatchItemToGrid(item, targetGridposition);
    //    //grid[targetGridposition.x, targetGridposition.y] = item;
    //    //item.SetGridPosition(targetGridposition);
    //    return true;
    //}
    public bool PlaceItem(MergeableItem item, Vector2Int targetGridposition)
    {

        // 아이템을 타일의 자식으로 배치
        item.transform.SetParent(tiles[targetGridposition.x, targetGridposition.y].transform);
        // 아이템의 위치 설정
        item.transform.localPosition = Vector3.zero;
        // 아이템의 Sibling Index를 타일보다 앞에 배치
        item.transform.SetSiblingIndex(tiles[targetGridposition.x, targetGridposition.y].transform.GetSiblingIndex() + 1);

        // 그리드에 아이템 배치
        item.GetComponent<RectTransform>().localScale = Vector3.one;
        //grid[targetGridposition.x, targetGridposition.y] = item;
        AttatchItemToGrid(item, targetGridposition);

        return true;
    }

    // 아이템을 그리드에서 제거
    public void DetatchItemFromGrid(Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            MergeableItem item = GetItemAt(position);
            if (item.Lv <= item.itemData.items.Length && item.itemData.type != ItemType.Generatable && item.state != ItemState.InBox)
            {
                RemoveOwnedItemsCanBeMerged(item);
            }
            grid[position.x, position.y] = null;
        }
    }
    public void DetatchItemFromGrid(MergeableItem item)
    {
        if (IsValidPosition(item.gridPosition))
        {
            if (item.Lv < item.itemData.items.Length && item.itemData.type != ItemType.Generatable && item.state != ItemState.InBox)
            {
                RemoveOwnedItemsCanBeMerged(item);
            }
            grid[item.gridPosition.x, item.gridPosition.y] = null;
        }
    }
    public void AttatchItemToGrid(MergeableItem item, Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            item.SetGridPosition(position);
            //지금 리스트에 넣는 조건 :제너레이터가 아니고, 상자상태가 아닌 경우     
            //추후 변동 가능
            if ((item.itemData.type == ItemType.Normal || item.itemData.type == ItemType.Usable) && (item.state == ItemState.Normal))
            {
                //Debug.Log("AddOwnedItemsCanBeMerged");
                AddOwnedItemsCanBeMerged(item);
            }
            grid[position.x, position.y] = item;
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
                        new Vector2Int(0, 1)   // 아래쪽
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
    // 아이템이 생성되거나 합성될 때 호출되는 메서드
    public void AddOwnedItemsCanBeMerged(MergeableItem item)
    {
        var key = item.itemKey;
        if (!ownedNormalItems.ContainsKey(key))
        {
            ownedNormalItems[key] = new List<Vector2Int>();
        }
        if (!ownedNormalItems[key].Contains(item.gridPosition))
        {
            ownedNormalItems[key].Add(item.gridPosition);
        }
    }

    // 아이템이 제거될 때 호출되는 메서드
    public void RemoveOwnedItemsCanBeMerged(MergeableItem item)
    {
        var key = item.itemKey;
        if (ownedNormalItems.TryGetValue(key, out var positions))
        {
            positions.Remove(item.gridPosition);

            if (positions.Count == 0)
            {
                ownedNormalItems.Remove(key);
            }
        }
    }
    public (Vector2Int, Vector2Int)? FindNearestItemPairCanBeMerged(ItemKey itemKey)
    {
        if (ownedNormalItems.TryGetValue(itemKey, out var positions) && positions.Count >= 2)
        {
            List<Vector2Int> lockedPositions = new List<Vector2Int>();
            List<Vector2Int> unlockedPositions = new List<Vector2Int>();

            string itemName = Managers.Game.GetItemName(itemKey);
            // 모든 벡터의 상태를 확인하여 분류
            foreach (var pos in positions)
            {
                MergeableItem item = GetItemAt(pos);
                if (item == null)
                {
                    Debug.Log($"{itemName} : ({pos.x},{pos.y})에 아이템이 없음");
                    continue;
                }
                if (item.state == ItemState.Locked)
                {
                    lockedPositions.Add(pos);
                }
                else
                {
                    unlockedPositions.Add(pos);
                }
            }

            // 모든 벡터가 Locked 상태인 경우 null 반환
            if (lockedPositions.Count == positions.Count)
            {

                Debug.Log($"{itemName}은 모두 잠겨있어서 합성 불가");
                return null;
            }

            // 모든 벡터가 Locked 상태가 아닌 경우 기존 알고리즘 적용
            if (unlockedPositions.Count == positions.Count)
            {
                if (positions.Count == 2)
                {
                    //Debug.Log($"{itemName} : {positions[0]}, {positions[1]} - 모든 아이템이 잠겨있지않고, 아이템이 두 개 뿐.");
                    return (positions[0], positions[1]);
                }
                else if (positions.Count > 2)
                {
                    Vector2Int? closestPos1 = null;
                    Vector2Int? closestPos2 = null;
                    float minDistance = float.MaxValue;

                    for (int i = 0; i < positions.Count; i++)
                    {
                        for (int j = i + 1; j < positions.Count; j++)
                        {
                            float distance = Vector2Int.Distance(positions[i], positions[j]);

                            // 거리가 1이거나 대각선 1칸인 경우 바로 반환
                            if (distance == 1 || distance == Mathf.Sqrt(2))
                            {
                                //Debug.Log($"{itemName} : {positions[i]}, {positions[j]} - 모든 아이템이 잠겨있지않고, 거리가 1칸차이.");
                                return (positions[i], positions[j]);
                            }

                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestPos1 = positions[i];
                                closestPos2 = positions[j];
                            }
                        }
                    }

                    if (closestPos1.HasValue && closestPos2.HasValue)
                    {
                        //Debug.Log($"{itemName}: {closestPos1.Value}, {closestPos2.Value} - 모든 아이템이 잠겨있지않고, 최소 거리 반환");
                        return (closestPos1.Value, closestPos2.Value);
                    }
                }
            }

            // Locked인 벡터와 Locked가 아닌 벡터 사이의 최소 거리 계산
            Vector2Int? closestLockedPos = null;
            Vector2Int? closestUnlockedPos = null;
            float minLockedDistance = float.MaxValue;

            foreach (var lockedPos in lockedPositions)
            {
                foreach (var unlockedPos in unlockedPositions)
                {
                    float distance = Vector2Int.Distance(lockedPos, unlockedPos);

                    // 거리가 1이거나 대각선 1칸인 경우 바로 반환
                    if (distance == 1 || distance == Mathf.Sqrt(2))
                    {
                        //Debug.Log($"{itemName}: {lockedPos}, {unlockedPos} - 잠긴 아이템과 잠기지않은 아이템 중 거리가 1칸차이.");
                        return (lockedPos, unlockedPos);
                    }

                    if (distance < minLockedDistance)
                    {
                        minLockedDistance = distance;
                        closestLockedPos = lockedPos;
                        closestUnlockedPos = unlockedPos;
                    }
                }
            }

            if (closestLockedPos.HasValue && closestUnlockedPos.HasValue)
            {
                //Debug.Log($"{itemName}: {closestLockedPos.Value}, {closestUnlockedPos.Value} - 잠긴 아이템과 잠기지않은 아이템 중 최소 거리 반환");
                return (closestLockedPos.Value, closestUnlockedPos.Value);
            }
        }
        return null;
    }

    public (Vector2Int, Vector2Int)? FindMergeablePosPair()
    {
        //Debug.Log("FindMergeablePosPair");
        (Vector2Int, Vector2Int)? mergeblePosPair = null;

        // ownedItemsCanBeMerged 딕셔너리 얕은 복사
        var itemsToCheck = new Dictionary<ItemKey, List<Vector2Int>>(ownedNormalItems);

        foreach (var guest in currentGuests)
        {
            foreach (var item in guest.itemsOrdered)
            {
                if (item.IsFulfill)
                {
                    continue;
                }
                for (int i = item.key.lv - 1; i > 0; i--)
                {
                    ItemKey keyToFind = new ItemKey(item.key.id, i);
                    mergeblePosPair = FindNearestItemPairCanBeMerged(keyToFind);
                    if (mergeblePosPair.HasValue)
                    {
                        //Debug.Log("주문 아이템에 가까운 아이템 페어 찾음");
                        return mergeblePosPair;
                    }
                    else
                    {
                        // 복사한 딕셔너리에서 해당 아이템 제거
                        itemsToCheck.Remove(keyToFind);
                    }
                }


            }
        }

        if (mergeblePosPair == null)
        {
            foreach (var item in itemsToCheck)
            {
                //Debug.Log($"{Managers.Game.GetItemName(item.Key)}/아이템 레벨 {item.Key.lv}/최대 레벨{Managers.Game.GetItemMaxLevel(item.Key)}");
                //아이템이 최대 레벨인 경우 제외
                if (item.Key.lv == Managers.Game.GetItemMaxLevel(item.Key))
                {
                    continue;
                }
                mergeblePosPair = FindNearestItemPairCanBeMerged(item.Key);
                if (mergeblePosPair.HasValue)
                {
                    //Debug.Log("주문 아이템에 가까운 아이템 존재X 그냥 머지 가능한 아이템 페어 찾음");
                    return mergeblePosPair;
                }
            }
        }
        return null;
    }


    // 특정 위치 주변의 머지 가능한 아이템 찾기
    public MergeableItem FindMergeableNeighbor(Vector2Int position, MergeableItem item)
    {
        if (IsValidPosition(position))
        {
            MergeableItem neighbor = grid[position.x, position.y];
            //Debug.Log($"{position.x},{position.y}위치 : {neighbor}");
            //Debug.Log(item.CanMergeWith(neighbor));
            //if (neighbor != null && item.CanMergeWith(neighbor))
            //{
            //    return neighbor;
            //}
            return neighbor;
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
    public bool DoesItemExist(ItemKey item)
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
    public int CountNormalItem(ItemKey item)
    {
        if (!ownedNormalItems.ContainsKey(item)) return 0;

        int count = 0;
        foreach (var pos in ownedNormalItems[item])
        {
            MergeableItem tempItem = GetItemAt(pos);
            if (tempItem.state == ItemState.Normal)
            {
                tempItem.isCheck = true;
                tempItem.OnChecked();
                count++;
            }
        }
        return count;
    }
    // 아이템키로 머지판에서 노말아이템 찾아서 삭제 (퀘스트 완료시)
    public List<Vector2Int> FindNormalItemsFromGrid(ItemKey item, int goalCount)
    {
        List<Vector2Int> targetPositions = new List<Vector2Int>();
        if (ownedNormalItems.ContainsKey(item))
        {
            foreach (var pos in ownedNormalItems[item])
            {
                MergeableItem tempItem = GetItemAt(pos);
                if (tempItem.state != ItemState.Locked)
                {
                    targetPositions.Add(pos);
                    if (targetPositions.Count == goalCount)
                    {
                        return targetPositions;
                    }
                }
            }
            if (targetPositions.Count < goalCount)
            {
                return null;
            }
        }
        return null;
    }
    public void UncheckNormalItem(ItemKey item)
    {
        if (ownedNormalItems.ContainsKey(item))
        {
            foreach (var pos in ownedNormalItems[item])
            {
                MergeableItem tempItem = GetItemAt(pos);
                if (tempItem.isCheck)
                {
                    tempItem.isCheck = false;
                    tempItem.OnUnchecked();
                }
            }
        }
    }
    public DG.Tweening.Sequence RemoveItemFromGridToGuest(MergeableItem mergeableItem, Vector3 worldPosition)
    {
        DG.Tweening.Sequence sequence = DOTween.Sequence();
        sequence.Append(mergeableItem.rectT.DOMove(worldPosition, 0.5f));
        sequence.OnComplete(() =>
        {
            // 이동이 끝난 후 DetatchItemFromGrid와 ReturnItemToPool을 실행합니다.
            DetatchItemFromGrid(mergeableItem.gridPosition);
            Managers.Game.ReturnItemToPool(mergeableItem);
        });
        return sequence;
    }
    public DG.Tweening.Sequence RemoveItemFromGridToGuest(Vector2Int gridPos, Vector3 worldPosition)
    {
        MergeableItem mergeableItem = GetItemAt(gridPos);

        if (mergeableItem.draggableItem = DraggableItem.currentlySelectedItem)
        {
            DraggableItem.currentlySelectedItem.mergeableItem.OnDeSelected();
            DraggableItem.currentlySelectedItem = null;
            Managers.Game.DeSelecItem();
        }

        mergeableItem.transform.SetParent(mergeBoard.transform);
        DG.Tweening.Sequence sequence = DOTween.Sequence();
        sequence.Append(mergeableItem.rectT.DOMove(worldPosition, 0.5f));
        sequence.OnComplete(() =>
        {
            // 이동이 끝난 후 DetatchItemFromGrid와 ReturnItemToPool을 실행합니다.
            DetatchItemFromGrid(mergeableItem.gridPosition);
            Managers.Game.ReturnItemToPool(mergeableItem);
        });
        return sequence;
    }
    public void RemoveItemFromGridInstantly(MergeableItem mergeableItem)
    {
        DetatchItemFromGrid(mergeableItem.gridPosition);
        Managers.Game.ReturnItemToPool(mergeableItem);
    }
    public void RemoveItemFromGridInstantly(Vector2Int gridPos)
    {
        MergeableItem mergeableItem = GetItemAt(gridPos);
        DetatchItemFromGrid(gridPos);
        Managers.Game.ReturnItemToPool(mergeableItem);
    }

    public Vector2Int GetNearestEmptyPosition(Vector2Int targetGridPos)
    {

        //// 시작 위치가 빈 위치라면 바로 반환
        //if (IsEmptyPosition(targetGridPos))
        //{
        //    return targetGridPos;
        //}

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
        float startX = mergeBoardPosition.x - mergeBoardRect.rect.width / 2 + spacing + TileSize / 2;
        float startY = mergeBoardPosition.y + mergeBoardRect.rect.height / 2 - spacing - TileSize / 2;

        // 그리드 라인 그리기
        Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
        for (int x = 0; x <= Width; x++)
        {
            Vector3 start = new Vector3(startX + x * (TileSize + spacing), startY, 0f);
            Vector3 end = new Vector3(startX + x * (TileSize + spacing), startY - Height * (TileSize + spacing), 0f);
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= Height; y++)
        {
            Vector3 start = new Vector3(startX, startY - y * (TileSize + spacing), 0f);
            Vector3 end = new Vector3(startX + Width * (TileSize + spacing), startY - y * (TileSize + spacing), 0f);
            Gizmos.DrawLine(start, end);
        }

        // 셀 중심점 및 아이템 표시
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector3 center = new Vector3(startX + x * (TileSize + spacing), startY - y * (TileSize + spacing), 0f);
                if (grid[x, y] != null)
                {
                    Gizmos.color = Color.green; // 아이템이 있는 경우 초록색
                    Gizmos.DrawSphere(center, TileSize / 4);
                }
                else
                {
                    Gizmos.color = Color.red; // 아이템이 없는 경우 빨간색
                    Gizmos.DrawWireSphere(center, TileSize / 4);
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