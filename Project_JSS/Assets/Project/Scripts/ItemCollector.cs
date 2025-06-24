using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
public class ItemCollector : MonoBehaviour
{
    public UnityEngine.Transform parentTransform;
    [SerializeField]
    private GameObject starEffectPrefab;
    [SerializeField]
    private GameObject energyEffectPrefab;
    [SerializeField]
    private GameObject gemEffectPrefab;
    [SerializeField]
    private GameObject goldEffectPrefab;
    [SerializeField]
    private RectTransform starUiElement;            // 아이템이 이동할 목표 위치 (Item Icon UI)
    [SerializeField]
    private RectTransform energyUiElement;
    [SerializeField]
    private RectTransform gemUiElement;
    [SerializeField]
    private RectTransform goldUiElement;


    private MemoryPool starMemoryPool;         // 아이템을 생성하고 관리할 메모리 풀
    private MemoryPool energyMemoryPool;         // 아이템을 생성하고 관리할 메모리 풀
    private MemoryPool gemMemoryPool;         // 아이템을 생성하고 관리할 메모리 풀
    private MemoryPool goldMemoryPool;         // 아이템을 생성하고 관리할 메모리 풀

    private void Awake()
    {
        starMemoryPool = new MemoryPool(starEffectPrefab, parentTransform);
        energyMemoryPool = new MemoryPool(energyEffectPrefab, parentTransform);
        gemMemoryPool = new MemoryPool(gemEffectPrefab, parentTransform);
        goldMemoryPool = new MemoryPool(goldEffectPrefab, parentTransform);

        Managers.Game.onStarSpawned.AddListener(SpawnItemEffect);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 예시: "StarUI"라는 이름의 오브젝트를 찾아서 할당
        GameObject starUiObj = GameObject.Find("별아이콘");
        if (starUiObj != null)
            starUiElement = starUiObj.GetComponent<RectTransform>();
        else
            Debug.LogWarning("StarUI 오브젝트를 찾을 수 없습니다.");
    }

    public void SpawnItemEffect(Vector2 point, int count, GoodsType goods)
    {
        Debug.Log("SpawnItemEffect");
        MemoryPool memoryPool = null;
        RectTransform uiElement = null;
        switch(goods)
        {
            case GoodsType.Star:
                memoryPool = starMemoryPool;
                uiElement = starUiElement;
                break;
            case GoodsType.Energy:
                memoryPool = energyMemoryPool;
                uiElement = energyUiElement;
                break;
            case GoodsType.Gem:
                memoryPool = gemMemoryPool;
                uiElement = gemUiElement;
                break;
            default:
                Debug.LogError("Invalid GoodsType");
                return;
        }
        for (int i = 0; i < count; ++i)
        {
            GameObject item = memoryPool.ActivatePoolItem(point, parentTransform);
            item.GetComponent<ItemCollectEffect>().Setup(this, uiElement);
        }
    }

    public void OnItemCollect(GameObject item, GoodsType goodsType)
    {
        switch(goodsType)
        {
            case GoodsType.Star:
                Managers.Game.AddStar(1);
                starMemoryPool.DeactivatePoolItem(item);
                break;
            case GoodsType.Energy:
                Managers.Game.AddEnergy(1);
                energyMemoryPool.DeactivatePoolItem(item);
                break;
            case GoodsType.Gem:
                Managers.Game.AddGem(1);
                gemMemoryPool.DeactivatePoolItem(item);
                break;
            case GoodsType.Gold:
                Managers.Game.AddGold(1);
                goldMemoryPool.DeactivatePoolItem(item);
                break;
            default:
                Debug.LogError("Invalid GoodsType");
                break;
        }
    }
}

