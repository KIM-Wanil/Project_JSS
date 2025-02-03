using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class GameManager : BaseManager
{
    public static GameManager Instance { get; private set; }

    public const int GRID_WIDTH = 7;
    public const int GRID_HEIGHT = 9;

    [Header("Energy Settings")]
    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private float energyRegenRate = 1f;
    [SerializeField] private int energyRegenAmount = 1;

    [Header("Prefab References")]
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private GameObject[] generatorPrefabs;

    [Header("Game Events")]
    public UnityEvent<int> onEnergyChanged;
    public UnityEvent<int> onScoreChanged;
    public UnityEvent<MergeableItem> onItemMerged;
    public UnityEvent<MergeableItem> onItemSpawned;

    private float currentEnergy;
    private int currentScore;
    private Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();
    private ObjectPool<GameObject> itemPool;
    private bool isGamePaused;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGame()
    {
        // 프리팹 딕셔너리 초기화
        foreach (var prefab in itemPrefabs)
        {
            MergeableItem item = prefab.GetComponent<MergeableItem>();
            if (item != null)
            {
                prefabDictionary[item.ItemId] = prefab;
            }
        }

        foreach (var prefab in generatorPrefabs)
        {
            Generator generator = prefab.GetComponent<Generator>();
            if (generator != null)
            {
                prefabDictionary[generator.ItemId] = prefab;
            }
        }

        // 오브젝트 풀 초기화
        itemPool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnToPool, OnDestroyPoolObject, true, 50, 100);



        LoadGame();
    }

    private void Start()
    {
        currentEnergy = maxEnergy;
        InvokeRepeating(nameof(RegenerateEnergy), energyRegenRate, energyRegenRate);
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

    #region Score Management

    public void AddScore(int amount)
    {
        currentScore += amount;
        onScoreChanged?.Invoke(currentScore);
    }

    #endregion

    #region Item Management

    public MergeableItem SpawnItem(string prefabId, int level, Vector2Int position)
    {
        if (!prefabDictionary.ContainsKey(prefabId))
        {
            Debug.LogError($"Prefab with ID {prefabId} not found!");
            return null;
        }

        GameObject itemObj = itemPool.Get();
        itemObj.transform.position = Managers.Grid.GetWorldPosition(position);

        MergeableItem item = itemObj.GetComponent<MergeableItem>();
        if (item != null)
        {
            item.Initialize(level);
            Managers.Grid.PlaceItem(item, position);
            onItemSpawned?.Invoke(item);
        }

        return item;
    }

    public bool TryMergeItems(MergeableItem item1, MergeableItem item2)
    {
        if (!item1.CanMergeWith(item2))
            return false;

        Vector2Int mergePosition = Managers.Grid.GetGridPosition(item1.transform.position);

        // 다음 레벨 아이템 생성
        MergeableItem mergedItem = SpawnItem(item1.ItemId, item1.Level + 1, mergePosition);

        if (mergedItem != null)
        {
            // 기존 아이템 제거
            Managers.Grid.RemoveItem(Managers.Grid.GetGridPosition(item1.transform.position));
            Managers.Grid.RemoveItem(Managers.Grid.GetGridPosition(item2.transform.position));

            ReturnItemToPool(item1.gameObject);
            ReturnItemToPool(item2.gameObject);

            // 점수 추가
            AddScore(CalculateMergeScore(item1.Level));

            onItemMerged?.Invoke(mergedItem);
            return true;
        }

        return false;
    }

    private int CalculateMergeScore(int level)
    {
        // 레벨이 높아질수록 더 많은 점수를 얻도록 설정
        return Mathf.RoundToInt(Mathf.Pow(2, level) * 10);
    }

    #endregion

    #region Object Pooling

    private GameObject CreatePooledItem()
    {
        GameObject obj = Instantiate(itemPrefabs[0]); // 기본 아이템 프리팹
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
                        itemId = item.ItemId,
                        level = item.Level,
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