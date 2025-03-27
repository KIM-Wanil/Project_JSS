
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
public class MergeableItem : MonoBehaviour
{
    [SerializeField] private Sprite boxSprite;
    public GameObject LockImageObj;
    public ItemState state { get; private set; }
    [SerializeField] public GameObject selectIcon;
    [Header("Item Settings")]
    [SerializeField] protected int lv = 1;
    private int lvIndex => Mathf.Clamp(lv - 1, 0, itemData.items.Length - 1);
    //[SerializeField] protected string itemId;
    [SerializeField] protected Image itemImage;
    public ItemSO itemData;
    public ItemKey itemKey;
    public int price => itemData.items[lvIndex].price;
    public DraggableItem draggableItem;
    [Header("Events")]
    public UnityEvent<int> onLevelChanged;
    public UnityEvent onMerged;
    public UnityEvent onSpawned;

    protected Vector2Int gridPosition;
    protected bool isInitialized = false;

    public ItemEffect itemEffect;
    public RectTransform itemRectT;

    public int Lv => lv;
    public Vector2Int GridPosition => gridPosition;
    public Image ItemImage => itemImage;
    
    private void Awake()
    {
        if (itemImage.IsUnityNull())
        {
            itemImage = GetComponent<Image>();
        }
        if (itemEffect.IsUnityNull())
        {
            itemEffect = transform.GetComponentInChildren<ItemEffect>();
        }
        if (itemRectT.IsUnityNull())
        {
            itemRectT = GetComponent<RectTransform>();
        }
        if(draggableItem.IsUnityNull())
        {
            draggableItem = GetComponent<DraggableItem>();
        }
    }

    public void Initialize(int inputLv, ItemState inputState = ItemState.None)
    {
        state = inputState;
        Debug.Log(state);
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
    }

    //public void Initialize(SaveData.ItemData saveData)
    //{
    //    //itemId = saveData.itemId;


    //    lv = saveData.level;
    //    gridPosition = saveData.position;
    //    UpdateVisuals();
    //    isInitialized = true;


    //}
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
            if (itemData.type == ItemType.Normal)
            {
                switch (state)
                {
                    case ItemState.None:
                        itemImage.sprite = itemData.items[lvIndex].itemSprite;
                        break;
                    case ItemState.Locked:
                        itemImage.sprite = itemData.items[lvIndex].itemSprite;
                        ItemImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                        LockImageObj.SetActive(true);
                        //draggableItem.enabled = false;
                        break;
                    case ItemState.InBox:
                        itemImage.sprite = boxSprite;
                        ItemImage.color = new Color(1.0f, 1.0f, 1.0f, 1f);
                        draggableItem.enabled = false;
                        break;
                }
            }
            else
            {
               itemImage.sprite = itemData.items[lvIndex].itemSprite;
            }
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


    public void SetGridPosition(Vector2Int pos)
    {
        gridPosition = pos;
    }

    public void LevelUp()
    {
        if (lv < itemData.items.Length)
        {          
            itemEffect.PlaySuccessMergeEffect();
            lv++;
            itemKey.lv = lv;
            UpdateVisuals();
            if (state == ItemState.Locked)
            {
                state = ItemState.None;
                LockImageObj.SetActive(false);
                ItemImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                draggableItem.enabled = true;
                Managers.Grid.OpenNearBox(gridPosition);
            }            
            onLevelChanged?.Invoke(lv);
            Managers.Grid.CheckGuestsOrder();

            if (itemData.type == ItemType.Generatable)
            {
                GetComponent<Generator>().Initialize();
            }

            // DoTween을 사용하여 아이템 이미지의 크기 변화를 애니메이션으로 추가
            itemImage.rectTransform.localScale = 0.5f * Vector3.one; // 처음 크기를 50%로 설정
            DG.Tweening.Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(10f / 60f) // 처음 10프레임 동안 대기
                    .Append(itemImage.rectTransform.DOScale(1.25f, 10f / 60f).SetEase(Ease.InOutQuad)) // 11f ~ 21f: 50% -> 125%
                    .Append(itemImage.rectTransform.DOScale(0.75f, 10f / 60f).SetEase(Ease.InOutQuad)) // 21f ~ 31f: 125% -> 75%
                    .Append(itemImage.rectTransform.DOScale(1.0f, 10f / 60f).SetEase(Ease.InOutQuad)) // 31f ~ 41f: 75% -> 100%
                    .Play(); // 시퀀스를 실행
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

