using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using static SaveData;
using System.Linq;
using static UnityEditor.Progress;
using UnityEngine.SceneManagement;

public class GameManager : BaseManager
{
    //public static GameManager Instance { get; private set; }

    public const int GRID_WIDTH = 7;
    public const int GRID_HEIGHT = 9;
    [Header("Script References")]
    public InfoPanelController infoPanelController;
    [Header("Energy Settings")]
    [SerializeField] private int maxEnergy = 100;
    public int MaxEnergy => maxEnergy;
    [SerializeField] private float energyRegenRate = 600f;
    private float energyRegenRemainSec = 0f;
    public float EnergyRegenRemainSec => energyRegenRemainSec;
    private bool isEnergyRegening = false;
    public bool IsEnergyRegening => isEnergyRegening;
    [SerializeField] private int energyRegenAmount = 1;
    private int currentEnergy;
    public int CurrentEnergy => currentEnergy;
    [Header("Gold Settings")]
    [SerializeField] private int currentGold = 100;
    public int CurrentGold => currentGold;

    [Header("Prefab References")]
    [SerializeField] private GameObject itemPrefab;
    //[SerializeField] private GameObject generatorPrefab;

    [Header("SO References")]
    //[SerializeField] private ItemSO[] itemDatas;
    [SerializeField] private string[] itemIds = {"N001", "N002"};
    //[SerializeField] private GeneratorSO[] genDatas;
    [SerializeField] private string[] genIds = {"G001", "G002"};
    [SerializeField] public CraftingDB craftingDB;

    [Header("Guest Referemces")]
    private GameObject guestBoard;
    [SerializeField] private GameObject guestPrefab;
    [SerializeField] private Sprite[] guestSprites;

    [Header("Game Events")]
    public UnityEvent<int> onEnergyChanged;
    public UnityEvent<int> onEnergyRegenTimeChanged;
    public UnityEvent<int> onGoldChanged;
    public UnityEvent<int> onScoreChanged;
    public UnityEvent<MergeableItem> onItemMerged;
    public UnityEvent<MergeableItem> onItemSpawned;

    
    private int currentScore;
    private Dictionary<string, ItemSO> itemDataDic = new Dictionary<string, ItemSO>();
    private Dictionary<string, GeneratorSO> genDataDic = new Dictionary<string, GeneratorSO>();
    [SerializeField] private ObjectPool<GameObject> itemPool;

    [Header("Bool Values")]
    private bool isGamePaused;
    public bool IsDataLoaded { get; private set; } = false;
    public void OnClickSetMergeBoardTest()
    {
        SpawnGenerator("G001", 1, (Vector2Int)Managers.Grid.GetEmptyPosition());
        SpawnGenerator("G002", 1, (Vector2Int)Managers.Grid.GetEmptyPosition());
    }
    private void Awake()
    {
    }
    public override void Init()
    {
        base.Init();
        InitializeGame();
    }
    private async void GetData()
    {
        IsDataLoaded = false; // 데이터 로드 시작 시 false로 설정

        string[] allItemIds = itemIds.Concat(genIds).ToArray();
        foreach (string id in allItemIds)
        {
            ItemSO itemData = await Managers.DB.LoadItem(id);
            itemDataDic[id] = itemData;
            Debug.Log($"{id} : {itemDataDic[id]} ");
        }
        foreach (string id in genIds)
        {
            GeneratorSO genData = await Managers.DB.LoadGenerator(id);
            genDataDic[id] = genData;
            Debug.Log($"{id} : {genDataDic[id]} ");
        }

        IsDataLoaded = true; // 데이터 로드 완료 시 true로 설정
    }
    private void InitializeGame()
    {
        if (!SceneManager.GetActiveScene().name.Equals(SceneManager.GetSceneByName("Main").name))
            return;

        //// 프리팹 딕셔너리 초기화
        //foreach (var itemData in itemDatas)
        //{
        //    itemDataDic[itemData.id] = itemData;
        //}

        //foreach (var genData in genDatas)
        //{
        //    genDataDic[genData.genId] = genData;
        //}

        // 오브젝트 풀 초기화
        itemPool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnToPool, OnDestroyPoolObject, true, 50, 100);
        guestBoard = GameObject.Find("GuestBoard");
        GetData();
        currentEnergy = maxEnergy;
        //LoadGame();
    }

    private void Start()
    {
        energyRegenRemainSec = energyRegenRate;
        FlagRegenEnergy(currentEnergy);
        onEnergyChanged.AddListener(FlagRegenEnergy);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateRandomGuest();
        }
    }
    #region Energy Management
    public void FlagRegenEnergy(int currentEnergy)
    {
        if (currentEnergy < maxEnergy)
        {
            if(!isEnergyRegening)
            {
                isEnergyRegening = true;
                InvokeRepeating(nameof(RegenerateEnergy), 1f, 1f);
                onEnergyRegenTimeChanged.Invoke(Mathf.RoundToInt(energyRegenRemainSec));
            }    
        }
        else
        {
            isEnergyRegening = false;
            CancelInvoke(nameof(RegenerateEnergy));
            onEnergyRegenTimeChanged.Invoke(Mathf.RoundToInt(energyRegenRemainSec));
        }
    }
    private void RegenerateEnergy()
    {
        Debug.Log(energyRegenRemainSec);
        energyRegenRemainSec -= 1f;
        onEnergyRegenTimeChanged.Invoke(Mathf.RoundToInt(energyRegenRemainSec));
        if (currentEnergy < maxEnergy && energyRegenRemainSec <= 0f)
        {
            AddEnergy(energyRegenAmount);
        }
    }

    public void AddEnergy(int amount)
    {
        energyRegenRemainSec = energyRegenRate;
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        onEnergyChanged?.Invoke(currentEnergy);
    }

    public bool TrySpendEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            onEnergyChanged?.Invoke(currentEnergy);
            return true;
        }
        return false;
    }

    #endregion

    #region Gold Management
    public void AddGold(int amount)
    {
        currentGold += amount;
        onGoldChanged?.Invoke(currentGold);
    }

    #endregion

    #region Score Management

    public void AddScore(int amount)
    {
        currentScore += amount;
        onScoreChanged?.Invoke(currentScore);
    }

    #endregion

    #region Item Management
    public ItemSO GetItemData(string itemId)
    {
        return itemDataDic[itemId];
    }
    public Sprite GetItemSprite(ItemKey key)
    {
        return itemDataDic[key.id].items[key.lv-1].itemSprite;
    }
    public string GetItemName(ItemKey key)
    {
        return itemDataDic[key.id].items[key.lv - 1].itemName;
    }
    public MergeableItem SpawnItem(string itemId, int level, Vector2Int position)
    {
        GameObject itemObj = itemPool.Get();
        //itemObj.transform.position = Managers.Grid.GetWorldPosition(position);

        MergeableItem item = itemObj.GetComponent<MergeableItem>();
        if (item != null)
        {
            item.itemData = itemDataDic[itemId];
            item.Initialize(level);
            item.GetComponent<RectTransform>().sizeDelta = new Vector2(Managers.Grid.TileSize * 0.9f, Managers.Grid.TileSize * 0.9f);
            Managers.Grid.PlaceItem(item, position);
            //onItemSpawned?.Invoke(item);
        }

        Managers.Grid.CheckGuestsOrder();
        return item;
    }
    public Generator SpawnGenerator(string itemId, int level, Vector2Int position)
    {
        MergeableItem item = SpawnItem(itemId, level, position);
        
        Generator tempGenerator = item.gameObject.AddComponent<Generator>();
        tempGenerator.genDB = genDataDic[itemId];
        tempGenerator.Initialize();

        DraggableItem draggableItem = item.gameObject.GetComponent<DraggableItem>();
        draggableItem.generator = tempGenerator;
        return tempGenerator;
    }
    public ItemKey? FindCraftingResult(MergeableItem componentItem1, MergeableItem componentItem2)
    {
        if (componentItem1 == null || componentItem2 == null || componentItem1 == componentItem2)
        {
            return null;
        }

        return craftingDB.FindCraftingResult(componentItem1.itemKey, componentItem2.itemKey);
    }
    public ItemKey[] FindCraftingComponents(ItemKey resultKey)
    {
        return craftingDB.FindCraftingComponents(resultKey);
    }
    public bool TryMergeItems(MergeableItem draggingItem, MergeableItem targetItem)
    {
        if (draggingItem.CanMergeWith(targetItem))
        {
            targetItem.LevelUp();
            Managers.Grid.DetatchItemFromGrid(draggingItem.GridPosition);
            if (draggingItem.itemData.type == ItemType.Generatable)
            {
                draggingItem.GetComponent<Generator>().OnReuturnToItemPool();
            }
            ReturnItemToPool(draggingItem.gameObject);
            return true;
        }

        //// 크래프팅 체크
        //else if(FindCraftingResult(draggingItem, targetItem) != null)
        //{
        //    ItemKey crftedItemKey = FindCraftingResult(draggingItem, targetItem).Value;
        //    Vector2Int mergePosition = targetItem.GridPosition;
        //    Managers.Grid.DetatchItemFromGrid(draggingItem.GridPosition);
        //    ReturnItemToPool(draggingItem.gameObject);
        //    Managers.Grid.DetatchItemFromGrid(targetItem.GridPosition);
        //    ReturnItemToPool(targetItem.gameObject);
        //    SpawnItem(crftedItemKey.id, crftedItemKey.lv, mergePosition);
        //    return true;
        //}





        //AddScore(CalculateMergeScore(mergedItem.Lv));
        //onItemMerged?.Invoke(mergedItem);


        return false;
    }
    //public bool TryMergeItems(MergeableItem item1, MergeableItem neighbor)
    //{
    //    if (!item1.CanMergeWith(neighbor))
    //    {
    //        return false;
    //    }

    //    //Vector2Int mergePosition = Managers.Grid.GetGridPosition(item1.transform.position);
    //    Vector2Int mergePosition = neighbor.GridPosition;
    //    // 다음 레벨 아이템 생성
    //    MergeableItem mergedItem = SpawnItem(item1.itemData.id, item1.Lv + 1, mergePosition);
    //    if (mergedItem != null)
    //    {
    //        // 기존 아이템 제거
    //        //Managers.Grid.RemoveItem(Managers.Grid.GetGridPosition(item1.transform.position));
    //        //Managers.Grid.RemoveItem(Managers.Grid.GetGridPosition(item2.transform.position));
    //        AddScore(CalculateMergeScore(mergedItem.Lv));

    //        Managers.Grid.RemoveItem(item1.GridPosition);
    //        //Managers.Grid.RemoveItem(neighbor.GridPosition);

    //        ReturnItemToPool(item1.gameObject);
    //        ReturnItemToPool(neighbor.gameObject);

    //        //Managers.Grid.PlaceItem(mergedItem, mergePosition);
    //        // 점수 추가

    //        //onItemMerged?.Invoke(mergedItem);

    //        return true;
    //    }

    //    return false;
    //}

    private int CalculateMergeScore(int level)
    {
        // 레벨이 높아질수록 더 많은 점수를 얻도록 설정
        return Mathf.RoundToInt(Mathf.Pow(2, level) * 10);
    }
    // 현재 보유 중인 제너레이터들의 제너레이터 데이터를 확인하여 만들 수 있는 아이템 ID를 반환하는 메서드
    public List<string> GetAvailableItemIds()
    {
        HashSet<string> availableItemIds = new HashSet<string>();


        // 현재 보유 중인 제너레이터들을 확인
        foreach (Generator generator in Managers.Grid.FindAllGenerators())
        {
            foreach (var item in generator.genData.generatableItems)
            {
                availableItemIds.Add(item.key.id);
            }
            //GeneratorDB generatorData = generator.genDB;

            //// 제너레이터의 레벨에 따른 생성 가능한 아이템 ID를 추가
            //foreach (var data in generatorData.generatorDatas)
            //{
            //    foreach (var item in data.generatableItems)
            //    {
            //        availableItemIds.Add(item.key.id);
            //    }
            //}
        }

        ////나중에 현재 제너레이터 상황에 따라 생성가능한 크래프트아이템만 추가하도록 로직 수정
        //foreach(var item in itemDatas)
        //{
        //    if (item.type == ItemType.Crafted)
        //    {
        //        availableItemIds.Add(item.id);
        //    }
        //}

        return availableItemIds.ToList();
    }
    //public UnityAction PrintGeneratableGeneratorDesc(ItemKey inputKey)
    //{
    //    // 현재 보유 중인 제너레이터들을 확인
    //    foreach (var generator in Managers.Grid.FindAllGenerators())
    //    {
    //        GeneratorSO generatorData = generator.genDB;

    //        //input아이템을 생성할 수 있는 제너레이터의 설명을 프린트하는 함수를 반환
    //        foreach (var levelData in generatorData.generatorDatas)
    //        {
    //            foreach (var item in levelData.generatableItems)
    //            {
    //                if(inputKey.id == item.key.id)
    //                {

    //                   return generator.GetComponent<DraggableItem>().PrintGeneratorDesc;
    //                }
    //            }
    //        }
    //    }
    //    return null;
    //}

    #endregion

    #region Guest Management
    public void CreateRandomGuest()
    {
        Guest guest = Instantiate(guestPrefab, guestBoard.transform).GetComponent<Guest>();

        int count = Random.Range(1, 3);
        ItemKey[] tempItems = new ItemKey[count];
        List<string> availableItems = GetAvailableItemIds();
        if(availableItems.Count <= 0)
        {
            Debug.LogError("제너레이터가 없음");
            return;
        }
        Dictionary<ItemKey, int> goalItems = new Dictionary<ItemKey, int>();
        for (int i = 0; i < count; i++)
        {
            tempItems[i].id = availableItems[Random.Range(0, availableItems.Count)];
            //tempItems[i].id = "N001";
            tempItems[i].lv = Random.Range(2, 4);

            goalItems[tempItems[i]] = Random.Range(1, 3);
        }
        int goldAmount = Random.Range(1, 4);
        goldAmount *= count;
        guest.Init(goalItems, goldAmount);
    }
    #endregion

    #region Object Pooling

    private GameObject CreatePooledItem()
    {
        GameObject obj = Instantiate(itemPrefab); // 기본 아이템 프리팹
        obj.SetActive(false);
        return obj;
    }

    private void OnTakeFromPool(GameObject obj)
    {
        obj.SetActive(true);
    }

    private void OnReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroyPoolObject(GameObject obj)
    {
        Destroy(obj);
    }

    public void ReturnItemToPool(GameObject item)
    {
        itemPool.Release(item);
    }

    #endregion

    #region Save/Load System

    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            energy = currentEnergy,
            score = currentScore
        };

        // 현재 그리드의 모든 아이템 저장
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                MergeableItem item = Managers.Grid.GetItemAt(new Vector2Int(x, y));
                if (item != null)
                {
                    saveData.items.Add(new SaveData.ItemData
                    {
                        itemId = item.itemData.id,
                        level = item.Lv,
                        position = new Vector2Int(x, y)
                    });
                }
            }
        }

        Managers.Save.SaveGame(saveData);
    }

    public void LoadGame()
    {
        SaveData saveData = Managers.Save.LoadGame();
        if (saveData != null)
        {
            currentEnergy = saveData.energy;
            currentScore = saveData.score;

            // 저장된 아이템들 복원
            foreach (var itemData in saveData.items)
            {
                SpawnItem(itemData.itemId, itemData.level, itemData.position);
            }

            onEnergyChanged?.Invoke(Mathf.RoundToInt(currentEnergy));
            onScoreChanged?.Invoke(currentScore);
        }
    }

    #endregion

    #region Game State Management

    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1;
    }

    public void RestartGame()
    {
        // 그리드 초기화
        Managers.Grid.ClearGrid();

        // 게임 상태 초기화
        currentEnergy = maxEnergy;
        currentScore = 0;

        // 이벤트 발송
        onEnergyChanged?.Invoke(Mathf.RoundToInt(currentEnergy));
        onScoreChanged?.Invoke(currentScore);

        ResumeGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    #endregion
}

// ObjectPool.cs - 제네릭 오브젝트 풀 클래스
public class ObjectPool<T>
{
    private readonly System.Func<T> createFunc;
    private readonly System.Action<T> actionOnGet;
    private readonly System.Action<T> actionOnRelease;
    private readonly System.Action<T> actionOnDestroy;
    private readonly int maxSize;

    private readonly Stack<T> pool;
    private readonly bool collectionCheck;

    public ObjectPool(System.Func<T> createFunc,
        System.Action<T> actionOnGet = null,
        System.Action<T> actionOnRelease = null,
        System.Action<T> actionOnDestroy = null,
        bool collectionCheck = true,
        int defaultCapacity = 10,
        int maxSize = 10000)
    {
        this.createFunc = createFunc;
        this.actionOnGet = actionOnGet;
        this.actionOnRelease = actionOnRelease;
        this.actionOnDestroy = actionOnDestroy;
        this.maxSize = maxSize;
        this.collectionCheck = collectionCheck;

        pool = new Stack<T>(defaultCapacity);
    }

    public T Get()
    {
        T item;
        if (pool.Count == 0)
        {
            item = createFunc();
        }
        else
        {
            item = pool.Pop();
        }

        actionOnGet?.Invoke(item);
        return item;
    }

    public void Release(T item)
    {
        if (collectionCheck && pool.Count > 0 && pool.Contains(item))
        {
            throw new System.InvalidOperationException("Trying to release an item that has already been released to the pool.");
        }

        actionOnRelease?.Invoke(item);

        if (pool.Count < maxSize)
        {
            pool.Push(item);
        }
        else
        {
            actionOnDestroy?.Invoke(item);
        }
    }

    public void Clear()
    {
        if (actionOnDestroy != null)
        {
            foreach (T item in pool)
            {
                actionOnDestroy(item);
            }
        }

        pool.Clear();
    }
}