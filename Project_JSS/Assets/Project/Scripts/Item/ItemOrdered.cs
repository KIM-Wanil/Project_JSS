using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ItemOrdered : MonoBehaviour//, IPointerClickHandler
{
    [SerializeField] private Image image;
    [SerializeField] private GameObject checkIcon;
    [SerializeField] private Button infoButton;
    [SerializeField] private TextMeshProUGUI countText;
    public RectTransform rectT;
    public ItemSO data;
    public ItemDetails details;
    public ItemKey key;
    public bool IsFulfill { get; private set; } = false;
    public int lvIndex;
    public int goalCount = 0;
    public int currentCount = 0;

    public bool canClick = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init(ItemKey inputKey, int inputGoalCount)
    {
        rectT = GetComponent<RectTransform>();
        key = inputKey;
        goalCount = inputGoalCount;
        data = Managers.Game.GetItemData(inputKey.id);

        if (image != null && data.items.Length > 0)
        {
            lvIndex = Mathf.Clamp(key.lv - 1, 0, data.items.Length - 1);
            details = data.items[lvIndex];
            image.sprite = details.itemSprite;
        }
        UpdateCountText();
    }
    public void UpdateCountText()
    {
        currentCount = Managers.Grid.CountNormalItem(key);
        countText.text = $"{currentCount}/{goalCount}";
        if(currentCount>= goalCount)
        {
            if (IsFulfill) return;
            ActivateCheckIcon();
        }
        else
        {

            DeactivateCheckIcon();
        }
    }
    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    Debug.Log("Click");
    //    if (!canClick) return;
    //    switch (data.type)
    //    {
    //        case ItemType.Normal:
    //        case ItemType.Generatable:
    //            Debug.Log("아이템 정보창 팝업");
    //            break;
    //        default:
    //            break;

    //    }

    //}

    // InfoButton 활성화 메서드 추가
    public void ActivateInfoButton()
    {
        if (infoButton != null)
        {
            //infoButton.onClick.AddListener(() =>
            //{
            //    Managers.UI.infoPanelController.ShowInfoPanel(data);
            //});
            infoButton.gameObject.SetActive(true);
        }
        
    }
    // checkIcon 활성화 메서드 추가
    public void ActivateCheckIcon()
    {
        checkIcon.SetActive(true);
        countText.enabled = false;
        checkIcon.transform.localScale = Vector3.one * 0.25f; // 초기 크기를 0.5로 설정
        IsFulfill = true;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(checkIcon.transform.DOScale(0.6f, 0.3f).SetEase(Ease.OutQuad))
                .Append(checkIcon.transform.DOScale(0.5f, 0.1f).SetEase(Ease.OutQuad))
                .OnComplete(() =>
                {
                });
    }
    // checkIcon 비활성화 메서드 추가
    public void DeactivateCheckIcon()
    {

        checkIcon.SetActive(false);
        countText.enabled = true;
        IsFulfill = false;
    }
    public void DeactivateAll()
    {

        checkIcon.SetActive(false);
        countText.enabled = false;
    }
}
