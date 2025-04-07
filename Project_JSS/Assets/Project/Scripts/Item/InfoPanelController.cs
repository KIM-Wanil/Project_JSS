using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.EventSystems;

public class InfoPanelController : MonoBehaviour
{
    public GameObject descInfo;
    public RectTransform itemNameInfo;
    public GameObject basicTextObj;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI priceText;
    public Button sellButton;

    void Start()
    {
        InitToBasicDesc();
        sellButton.onClick.AddListener(InitToBasicDesc);
        Managers.Game.onSellableItemSelected.AddListener(PrintSellableItemDesc);
        Managers.Game.onUnsellableItemSelected.AddListener(PrintUnsellableItemDesc);
        Managers.Game.onLockedItemSelected.AddListener(PrintLockedItemDesc);
        Managers.Game.onItemDeSelected.AddListener(InitToBasicDesc);
    }

    private void Update()
    {
        //if (Input.GetMouseButtonUp(0))
        //{
        //    Invoke("HandlePointerUp",0.1f);
        //}
    }

    //public void HandlePointerUp()
    //{
    //    // 클릭한 위치에 DraggableItem이 있는지 확인
    //    PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
    //    {
    //        position = Input.mousePosition
    //    };

    //    List<RaycastResult> results = new List<RaycastResult>();
    //    EventSystem.current.RaycastAll(pointerEventData, results);

    //    bool isDraggableItemClicked = results.Any(r => r.gameObject.GetComponent<DraggableItem>() != null);
    //    Debug.Log($"isDraggableItemClicked : {isDraggableItemClicked}");
    //    if (!isDraggableItemClicked && DraggableItem.currentlySelectedItem != null)
    //    {
            
    //        DraggableItem.currentlySelectedItem.mergeableItem.OnDeSelected();
    //        DraggableItem.currentlySelectedItem = null;
    //        InitToBasicDesc();
    //    }
    //}

    public void PrintSellableItemDesc(ItemKey inputKey, int price = -1, UnityAction onItemSold = null)
    {
        Debug.Log("판매 가능 아이템 정보 출력");
        ItemSO data = Managers.Game.GetItemData(inputKey.id);
        if (data.type != ItemType.Normal) return; //현재 노말 아이템만 판매 가능
        descInfo.SetActive(true);

        nameText.text = $"{data.items[inputKey.Lv - 1].itemName} (Lv {inputKey.Lv})";
        basicTextObj.SetActive(false);

        sellButton.onClick.AddListener(onItemSold);
        sellButton.onClick.AddListener(InitToBasicDesc);
        priceText.text = price.ToString();
        sellButton.gameObject.SetActive(true);

        itemNameInfo.offsetMax = new Vector2(-119f, itemNameInfo.offsetMax.y);
        descText.rectTransform.offsetMax = new Vector2(-119f, descText.rectTransform.offsetMax.y);
        descText.text = data.items[inputKey.Lv - 1].itemDesc;

    }
    public void PrintUnsellableItemDesc(ItemKey inputKey)
    {
        Debug.Log("판매 불가 아이템 정보 출력");
        ItemSO data = Managers.Game.GetItemData(inputKey.id);
        if (data.type == ItemType.Normal) return; //현재 노말 아이템만 판매 가능
        descInfo.SetActive(true);

        nameText.text = $"{data.items[inputKey.Lv - 1].itemName} (Lv {inputKey.Lv})";
        basicTextObj.SetActive(false);

        sellButton.onClick.RemoveAllListeners();
        sellButton.gameObject.SetActive(false);

        itemNameInfo.offsetMax = new Vector2(-25f, itemNameInfo.offsetMax.y);
        descText.rectTransform.offsetMax = new Vector2(-25f, descText.rectTransform.offsetMax.y);
        descText.text = data.items[inputKey.Lv - 1].itemDesc;
    }
    public void PrintLockedItemDesc(ItemKey inputKey)
    {
        Debug.Log("잠긴 아이템 정보 출력");
        ItemSO data = Managers.Game.GetItemData(inputKey.id);
        if (data.type != ItemType.Normal) return; //현재 노말 아이템만 판매 가능
        descInfo.SetActive(true);

        nameText.text = $"{data.items[inputKey.Lv - 1].itemName} (Lv {inputKey.Lv})";
        basicTextObj.SetActive(false);

        sellButton.onClick.RemoveAllListeners();
        sellButton.gameObject.SetActive(false);

        itemNameInfo.offsetMax = new Vector2(-25f, itemNameInfo.offsetMax.y);
        descText.rectTransform.offsetMax = new Vector2(-25f, descText.rectTransform.offsetMax.y);

        //descText.text = data.items[inputKey.lv - 1].itemDesc;
        //나중에 번역 넣을 때 localization key로 수정
        descText.text = "잠긴 아이템은 움직일 수 없지만, 같은 아이템으로 합성할 수 있습니다.";
    }
    public void InitToBasicDesc()
    {
        sellButton.onClick.RemoveAllListeners();
        sellButton.gameObject.SetActive(false);
        descInfo.SetActive(false);
        basicTextObj.SetActive(true);
    }
}
