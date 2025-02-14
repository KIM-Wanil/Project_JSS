using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MergeableItem : MonoBehaviour
{
    [SerializeField] public Button button;
    [SerializeField] public GameObject selectIcon;
    [Header("Item Settings")]
    [SerializeField] protected int lv = 1;
    private int lvIndex => Mathf.Clamp(lv - 1, 0, itemData.items.Length - 1);
    //[SerializeField] protected string itemId;
    [SerializeField] protected Image itemImage;
    public ItemData itemData;
    public ItemKey itemKey;
    public int price => itemData.items[lvIndex].price;
    [Header("Effects")]
    [SerializeField] protected ParticleSystem mergeEffect;
    [SerializeField] protected ParticleSystem spawnEffect;

    [Header("Events")]
    public UnityEvent<int> onLevelChanged;
    public UnityEvent onMerged;
    public UnityEvent onSpawned;

    protected Vector2Int gridPosition;
    protected bool isInitialized = false;

    public int Lv => lv;
    public Vector2Int GridPosition => gridPosition;
    public Image ItemImage => itemImage;
    
    private void Awake()
    {
        if (itemImage == null)
        {
            itemImage = GetComponent<Image>();
        }
    }

    public void Initialize(int inputLv)
    {
        lv = Mathf.Clamp(inputLv, 1, itemData.items.Length);
        UpdateVisuals();
        itemKey = new ItemKey(itemData.id, lv);
        //switch (itemData.type)
        //{
        //    case ItemType.Normal:
        //        button.onClick.AddListener(() =>
        //        Managers.Game.infoPanelController.PrintItemDesc(itemKey, itemData.items[lvIndex].price, SellThisItem)
        //    );
        //        break;
        //    default:
        //        break;
        //}

        isInitialized = true;

        if (spawnEffect != null)
        {
            spawnEffect.Play();
        }

        onSpawned?.Invoke();
    }

    public void Initialize(SaveData.ItemData saveData)
    {
        //itemId = saveData.itemId;
        lv = saveData.level;
        gridPosition = saveData.position;
        UpdateVisuals();
        isInitialized = true;
    }
    public void SellThisItem()
    {
       Managers.Game.AddGold(itemData.items[lvIndex].price);
       Managers.Grid.RemoveItemFromGrid(gridPosition);
        Debug.Log(gridPosition);
    }
    protected void UpdateVisuals()
    {
        if (itemImage != null  && itemData.items.Length > 0)
        {
            itemImage.sprite = itemData.items[lvIndex].itemSprite;
        }
    }

    public bool CanMergeWith(MergeableItem other)
    {
        return other != null &&
               other != this &&
               other.itemData.id == itemData.id &&
               other.lv == lv &&
               lv < itemData.items.Length; // 최대 레벨 체크
    }
    public ItemKey? GetCraftingResult(MergeableItem other)
    {
        if(other == null || other == this)
        {
            return null;
        }

        return Managers.Game.craftingDB.FindCraftingResult(itemKey, other.itemKey);

    }

    public void OnMerged()
    {
        if (mergeEffect != null)
        {
            // 이펙트를 부모에서 분리하고 자동 제거되도록 설정
            mergeEffect.transform.SetParent(null);
            mergeEffect.Play();
            Destroy(mergeEffect.gameObject, mergeEffect.main.duration);
        }

        onMerged?.Invoke();
        Managers.Grid.DetatchItemFromGrid(gridPosition);
    }

    public void SetGridPosition(Vector2Int pos)
    {
        gridPosition = pos;
    }

    public void LevelUp()
    {
        if (lv < itemData.items.Length)
        {
            lv++;
            itemKey.lv = lv;
            UpdateVisuals();
            onLevelChanged?.Invoke(lv);
            Managers.Grid.CheckGuestsOrder();
        }
    }
    public void OnSelected()
    {
        selectIcon.SetActive(true);
    }
    public void OnDeSelected()
    {
        selectIcon.SetActive(false);
    }

    // 아이템 타입별 특수 효과를 위한 가상 메서드들
    protected virtual void OnItemPlaced() { }
    protected virtual void OnItemRemoved() { }
    protected virtual void OnItemMoved() { }

    //// 애니메이션 관련 메서드
    //public virtual void PlayMergeAnimation(Vector3 targetPosition, System.Action onComplete = null)
    //{
    //    // 기본 구현: 단순 이동
    //    StartCoroutine(MergeAnimationCoroutine(targetPosition, onComplete));
    //}

    //private System.Collections.IEnumerator MergeAnimationCoroutine(Vector3 targetPosition, System.Action onComplete)
    //{
    //    float duration = 0.2f;
    //    float elapsed = 0f;
    //    Vector3 startPosition = transform.position;

    //    while (elapsed < duration)
    //    {
    //        elapsed += Time.deltaTime;
    //        float t = elapsed / duration;

    //        // 이징 함수 적용
    //        t = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease-out

    //        transform.position = Vector3.Lerp(startPosition, targetPosition, t);
    //        yield return null;
    //    }

    //    transform.position = targetPosition;
    //    onComplete?.Invoke();
    //}


    // 저장/로드를 위한 데이터 직렬화
    public SaveData.ItemData GetSaveData()
    {
        return new SaveData.ItemData
        {
            //itemId = itemId,
            level = lv,
            position = gridPosition
        };
    }
}

