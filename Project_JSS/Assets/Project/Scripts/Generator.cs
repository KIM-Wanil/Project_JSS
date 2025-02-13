// Generator.cs - �������� �����ϴ� ���ʷ�����
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static GeneratorDB;

public class Generator : MonoBehaviour
{
    public GeneratorDB genDB;
    public Button generatorButton; // ��ư ������Ʈ �߰�
    private MergeableItem mergeableItem;
    private int currentDurability;
    private int maxDurability;
    private DraggableItem draggableItem; // DraggableItem ������Ʈ ����

    private GameObject durablilty;
    private Image durabilityGauge; // �������� ǥ���� Image ������Ʈ

    private GeneratorData genData;
    private void Awake()
    {
        mergeableItem = GetComponent<MergeableItem>();
        draggableItem = GetComponent<DraggableItem>(); // DraggableItem ������Ʈ ��������
        generatorButton = mergeableItem.button;

        // Image ������Ʈ �߰�
        durablilty = transform.GetChild(1).gameObject;
        durablilty.SetActive(true);
        durabilityGauge = durablilty.transform.Find("DurabilityGauge").GetComponent<Image>();

        //durabilitySlider = imageObject.AddComponent<Image>();
        //if(durabilitySlider != null)
        //{
        //    durabilitySlider.type = Image.Type.Filled;
        //    durabilitySlider.fillMethod = Image.FillMethod.Horizontal;
        //    durabilitySlider.fillOrigin = (int)Image.OriginHorizontal.Left;
        //    durabilitySlider.rectTransform.sizeDelta = new Vector2(100, 10); // ũ�� ����
        //    durabilitySlider.rectTransform.anchoredPosition = new Vector2(0, -50); // ��ġ ����
        //}
    }

    public void Initialize()
    {
        genData = genDB.generatorDatas[mergeableItem.Lv - 1];
        maxDurability = genData.maxDurability;
        currentDurability = maxDurability;
        UpdateDurabilityUI();

        List<Sprite> generatableSprites = new List<Sprite>();

        foreach (var item in genData.generatableItems)
        {
            generatableSprites.Add(Managers.Game.GetItemSprite(item.key));
        }
        mergeableItem.button.onClick.AddListener(() =>
                Managers.Game.infoPanelController.PrintGeneratorDesc(mergeableItem.itemKey, generatableSprites));

        //// ��ư Ŭ�� �̺�Ʈ ����
        //if (generatorButton != null)
        //{
        //    generatorButton.onClick.AddListener(OnGeneratorClicked);
        //}
    }

    private void OnDestroy()
    {
        //// ��ư Ŭ�� �̺�Ʈ ����
        //if (generatorButton != null)
        //{
        //    generatorButton.onClick.RemoveListener(OnGeneratorClicked);
        //}
    }

    private void OnGeneratorClicked()
    {
        Debug.Log("isDragging: " + draggableItem.IsDragging() + ", CanClick: " + draggableItem.CanClick());
        // �巡�� ���̰ų� �巡�װ� ���� ���Ŀ��� ������ �������� ����
        if (draggableItem != null && (draggableItem.IsDragging() || !draggableItem.CanClick()))
        {
            return;
        }
        if(!draggableItem.IsSelected())
        {
           return;
        }
        TryGenerateItem();
    }

    public bool TryGenerateItem()
    {
        if (currentDurability <= 0 || !Managers.Game.TrySpendEnergy(genDB.energyCost))
            return false;

        currentDurability--;
        UpdateDurabilityUI();

        float randomValue = Random.value;
        float accumulatedChance = 0;

        foreach (GeneratableItem item in genDB.generatorDatas[mergeableItem.Lv - 1].generatableItems)
        {
            Debug.Log(item.key.id);
            accumulatedChance += item.spawnChance;

            if (randomValue <= accumulatedChance)
            {
                Vector2Int? pos = Managers.Grid.GetEmptyPosition();
                if (pos == null)
                {
                    Debug.Log("No empty position");
                    return false;
                }
                else
                {
                    //���߿� usable������ ���ʷ����͵� �߰�
                    Managers.Game.SpawnItem(item.key.id, item.key.lv, (Vector2Int)pos);
                }
                break;
            }
        }

        return true;
    }

    private void UpdateDurabilityUI()
    {
        if (durabilityGauge != null)
        {
            durabilityGauge.fillAmount = (float)currentDurability / maxDurability;
        }
    }
}
