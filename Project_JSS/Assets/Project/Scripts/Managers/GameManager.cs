using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using static SaveData;
using System.Linq;

public class GameManager : BaseManager
{
    //public static GameManager Instance { get; private set; }

    public const int GRID_WIDTH = 8;
    public const int GRID_HEIGHT = 8;

    [Header("Energy Settings")]
    [SerializeField] private int maxEnergy = 100;
    public int MaxEnergy => maxEnergy;
    [SerializeField] private float energyRegenRate = 1f;
    [SerializeField] private int energyRegenAmount = 1;
    private float currentEnergy;
    public float CurrentEnergy => currentEnergy;
    [Header("Gold Settings")]
    [SerializeField] private int currentGold = 0;
    public int CurrentGold => currentGold;

    [Header("Prefab References")]
    [SerializeField] private GameObject itemPrefab;
    //[SerializeField] private GameObject generatorPrefab;

    [Header("SO References")]
    [SerializeField] private ItemData[] itemDatas;
    [SerializeField] private GeneratorData[] genDatas;

    [Header("Guest Referemces")]
    private GameObject guestBoard;
    [SerializeField] private GameObject guestPrefab;
    [SerializeField] private Sprite[] guestSprites;

    [Header("Game Events")]
    public UnityEvent<int> onEnergyChanged;
    public UnityEvent<int> onGoldChanged;
    public UnityEvent<int> onScoreChanged;
    public UnityEvent<MergeableItem> onItemMerged;
    public UnityEvent<MergeableItem> onItemSpawned;

    
    private int currentScore;
    private Dictionary<string, ItemData> itemDataDic = new Dictionary<string, ItemData>();
    private Dictionary<string, GeneratorData> genDataDic = new Dictionary<string, GeneratorData>();
    [SerializeField] private ObjectPool<GameObject> itemPool;
    private bool isGamePaused;

    private void Awake()
    {
    }
    public override void Init()
    {
        base.Init();
        InitializeGame();
    }
    private void InitializeGame()
    {
        // 프리팹 딕셔너리 초기화
        foreach (var itemData in itemDatas)
        {
            itemDataDic[itemData.id] = itemData;
        }

        foreach (var genData in genDatas)
        {
            genDataDic[genData.genId] = genData;
        }

        // 오브젝트 풀 초기화
        itemPool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnToPool, OnDestroyPoolObject, true, 50, 100);

        guestBoard = GameObject.Find("GuestBoard");

        //LoadGame();
    }

    private void Start()
    {
        currentEnergy = maxEnergy;
        InvokeRepeating(nameof(RegenerateEnergy), energyRegenRate, energyRegenRate);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateGuest();
        }
    }
    #region Energy Management

    private void RegenerateEnergy()
    {
        if (currentEnergy < maxEnergy)
        {
            AddEnergy(energyRegenAmount);
        }
    }

    public void AddEnergy(int amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        onEnergyChanged?.Invoke(Mathf.RoundToInt(currentEnergy));
    }

    public bool TrySpendEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            onEnergyChanged?.Invoke(Mathf.RoundToInt(currentEnergy));
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
    public ItemData GetItemData(string itemId)
    {
        return itemDataDic[itemId];
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
            Managers.Grid.PlaceItem(item, position);
            //onItemSpawned?.Invoke(item);
        }

        Managers.Grid.CheckGuestsOrder();
        return item;
    }
    public Generator SpawnGenerator(string itemId, int level, Vector2Int position)
    {
        MergeableItem item = SpawnItem(itemId, level, position);
        Generator generator = item.gameObject.AddComponent<Generator>();
        generator.genData = genDataDic[itemId];
        return generator;
    }

    public bool TryMergeItems(MergeableItem item1, MergeableItem neighbor)
    {
        if (!item1.CanMergeWith(neighbor))
        {
            return false;
        }

        //Vector2Int mergePosition = Managers.Grid.GetGridPosition(item1.transform.position);
        Vector2Int mergePosition = neighbor.GridPosition;
        // 다음 레벨 아이템 생성
        MergeableItem mergedItem = SpawnItem(item1.itemData.id, item1.Lv + 1, mergePosition);
        if (mergedItem != null)
        {
            // 기존 아이템 제거
            //Managers.Grid.RemoveItem(Managers.Grid.GetGridPosition(item1.transform.position));
            //Managers.Grid.RemoveItem(Managers.Grid.GetGridPosition(item2.transform.position));
            AddScore(CalculateMergeScore(mergedItem.Lv));

            Managers.Grid.RemoveItem(item1.GridPosition);
            //Managers.Grid.RemoveItem(neighbor.GridPosition);

            ReturnItemToPool(item1.gameObject);
            ReturnItemToPool(neighbor.gameObject);

            //Managers.Grid.PlaceItem(mergedItem, mergePosition);
            // 점수 추가

            //onItemMerged?.Invoke(mergedItem);

            return true;
        }

        return false;
    }

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
        foreach (var generator in Managers.Grid.FindAllGenerators())
        {
            GeneratorData generatorData = generator.genData;

            // 제너레이터의 레벨에 따른 생성 가능한 아이템 ID를 추가
            foreach (var levelData in generatorData.generatorLevelDatas)
            {
                foreach (var item in levelData.generatableItems)
                {
                    availableItemIds.Add(item.itemInfo.id);
                }
            }
        }

        return availableItemIds.ToList();
    }

    #endregion

    #region Guest Management
    public void CreateGuest()
    {
        Guest guest = Instantiate(guestPrefab, guestBoard.transform).GetComponent<Guest>();

        int count = Random.Range(1, 3);
        Item[] tempItems = new Item[count];
        List<string> availableItems = GetAvailableItemIds();
        for (int i = 0; i < count; i++)
        {
            tempItems[i].id = availableItems[Random.Range(0, availableItems.Count)];
            tempItems[i].lv = Random.Range(3, 5);
        }
        int goldAmount = Random.Range(30, 61);
        guest.Init(guestSprites[Random.Range(0, guestSprites.Length)], tempItems, goldAmount*10);
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