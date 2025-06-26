
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
public class MergeableItem : MonoBehaviour
{
    [SerializeField] private Sprite[] boxSprite = new Sprite[2];
    public GameObject LockImageObj;
    [SerializeField] public GameObject adIcon;
    public ItemState state { get; private set; }
    [SerializeField] public CanvasGroup selectIcon;
    [SerializeField] public CanvasGroup selectBackground;
    public bool isSelect = true;

    [SerializeField] public CanvasGroup checkIcon;
    [SerializeField] public CanvasGroup checkBackground;
    public bool isCheck = false;
    [Header("Item Settings")]
    [SerializeField] protected int lv = 1;
    public int lvIndex => Mathf.Clamp(lv - 1, 0, itemData.items.Length - 1);
    //[SerializeField] protected string itemId;
    public Image itemImage { get; private set; }
    public Image bubbleImage;
    public ItemSO itemData;
    public ItemKey itemKey;
    public int price => itemData.items[lvIndex].price;
    public DraggableItem draggableItem;

    public Vector2Int gridPosition { get;  private set; }
    protected bool isInitialized = false;

    public ItemEffect itemEffect;
    public RectTransform rectT;

    public int Lv => lv;
    public float gemBubbleRemainSec = 0f;
    private void Awake()
    {
        if (itemImage.IsUnityNull())
        {
            itemImage = transform.GetChild(2).GetComponent<Image>();

        }
        if (itemEffect.IsUnityNull())
        {
            itemEffect = transform.GetComponentInChildren<ItemEffect>();
        }
        if (rectT.IsUnityNull())
        {
            rectT = GetComponent<Image>().rectTransform;
        }
        if(draggableItem.IsUnityNull())
        {
            draggableItem = GetComponent<DraggableItem>();
        }
    }

    public void Initialize(int inputLv, Vector2Int pos, ItemState inputState = ItemState.Normal)
    {
        draggableItem = GetComponent<DraggableItem>();
        lv = Mathf.Clamp(inputLv, 1, itemData.items.Length);
        SetGridPosition(pos);
        state = inputState;

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
        if (!itemEffect.IsUnityNull())
        {
            itemEffect.successParticleImage.enabled = false;
        }
        
        isCheck = false;

        if(checkIcon)
        {
            checkIcon.gameObject.SetActive(false);
        }
        if (checkBackground)
        { 
            checkBackground.gameObject.SetActive(false); 
        }

        isSelect = false;
        if (selectIcon)
        {
            selectIcon.gameObject.SetActive(false);
        }
        if(selectBackground)
        {
            selectBackground.gameObject.SetActive(false);
        }



        isInitialized = true;
    }

    public void StartGemBubbleCounting()
    {
        gemBubbleRemainSec = itemData.items[lvIndex].bubbleTime;
        InvokeRepeating(nameof(DisappearGemBubble), 1f, 1f);
        if (draggableItem.isSelected)
        {
            Managers.Game.UpdateGemBubbleRemainSeconds((int)gemBubbleRemainSec);
        }
        //onEnergyRegenTimeChanged.Invoke(Mathf.RoundToInt(gemCountRemainSec));

    }
    private void DisappearGemBubble()
    {
        //Debug.Log(energyRegenRemainSec);
        gemBubbleRemainSec -= 1f;
        if (draggableItem.isSelected)
        {
            Managers.Game.UpdateGemBubbleRemainSeconds((int)gemBubbleRemainSec);
        }
        //onEnergyRegenTimeChanged.Invoke(Mathf.RoundToInt(gemCountRemainSec));
        if (gemBubbleRemainSec <= 0f)
        {
            Managers.Grid.RemoveItemFromGridInstantly(gridPosition);
            CancelInvoke(nameof(DisappearGemBubble));
            if (draggableItem.isSelected)
            {
                OnDeSelected();
            }

        }
    }
    public void SellThisItem()
    {
       Managers.Game.AddStar(itemData.items[lvIndex].price);
       Managers.Grid.RemoveItemFromGridInstantly(gridPosition);
       Managers.Grid.CheckGuestsOrder();
    }

    #region Bubble State
    public void GiveUpBubbleItem()
    {
        Managers.Grid.RemoveItemFromGridInstantly(gridPosition);
    }
    public void PopBubbleItemByAd()
    {
        state = ItemState.Normal;
        adIcon.SetActive(false);
        bubbleImage.gameObject.SetActive(false);
        itemImage.rectTransform.localScale = Vector3.one;
        OnDeSelected();
        //draggableItem.SelectItem();
    }
    public void SkipBubbleItem()
    {
        //이 아이템 자리에 골드 떨어지는거 구현 
        Managers.Grid.RemoveItemFromGridInstantly(gridPosition);
    }
    public void PopBubbleItemByGem()
    {
        if (!Managers.Game.TrySpendGem(itemData.items[lvIndex].bubbleCost)) return;
        state = ItemState.Normal;
        bubbleImage.gameObject.SetActive(false);
        itemImage.rectTransform.localScale = Vector3.one;
        OnDeSelected();
        //draggableItem.SelectItem();
    }

    #endregion

    protected void UpdateVisuals()
    {
        if (itemImage != null && itemData.items.Length > 0)
        {
            if (itemData.type == ItemType.Normal || itemData.type == ItemType.Usable || itemData.type == ItemType.Generatable)
            {
                itemImage.color = new Color(1.0f, 1.0f, 1.0f, 1f);
                if (adIcon != null)
                {
                    adIcon.SetActive(false);
                }
                if (bubbleImage)
                {
                    bubbleImage.gameObject.SetActive(false);
                }
                draggableItem.enabled = true;

                switch (state)
                {
                    case ItemState.Normal:
                        itemImage.rectTransform.localScale = 1.0f * Vector3.one;
                        itemImage.sprite = itemData.items[lvIndex].itemSprite;
                        break;
                    case ItemState.Locked:
                        itemImage.sprite = itemData.items[lvIndex].itemSprite;
                        itemImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                        LockImageObj.SetActive(true);
                        //draggableItem.enabled = false;
                        break;
                    case ItemState.InBox:
                        itemImage.sprite = (gridPosition.x + gridPosition.y) % 2 == 0 ? boxSprite[0] : boxSprite[1];
                        itemImage.color = new Color(1.0f, 1.0f, 1.0f, 1f);
                        draggableItem.enabled = false;
                        break;
                    //방울에 광고모양 추가하기
                    case ItemState.BubbleAd:
                        //광고이미지 on 구현
                        adIcon.SetActive(true);
                        bubbleImage.gameObject.SetActive(true);
                        itemImage.rectTransform.localScale = 0.8f * Vector3.one;
                        itemImage.sprite = itemData.items[lvIndex].itemSprite;
                        break;
                    case ItemState.BubbleGem:
                        StartGemBubbleCounting();
                        bubbleImage.gameObject.SetActive(true);
                        itemImage.rectTransform.localScale = 0.8f * Vector3.one;
                        itemImage.sprite = itemData.items[lvIndex].itemSprite;
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
               (other.state == ItemState.Normal || other.state == ItemState.Locked) &&
               other.itemData.id == itemData.id &&
               other.Lv == lv &&
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
            string soundKey = lv.ToString();
            Managers.Asset.PlaySound(soundKey, SoundType.Effect);
            UpdateVisuals();
            if (state == ItemState.Locked)
            {
                state = ItemState.Normal;
                LockImageObj.SetActive(false);
                itemImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                draggableItem.enabled = true;
                Managers.Grid.OpenNearBox(gridPosition);
            }            
            

            if (itemData.type == ItemType.Generatable)
            {
                GetComponent<Generator>().Initialize(Managers.Grid.GeneratorSyncTime);
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
        
        if(isCheck && !isSelect)
        {
            checkBackground.DOFade(0f, 0.1f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                checkBackground.gameObject.SetActive(false);
            });
        }
        isSelect = true;
        selectIcon.gameObject.SetActive(true);
        selectBackground.gameObject.SetActive(true);

        // 아이콘의 스케일 애니메이션
        selectIcon.alpha = 1.0f;
        selectIcon.transform.localScale = 0.5f * Vector3.one;
        selectIcon.transform.DOScale(1.0f, 0.2f).SetEase(Ease.OutBack);

        // 배경의 알파 애니메이션
        selectBackground.alpha = 0f;
        selectBackground.DOFade(1f, 0.2f).SetEase(Ease.InOutQuad);

    }
    public void OnDeSelected()
    {
        
        if (isCheck && isSelect)
        {
            checkBackground.gameObject.SetActive(true);
            checkBackground.alpha = 0f;
            checkBackground.DOFade(1f, 0.2f).SetEase(Ease.InOutQuad);
        }
        isSelect = false;
        selectIcon.DOFade(0f, 0.1f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            selectIcon.gameObject.SetActive(false);
        });
        selectBackground.DOFade(0f, 0.1f).SetEase(Ease.InOutQuad).OnComplete(()=>
        {
            selectBackground.gameObject.SetActive(false);
        });
    }

    public void OnChecked()
    {
        if (itemData.type != ItemType.Normal) return;


        checkIcon.gameObject.SetActive(true);
        checkBackground.gameObject.SetActive(true);

        //// 아이콘의 스케일 애니메이션
        //checkIcon.alpha = 1.0f;
        //checkIcon.transform.localScale = 0.1f * Vector3.one;
        //checkIcon.transform.DOScale(0.3f, 0.2f).SetEase(Ease.OutBack);

        //// 배경의 알파 애니메이션
        //checkBackground.alpha = 0f;
        //checkBackground.DOFade(1f, 0.2f).SetEase(Ease.InOutQuad);

    }
    public void OnUnchecked()
    {
        if (itemData.type != ItemType.Normal) return;
        checkIcon.gameObject.SetActive(false);
        checkBackground.gameObject.SetActive(false);
        

        //checkIcon.DOFade(0f, 0.1f).SetEase(Ease.InOutQuad).OnComplete(() =>
        //{
        //    checkIcon.gameObject.SetActive(false);
        //});
        //checkBackground.DOFade(0f, 0.1f).SetEase(Ease.InOutQuad).OnComplete(() =>
        //{
        //    checkBackground.gameObject.SetActive(false);
        //});
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

