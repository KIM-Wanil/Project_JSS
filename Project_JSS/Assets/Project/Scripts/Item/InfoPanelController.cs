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

    public Button giveUpButton;
    public Button earnByAdButton;
    public TextMeshProUGUI adCountText;
    public Button skipButton;
    public Button earnByGemButton;
    public TextMeshProUGUI priceToBuyText;
    public TextMeshProUGUI timeToDisappearText;
    public Button sellButton;
    public TextMeshProUGUI priceToSellText;

    void Start()
    {
        InitToBasicDesc();
        sellButton.onClick.AddListener(InitToBasicDesc);
        Managers.Game.onSellableItemSelected.AddListener(PrintSellableItemDesc);
        Managers.Game.onUnsellableItemSelected.AddListener(PrintUnsellableItemDesc);
        Managers.Game.onLockedItemSelected.AddListener(PrintLockedItemDesc);
        Managers.Game.onItemDeSelected.AddListener(InitToBasicDesc);

        Managers.Game.onAdBubbleItemSelected.AddListener(PrintAdBubbleItemDesc);
        Managers.Game.onGemBubbleItemSelected.AddListener(PrintGemBubbleItemDesc);
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
        InitAllButtons();

        Debug.Log("판매 가능 아이템 정보 출력");
        ItemSO data = Managers.Game.GetItemData(inputKey.id);
        if (data.type != ItemType.Normal) return; //현재 노말 아이템만 판매 가능
        descInfo.SetActive(true);

        nameText.text = $"{data.items[inputKey.Lv - 1].itemName} (Lv {inputKey.Lv})";
        basicTextObj.SetActive(false);

        sellButton.onClick.AddListener(onItemSold);
        sellButton.onClick.AddListener(InitToBasicDesc);
        priceToSellText.text = price.ToString();
        sellButton.gameObject.SetActive(true);

        //itemNameInfo.offsetMax = new Vector2(-119f, itemNameInfo.offsetMax.y);
        //descText.rectTransform.offsetMax = new Vector2(-119f, descText.rectTransform.offsetMax.y);
        descText.text = data.items[inputKey.Lv - 1].itemDesc;

    }
    public void PrintUnsellableItemDesc(ItemKey inputKey)
    {
        InitAllButtons();

        Debug.Log("판매 불가 아이템 정보 출력");
        ItemSO data = Managers.Game.GetItemData(inputKey.id);
        if (data.type == ItemType.Normal) return; //현재 노말 아이템만 판매 가능
        descInfo.SetActive(true);

        nameText.text = $"{data.items[inputKey.Lv - 1].itemName} (Lv {inputKey.Lv})";
        basicTextObj.SetActive(false);

        //itemNameInfo.offsetMax = new Vector2(-25f, itemNameInfo.offsetMax.y);
        //descText.rectTransform.offsetMax = new Vector2(-25f, descText.rectTransform.offsetMax.y);
        descText.text = data.items[inputKey.Lv - 1].itemDesc;
    }
    public void PrintLockedItemDesc(ItemKey inputKey)
    {
        InitAllButtons();

        Debug.Log("잠긴 아이템 정보 출력");
        ItemSO data = Managers.Game.GetItemData(inputKey.id);
        if (data.type != ItemType.Normal) return; //현재 노말 아이템만 판매 가능
        descInfo.SetActive(true);

        nameText.text = $"{data.items[inputKey.Lv - 1].itemName} (Lv {inputKey.Lv})";
        basicTextObj.SetActive(false);


        //itemNameInfo.offsetMax = new Vector2(-25f, itemNameInfo.offsetMax.y);
        //descText.rectTransform.offsetMax = new Vector2(-25f, descText.rectTransform.offsetMax.y);

        //descText.text = data.items[inputKey.lv - 1].itemDesc;
        //나중에 번역 넣을 때 localization key로 수정
        descText.text = "잠긴 아이템은 움직일 수 없지만, 같은 아이템으로 합성할 수 있습니다.";
    }

    public void PrintAdBubbleItemDesc(ItemKey inputKey, UnityAction onBubblePop = null, UnityAction onGiveUp = null)
    {
        InitAllButtons();

        Debug.Log("잠긴 아이템 정보 출력");
        ItemSO data = Managers.Game.GetItemData(inputKey.id);
        if (data.type != ItemType.Normal) return; //현재 노말 아이템만 판매 가능
        descInfo.SetActive(true);

        nameText.text = $"{data.items[inputKey.Lv - 1].itemName} (Lv {inputKey.Lv})";
        basicTextObj.SetActive(false);


        giveUpButton.onClick.AddListener(onGiveUp);
        giveUpButton.onClick.AddListener(InitToBasicDesc);
        giveUpButton.gameObject.SetActive(true);

        //첫 클릭에 광고 나오게 바꾸고, 광고개수 세서 1/1 이렇게 됐을 때 누르면 그 때 아이템 얻어지게 수정
        //earnByAdButton.onClick.AddListener();
        earnByAdButton.onClick.AddListener(onBubblePop);
        earnByAdButton.onClick.AddListener(InitToBasicDesc);
        adCountText.text = "0/1";// 
        earnByAdButton.gameObject.SetActive(true);


        descText.text = "광고시청으로 어쩌고저쩌고";
    }

    public void PrintGemBubbleItemDesc(ItemKey inputKey, UnityAction onBubblePop = null, UnityAction onSkip = null)
    {
        InitAllButtons();

        Debug.Log("잠긴 아이템 정보 출력");
        ItemSO data = Managers.Game.GetItemData(inputKey.id);
        if (data.type != ItemType.Normal) return; //현재 노말 아이템만 판매 가능
        descInfo.SetActive(true);

        nameText.text = $"{data.items[inputKey.Lv - 1].itemName} (Lv {inputKey.Lv})";
        basicTextObj.SetActive(false);


        skipButton.onClick.AddListener(onSkip);
        skipButton.onClick.AddListener(InitToBasicDesc);
        skipButton.gameObject.SetActive(true);

        //첫 클릭에 광고 나오게 바꾸고, 광고개수 세서 1/1 이렇게 됐을 때 누르면 그 때 아이템 얻어지게 수정
        //earnByAdButton.onClick.AddListener();
        earnByGemButton.onClick.AddListener(onBubblePop);
        earnByGemButton.onClick.AddListener(InitToBasicDesc);
        adCountText.text = "0/1";// 
        earnByAdButton.gameObject.SetActive(true);


        descText.text = "젬을 사용해 어쩌고 저쩌고";
    }
    public void InitToBasicDesc()
    {
        InitAllButtons();
        descInfo.SetActive(false);
        basicTextObj.SetActive(true);
    }

    public void InitAllButtons()
    {
        sellButton.onClick.RemoveAllListeners();
        sellButton.gameObject.SetActive(false);

        earnByAdButton.onClick.RemoveAllListeners();
        earnByAdButton.gameObject.SetActive(false);

        skipButton.onClick.RemoveAllListeners();
        skipButton.gameObject.SetActive(false);

        earnByGemButton.onClick.RemoveAllListeners();
        earnByGemButton.gameObject.SetActive(false);

        giveUpButton.onClick.RemoveAllListeners();
        giveUpButton.gameObject.SetActive(false);
    }
}
