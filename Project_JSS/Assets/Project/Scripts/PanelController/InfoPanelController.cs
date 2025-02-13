using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;
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

        //아이템 정보 표시
        if (targetItem.lvIndex + 1 < targetItem.data.items.Length)
        {
            descText.text = $"Merge and create {targetItem.data.items[targetItem.lvIndex + 1].itemName} (Lv.{targetItem.key.lv + 1})";
        }
        else
        {
            descText.text = "Max level reached";
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
        descInfo.SetActive(false);
        mergeInfo.SetActive(true);

        SetTargetItemBox(inputKey);

        if (inputKey.lv < 2)
        {
            return;
        }
        //일반 머지 아이템만 해당. 크래프팅아이템의 경우 나중에 추가
        ItemKey componentItem = new ItemKey(inputKey.id, inputKey.lv-1);

        for(int i = 0; i < componentItems.Length; i++)
        {
            componentItems[i].Init(componentItem);
            componentItems[i].ActivateNameText();
            if(Managers.Grid.DoesItemExist(componentItem))
            {
                componentItems[i].ActivateCheckIcon();
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
