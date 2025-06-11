using UnityEngine;
using UnityEngine.Events;

public class ItemCollector : MonoBehaviour
{
    public UnityEngine.Transform canvasTransform;
    [SerializeField]
    private GameObject goldEffectPrefab;
    [SerializeField]
    private GameObject energyEffectPrefab;
    [SerializeField]
    private GameObject gemEffectPrefab;
    [SerializeField]
    private RectTransform goldUiElement;            // 아이템이 이동할 목표 위치 (Item Icon UI)
    [SerializeField]
    private RectTransform energyUiElement;
    [SerializeField]
    private RectTransform gemUiElement;


    private MemoryPool goldMemoryPool;         // 아이템을 생성하고 관리할 메모리 풀
    private MemoryPool energyMemoryPool;         // 아이템을 생성하고 관리할 메모리 풀
    private MemoryPool gemMemoryPool;         // 아이템을 생성하고 관리할 메모리 풀

    private void Awake()
    {
        goldMemoryPool = new MemoryPool(goldEffectPrefab, canvasTransform);
        energyMemoryPool = new MemoryPool(energyEffectPrefab, canvasTransform);
        gemMemoryPool = new MemoryPool(gemEffectPrefab, canvasTransform);

        Managers.Game.onGoldSpawned.AddListener(SpawnItemEffect);
    }

    public void SpawnItemEffect(Vector2 point, int count, GoodsType goods)
    {
        Debug.Log("SpawnItemEffect");
        MemoryPool memoryPool = null;
        RectTransform uiElement = null;
        switch(goods)
        {
            case GoodsType.Gold:
                memoryPool = goldMemoryPool;
                uiElement = goldUiElement;
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
            GameObject item = memoryPool.ActivatePoolItem(point, canvasTransform);
            item.GetComponent<ItemCollectEffect>().Setup(this, uiElement);
        }
    }

    public void OnItemCollect(GameObject item, GoodsType goodsType)
    {
        switch(goodsType)
        {
            case GoodsType.Gold:
                Managers.Game.AddGold(1);
                goldMemoryPool.DeactivatePoolItem(item);
                break;
            case GoodsType.Energy:
                Managers.Game.AddEnergy(1);
                energyMemoryPool.DeactivatePoolItem(item);
                break;
            case GoodsType.Gem:
                Managers.Game.AddGem(1);
                gemMemoryPool.DeactivatePoolItem(item);
                break;
            default:
                Debug.LogError("Invalid GoodsType");
                break;
        }
    }
}

