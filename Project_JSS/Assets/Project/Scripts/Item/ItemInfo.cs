using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEditor.Localization.Plugins.XLIFF.V12;

public class ItemInfo : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image image;
    [SerializeField] private GameObject checkIcon;
    [SerializeField] private Button infoButton;
    [SerializeField] private TextMeshProUGUI countText;

    public ItemSO data;
    public ItemDetails details;
    public ItemKey key;
    public bool IsComplete { get; private set; } = false;
    public int lvIndex;
    public int goalCount = 0;
    public int currentCount = 0;

    public bool canClick = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init(ItemKey inputKey, int inputGoalCount)
    {
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
        currentCount = Managers.Grid.CountItem(key);
        countText.text = $"{currentCount}/{goalCount}";
        if(currentCount>= goalCount)
        {
            IsComplete = true;
        }
        else
        {
            IsComplete = false;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click");
        if (!canClick) return;
        switch (data.type)
        {
            case ItemType.Normal:
            case ItemType.Generatable:
                Debug.Log("아이템 정보창 팝업");
                break;
            default:
                break;

        }

    }

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
    //public void ActivateCheckIcon()
    //{
    //    if (checkIcon != null)
    //    {
    //        isExist = true;
    //        checkIcon.SetActive(true);
    //    }
    //}
    //// checkIcon 비활성화 메서드 추가
    //public void DeactivateCheckIcon()
    //{
    //    if (checkIcon != null)
    //    {
    //        isExist = false;
    //        checkIcon.SetActive(false);
    //    }
    //}
}
