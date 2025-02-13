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
        //���� �� ������ ����
        SetTargetItemBox(inputKey);

        //����âO ���ս�X
        descInfo.SetActive(true);
        mergeInfo.SetActive(false);

        //�������̸��� �����ʿ� ǥ�õǴϱ� �����۹ؿ� �̸� ����X & �������� ��ư Ȱ��ȭ
        targetItem.DeactivateNameText();
        targetItem.ActivateInfoButton();

        //������ �̸� �����ʿ� ǥ��
        nameText.text = targetItem.details.itemName;

        //����â ǥ��
        normalDesc.SetActive(true);
        generatorDesc.SetActive(false);

        //������ ���� ǥ��
        if (targetItem.lvIndex + 1 < targetItem.data.items.Length)
        {
            descText.text = $"Merge and create {targetItem.data.items[targetItem.lvIndex + 1].itemName} (Lv.{targetItem.key.lv + 1})";
        }
        else
        {
            descText.text = "Max level reached";
        }

        //���� ǥ��
        priceText.text = price.ToString();
        sellButton.onClick.AddListener(onItemSold);

    }
    public void PrintGeneratorDesc(ItemKey inputKey, List<Sprite> sprites)
    {
        //���� �� ������ ����
        SetTargetItemBox(inputKey);

        //����âO ���ս�X
        descInfo.SetActive(true);
        mergeInfo.SetActive(false);

        //�������̸��� �����ʿ� ǥ�õǴϱ� �����۹ؿ� �̸� ����X & �������� ��ư Ȱ��ȭ
        targetItem.DeactivateNameText();
        targetItem.ActivateInfoButton();

        //������ �̸� �����ʿ� ǥ��
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
        //�Ϲ� ���� �����۸� �ش�. ũ�����þ������� ��� ���߿� �߰�
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
