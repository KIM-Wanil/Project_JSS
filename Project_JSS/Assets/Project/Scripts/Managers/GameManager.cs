using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using static SaveData;
using System.Linq;
using UnityEngine.SceneManagement;
using System;
using static UnityEngine.Tilemaps.Tilemap;

public class GameManager : BaseManager
{
    //public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject rippleEffectPrefab;
    [SerializeField] private Canvas cursorCanvas;
    public const int GRID_WIDTH = 7;
    public const int GRID_HEIGHT = 9;
    [Header("Script References")]
    //public InfoPanelController infoPanelController;
    [Header("Energy Settings")]
    [SerializeField] private int maxEnergy = 100;
    public int MaxEnergy => maxEnergy;
    [SerializeField] private float energyRegenRate = 600f;
    private float energyRegenRemainSec = 0f;
    public float EnergyRegenRemainSec => energyRegenRemainSec;
    private bool isEnergyRegening = false;
    public bool IsEnergyRegening => isEnergyRegening;
    [SerializeField] private int energyRegenAmount = 1;
    [Header("Gold Settings")]
    private int currentEnergy;
    public int CurrentEnergy => currentEnergy;
    private int currentGold;
    public int CurrentGold => currentGold;
    private int currentGem;
    public int CurrentGem => currentGem;
    public Queue<ItemKey> currentRewardQueue = new Queue<ItemKey>();
    [Header("Prefab References")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject generatorPrefab;
    private float generatorSyncTime;
    public float GeneratorSyncTime => generatorSyncTime;

    [Header("SO References")]
    //[SerializeField] private ItemSO[] itemDatas;
    [SerializeField] private string[] itemIds = {"N000", "N001", "N002"};
    //[SerializeField] private GeneratorSO[] genDatas;
    [SerializeField] private string[] genIds = {"G001", "G002"};

    

    

    [Header("Game Events")]
    public UnityEvent<int> onEnergyChanged;
    public UnityEvent<int> onEnergyRegenTimeChanged;
    public UnityEvent<Vector2,int,GoodsType> onGoldSpawned;
    public UnityEvent<int> onGoldChanged;
    public UnityEvent<int> onGemChanged;

    public UnityEvent<Queue<ItemKey>> onRewardQueueChanged;

    public UnityEvent<MergeableItem> onItemMerged;
    public UnityEvent<MergeableItem> onItemSpawned;

    public UnityEvent<ItemKey, int, UnityAction> onSellableItemSelected;
    public UnityEvent<ItemKey> onUnsellableItemSelected;
    public UnityEvent<ItemKey> onLockedItemSelected;
    public UnityEvent onItemDeSelected;
    public UnityEvent onRandomGuestCreated; 
    public UnityEvent<int, KeyValuePair<ItemKey, int>, KeyValuePair<ItemKey, int>?> onGuestCreated;




    private Dictionary<string, ItemSO> itemDataDic = new Dictionary<string, ItemSO>();
    private Dictionary<string, GeneratorSO> genDataDic = new Dictionary<string, GeneratorSO>();
    [SerializeField] private ObjectPool<GameObject> itemPool;
    [SerializeField] private ObjectPool<GameObject> generatorPool;   

    [Header("Bool Values")]
    private bool isGamePaused;
    public bool IsDataLoaded { get; private set; } = false;
    private void Start()
    {
        
    }
    public void Update()
    {
        generatorSyncTime = Time.time % 4f;
        if (Input.GetMouseButtonUp(0))
        {
            CreateRippleEffect();
        }
    }
    private void CreateRippleEffect()
    {
        GameObject ripple = Instantiate(rippleEffectPrefab, Input.mousePosition, Quaternion.identity, cursorCanvas.transform);
        ripple.transform.SetSiblingIndex(0);
    }
    //public void OnClickSetMergeBoardTest()
    //{
    //    SpawnMoveGenerator("G001", 1, new Vector2(-113f, 513f), (Vector2Int)Managers.Grid.GetEmptyPosition());
    //    SpawnMoveGenerator("G002", 1, new Vector2(-113f, 513f), (Vector2Int)Managers.Grid.GetEmptyPosition());
    //}
    public void SelectSellableItem(ItemKey inputKey, int price = -1, UnityAction onItemSold = null)
    {
        onSellableItemSelected.Invoke(inputKey, price, onItemSold);
    }
    public void SelectUnsellableItem(ItemKey inputKey)
    {
        onUnsellableItemSelected.Invoke(inputKey);
    }
    public void SelectLockedItem(ItemKey inputKey)
    {
        onLockedItemSelected.Invoke(inputKey);
    }
    public void DeSelecItem()
    {
        onItemDeSelected.Invoke();
    }
    private void Awake()
    {
    }
    public override void Init()
    {
        if (!SceneManager.GetActiveScene().name.Equals(SceneManager.GetSceneByName("Main").name))
            return;
        base.Init();
        energyRegenRemainSec = energyRegenRate;
        FlagRegenEnergy(currentEnergy);
        onEnergyChanged.AddListener(FlagRegenEnergy);

        InitializeGame();


        
    }
    private async void LoadData()
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
        LoadGame();
        
    }
    private void InitializeGame()
    {
        
        // 아이템 오브젝트 풀 초기화
        itemPool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnToPool, OnDestroyPoolObject, true, 50, 100);

        // 제너레이터 오브젝트 풀 초기화
        generatorPool = new ObjectPool<GameObject>(CreatePooledGenerator, OnTakeFromPool, OnReturnToPool, OnDestroyPoolObject, true, 10, 20);
        
        LoadData();

        generatorSyncTime = Time.time % 4f; // 4초 주기로 동기화
        //LoadGame();
    }

    public void EnqueueReward(ItemKey rewardKey)
    {
        currentRewardQueue.Enqueue(rewardKey);
        onRewardQueueChanged?.Invoke(currentRewardQueue);
    }
    public void DequeueReward()
    {
        currentRewardQueue.Dequeue();
        onRewardQueueChanged?.Invoke(currentRewardQueue);
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
        //Debug.Log(energyRegenRemainSec);
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
    public void GetGold(int amount)
    {
        onGoldChanged?.Invoke(currentGold);
    }
    public void SpawnGold(Vector2 point, int count, GoodsType type = GoodsType.Gold)
    {
        onGoldSpawned?.Invoke(point, count, type);
    }
    public void AddGold(int amount)
    {
        currentGold += amount;
        onGoldChanged?.Invoke(currentGold);
    }

    #endregion
    #region Gem Management
    public void AddGem(int amount)
    {
        currentGem += amount;
        onGemChanged?.Invoke(currentGem);
    }

    #endregion

    #region Score Management

    public void AddScore(int amount)
    {
        currentGem += amount;
        onGemChanged?.Invoke(currentGem);
    }

    #endregion

    #region Item Management
    public ItemSO GetItemData(string itemId)
    {
        return itemDataDic[itemId];
    }
    public Sprite GetItemSprite(ItemKey key)
    {
        return itemDataDic[key.id].items[key.Lv-1].itemSprite;
    }
    public string GetItemName(ItemKey key)
    {
        return itemDataDic[key.id].items[key.Lv - 1].itemName;
    }
    public int GetItemMaxLevel(ItemKey key)
    {
        return itemDataDic[key.id].items.Length;
    }
    //이 경우에만 잠겨있는 아이템 존재
    public bool SpawnItem(string itemId, int level, Vector2Int targetGridposition, ItemState state = ItemState.Normal)
    {
        GameObject itemObj = itemPool.Get();
        if (!itemObj) return false;

        MergeableItem item = itemObj.GetComponent<MergeableItem>();
        if (!item) return false;
        
        item.itemData = itemDataDic[itemId];
        item.Initialize(level, targetGridposition, state);
        item.draggableItem.Initialize();
        item.itemRectT.sizeDelta = new Vector2(Managers.Grid.TileSize * 0.9f, Managers.Grid.TileSize * 0.9f);
        Managers.Grid.PlaceItem(item, targetGridposition);
            
        

        return true;
    }
    public bool SpawnMoveItem(string itemId, int level, Vector2 startWorldPosition, Vector2Int targetGridposition)
    {
        GameObject itemObj = itemPool.Get();
        if (!itemObj) return false;

        MergeableItem item = itemObj.GetComponent<MergeableItem>();
        if (!item) return false;
        
        item.transform.position = startWorldPosition;
        //item.itemRectT.anchoredPosition = startWorldPosition;
        item.itemData = itemDataDic[itemId];
        item.Initialize(level, targetGridposition);
        item.draggableItem.Initialize();
        item.GetComponent<RectTransform>().sizeDelta = new Vector2(Managers.Grid.TileSize * 0.9f, Managers.Grid.TileSize * 0.9f);
        Managers.Grid.PlaceMoveItem(item, startWorldPosition, targetGridposition);
        
        
        return true;
    }

    public bool SpawnGenerator(string itemId, int level, Vector2Int position)
    {
        GameObject genObj = generatorPool.Get();
        if (!genObj) return false;

        MergeableItem item = genObj.GetComponent<MergeableItem>();
        if (!item) return false;

        item.itemData = itemDataDic[itemId];
        item.Initialize(level, position);
        item.draggableItem.Initialize();
        item.GetComponent<RectTransform>().sizeDelta = new Vector2(Managers.Grid.TileSize * 0.9f, Managers.Grid.TileSize * 0.9f);
        Managers.Grid.PlaceItem(item, position);
        
        Generator tempGenerator = item.gameObject.GetComponent<Generator>();
        tempGenerator.genDB = genDataDic[itemId];
        tempGenerator.Initialize(generatorSyncTime);

        return true;
    }
    public bool SpawnMoveGenerator(string itemId, int level, Vector2 startWorldPosition, Vector2Int targetGridposition)
    {
        GameObject genObj = generatorPool.Get();
        if (!genObj) return false;

        MergeableItem item = genObj.GetComponent<MergeableItem>();
        if (!item) return false;

        item.transform.position = startWorldPosition;
        item.itemData = itemDataDic[itemId];
        item.Initialize(level, targetGridposition);
        item.draggableItem.Initialize();
        item.GetComponent<RectTransform>().sizeDelta = new Vector2(Managers.Grid.TileSize * 0.9f, Managers.Grid.TileSize * 0.9f);
        Managers.Grid.PlaceMoveItem(item, startWorldPosition, targetGridposition);

        Generator tempGenerator = item.gameObject.GetComponent<Generator>();
        tempGenerator.genDB = genDataDic[itemId];
        tempGenerator.Initialize(generatorSyncTime);

        return true;
    }
    public bool CanMerge(MergeableItem draggingItem, MergeableItem targetItem)
    {
        if (draggingItem.CanMergeWith(targetItem))
        {
            return true;
        }
        return false;
    }
    public bool TryMergeItems(MergeableItem draggingItem, MergeableItem targetItem)
    {
        if (draggingItem.CanMergeWith(targetItem))
        {
            
            Managers.Grid.DetatchItemFromGrid(targetItem.GridPosition);
            Managers.Grid.DetatchItemFromGrid(draggingItem.GridPosition);

            targetItem.LevelUp();
            Managers.Grid.AttatchItemToGrid(targetItem, targetItem.GridPosition);
            Managers.Grid.CheckGuestsOrder();

            ReturnItemToPool(draggingItem);
          
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

    

    #region Object Pooling

    private GameObject CreatePooledItem()
    {
        GameObject obj = Instantiate(itemPrefab, Managers.Grid.MergeBoard.transform); // 기본 아이템 프리팹
        obj.SetActive(false);
        return obj;
    }
    private GameObject CreatePooledGenerator()
    {
        GameObject obj = Instantiate(generatorPrefab, Managers.Grid.MergeBoard.transform); // 기본 제너레이터 프리팹
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

    public void ReturnItemToPool(MergeableItem item)
    {
        if (item.itemData.type == ItemType.Generatable)
        {
            ReturnGeneratorToPool(item.gameObject);
        }
        else
        {
            ReturnNormalItemToPool(item.gameObject);
        }
    }
    public void ReturnNormalItemToPool(GameObject itemObj)
    {
        itemPool.Release(itemObj);
    }
    public void ReturnGeneratorToPool(GameObject genObj)
    {
        generatorPool.Release(genObj);
    }
    #endregion

    #region Save/Load System

    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            energy = currentEnergy,
            gold = currentGem,
            rewardList = new List<ItemKey>(currentRewardQueue)
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
                        position = new Vector2Int(x, y),
                        state = item.state,
                        type = item.itemData.type
                    });
                }
            }
        }
        Debug.Log($"현재 보상카드{currentRewardQueue.Count}개 있음");
        Debug.Log($"보상카드{saveData.rewardList.Count}개 저장");
        Managers.Save.SaveGame(saveData);
    }

    public void LoadGame()
    {
        SaveData saveData = Managers.Save.LoadGame();
        if (saveData != null)
        {
            currentEnergy = saveData.energy;
            currentRewardQueue = new Queue<ItemKey>(saveData.rewardList);
            // 저장된 아이템들 복원
            foreach (var itemData in saveData.items)
            {
                switch(itemData.type)
                {
                    case ItemType.Normal:
                        if(SpawnItem(itemData.itemId, itemData.level, itemData.position, itemData.state)) {}
                        break;
                    case ItemType.Generatable:
                        if (SpawnGenerator(itemData.itemId, itemData.level, itemData.position)) {}
                        break;

                }
            }

            onEnergyChanged?.Invoke(Mathf.RoundToInt(currentEnergy));
            onGemChanged?.Invoke(currentGem);
            Debug.Log($"보상카드{currentRewardQueue.Count}개 불러옴");
            onRewardQueueChanged?.Invoke(currentRewardQueue);
        }
        KeyValuePair<ItemKey, int>temp1 = new KeyValuePair<ItemKey, int>(new ItemKey("N001", 2), 1);
        KeyValuePair<ItemKey, int>temp2 = new KeyValuePair<ItemKey, int>(new ItemKey("N001", 3), 1);
        KeyValuePair<ItemKey, int>temp3 = new KeyValuePair<ItemKey, int>(new ItemKey("N001", 4), 1);

        KeyValuePair<ItemKey, int>temp4 = new KeyValuePair<ItemKey, int>(new ItemKey("N002", 2), 1);
        KeyValuePair<ItemKey, int>temp5 = new KeyValuePair<ItemKey, int>(new ItemKey("N002", 3), 1);
        KeyValuePair<ItemKey, int>temp6 = new KeyValuePair<ItemKey, int>(new ItemKey("N002", 4), 1);

        KeyValuePair<ItemKey, int>temp7 = new KeyValuePair<ItemKey, int>(new ItemKey("N001", 2), 2);
        KeyValuePair<ItemKey, int>temp8 = new KeyValuePair<ItemKey, int>(new ItemKey("N002", 4), 2);

        KeyValuePair<ItemKey, int>temp9 = new KeyValuePair<ItemKey, int>(new ItemKey("N001", 4), 2);
        KeyValuePair<ItemKey, int>temp10 = new KeyValuePair<ItemKey, int>(new ItemKey("N002", 3), 2);

        onGuestCreated?.Invoke(1, temp1, null);
        onGuestCreated?.Invoke(1, temp4, null);
        onGuestCreated?.Invoke(4, temp7, temp5);
        onGuestCreated?.Invoke(4, temp10, null);
        onGuestCreated?.Invoke(12, temp6, temp9);
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
        currentGem = 0;

        // 이벤트 발송
        onEnergyChanged?.Invoke(Mathf.RoundToInt(currentEnergy));
        onGemChanged?.Invoke(currentGem);

        ResumeGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !Managers.Save.isClear)
        {
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        if(!Managers.Save.isClear) SaveGame();
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