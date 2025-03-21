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
    public GameObject normalTextObj;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI priceText;
    public Button sellButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
        sellButton.onClick.AddListener(Init);
    }
    private void Update()
    {
        //수정 필요
        //if (Input.GetMouseButtonUp(0))
        //{
        //    // 클릭한 위치에 DraggableItem이 있는지 확인
        //    PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        //    {
        //        position = Input.mousePosition
        //    };

        //    List<RaycastResult> results = new List<RaycastResult>();
        //    EventSystem.current.RaycastAll(pointerEventData, results);

        //    bool isDraggableItemClicked = results.Any(r => r.gameObject.GetComponent<DraggableItem>() != null);

        //    if (!isDraggableItemClicked && DraggableItem.currentlySelectedItem != null)
        //    {
        //        DraggableItem.currentlySelectedItem.mergeableItem.OnDeSelected();
        //        DraggableItem.currentlySelectedItem = null;
        //        Init();
        //    }
        //}
    }

    public void PrintItemDesc(ItemKey inputKey,int price = -1, UnityAction onItemSold = null)
    {


        descInfo.SetActive(true);
        ItemSO data = Managers.Game.GetItemData(inputKey.id);


        //아이템 이름 오른쪽에 표시
        nameText.text = data.items[inputKey.lv-1].itemName;
        switch (data.type)
        {
            case ItemType.Normal:
                normalTextObj.SetActive(false);

                sellButton.onClick.AddListener(onItemSold);
                sellButton.onClick.AddListener(Init);
                priceText.text = price.ToString();
                sellButton.gameObject.SetActive(true);

                itemNameInfo.offsetMax = new Vector2(-119f, itemNameInfo.offsetMax.y);

                descText.rectTransform.offsetMax = new Vector2(-119f, descText.rectTransform.offsetMax.y);
                descText.text = data.items[inputKey.lv - 1].itemDesc;

                break;

            case ItemType.Generatable:
                normalTextObj.SetActive(false);

                sellButton.onClick.RemoveAllListeners();
                sellButton.gameObject.SetActive(false);

                itemNameInfo.offsetMax = new Vector2(-25f, itemNameInfo.offsetMax.y);

                descText.rectTransform.offsetMax = new Vector2(-25f, descText.rectTransform.offsetMax.y);
                descText.text = data.items[inputKey.lv - 1].itemDesc;

                break;

            default:
                break;
        }
    }
    public void Init()
    {
        sellButton.onClick.RemoveAllListeners();
        sellButton.gameObject.SetActive(false);
        descInfo.SetActive(false);
        normalTextObj.SetActive(true);

    }

}
