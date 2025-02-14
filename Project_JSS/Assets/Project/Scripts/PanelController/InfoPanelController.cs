using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
public class InfoPanelController : MonoBehaviour
{
    public GameObject descInfo;
    public GameObject normalDesc;
    public GameObject generatorDesc;
    public ItemInfo targetItem;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI priceText;
    public GameObject[] generatableItemSlots = new GameObject[4];
    public Image[] generatableItemImages = new Image[4];

    public GameObject mergeInfo;
    public ItemInfo[] componentItems = new ItemInfo[2];

    public Button sellButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
        sellButton.onClick.AddListener(Init);
    }
    private void Init()
    {
        targetItem.gameObject.SetActive(false);
        targetItem.canClick = false;
        descInfo.SetActive(false);
        mergeInfo.SetActive(false);
    }
    public void SetTargetItemBox(ItemKey inputKey)
    {
        targetItem.Init(inputKey);
        targetItem.ActivateNameText();
        targetItem.ActivateInfoButton();
        targetItem.gameObject.SetActive(true);
    }
    public void PrintItemDesc(ItemKey inputKey,int price, UnityAction onItemSold)
    {
        //정보 볼 아이템 설정
        SetTargetItemBox(inputKey);

        //설명창O 조합식X
        descInfo.SetActive(true);
        mergeInfo.SetActive(false);

        //아이템이름이 오른쪽에 표시되니까 아이템밑에 이름 설정X & 정보보기 버튼 활성화
        targetItem.DeactivateNameText();
        targetItem.ActivateInfoButton();

        //아이템 이름 오른쪽에 표시
        nameText.text = targetItem.details.itemName;

        //설명창 표시
        normalDesc.SetActive(true);
        generatorDesc.SetActive(false);

        switch(targetItem.data.type)
        {
            case ItemType.Normal:
                //아이템 정보 표시
                if (targetItem.lvIndex + 1 < targetItem.data.items.Length)
                {
                    descText.text = $"Merge and create {targetItem.data.items[targetItem.lvIndex + 1].itemName} (Lv.{targetItem.key.lv + 1})";
                }
                else
                {
                    descText.text = "Max level reached";
                }
                break;

            case ItemType.Crafted:
                ItemKey[] componentItemKey = Managers.Game.FindCraftingComponents(inputKey);
                string componentItemName1 = Managers.Game.GetItemName(componentItemKey[0]);
                string componentItemName2 = Managers.Game.GetItemName(componentItemKey[1]);
                descText.text = $"{componentItemName1} + {componentItemName2}";
                break;
        }
       

        //가격 표시
        priceText.text = price.ToString();
        sellButton.onClick.AddListener(onItemSold);

    }
    public void PrintGeneratorDesc(ItemKey inputKey, List<Sprite> sprites)
    {
        //정보 볼 아이템 설정
        SetTargetItemBox(inputKey);

        //설명창O 조합식X
        descInfo.SetActive(true);
        mergeInfo.SetActive(false);

        //아이템이름이 오른쪽에 표시되니까 아이템밑에 이름 설정X & 정보보기 버튼 활성화
        targetItem.DeactivateNameText();
        targetItem.ActivateInfoButton();

        //아이템 이름 오른쪽에 표시
        nameText.text = targetItem.details.itemName;

        normalDesc.SetActive(false);
        generatorDesc.SetActive(true);

        for(int i = 0; i < generatableItemImages.Length; i++)
        {
            if(i < sprites.Count)
            {
                generatableItemSlots[i].SetActive(true);
                generatableItemImages[i].sprite = sprites[i];
            }
            else
            {
                generatableItemSlots[i].SetActive(false);
                generatableItemImages[i].sprite = null;
            } 
        }

    }
    public void PrintComponentItems(ItemKey inputKey)
    {
        //정보 볼 아이템 설정
        SetTargetItemBox(inputKey);

        //설명창X 조합식O
        descInfo.SetActive(false);
        mergeInfo.SetActive(true);

        ItemKey[] componentItemKey = new ItemKey[2];

        switch (targetItem.data.type)
        {
            case ItemType.Normal:
                Debug.Log(inputKey.lv);
                if (inputKey.lv > 1)
                {
                    ItemKey downgradeItemKey = new ItemKey(inputKey.id, inputKey.lv - 1);
                    componentItemKey[0] = downgradeItemKey;
                    componentItemKey[1] = downgradeItemKey;
                }
                else
                {
                    UnityAction action = Managers.Game.PrintGeneratableGeneratorDesc(inputKey);
                    action();
                    return;
                }
                break;

            case ItemType.Crafted:
                componentItemKey = Managers.Game.FindCraftingComponents(inputKey);
                break;
            default:
                Debug.Log("Invalid item type error");
                return;
        }

        for(int i = 0; i < componentItems.Length; i++)
        {
            componentItems[i].Init(componentItemKey[i]);
            componentItems[i].ActivateNameText();
            if (Managers.Grid.DoesItemExist(componentItemKey[i]))
            {
                componentItems[i].ActivateCheckIcon();
            }
        }
        Debug.Log("PrintComponentItems");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
