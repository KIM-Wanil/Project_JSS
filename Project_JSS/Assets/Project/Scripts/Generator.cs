// Generator.cs - 아이템을 생성하는 제너레이터
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static GeneratorDB;

public class Generator : MonoBehaviour
{
    public GeneratorDB genDB;
    public Button generatorButton; // 버튼 컴포넌트 추가
    private MergeableItem mergeableItem;
    private int currentDurability;
    private int maxDurability;
    private DraggableItem draggableItem; // DraggableItem 컴포넌트 참조

    private GameObject durablilty;
    private Image durabilityGauge; // 내구도를 표시할 Image 컴포넌트

    private GeneratorData genData;
    private void Awake()
    {
        mergeableItem = GetComponent<MergeableItem>();
        draggableItem = GetComponent<DraggableItem>(); // DraggableItem 컴포넌트 가져오기
        generatorButton = mergeableItem.button;

        // Image 컴포넌트 추가
        durablilty = transform.GetChild(1).gameObject;
        durablilty.SetActive(true);
        durabilityGauge = durablilty.transform.Find("DurabilityGauge").GetComponent<Image>();

        //durabilitySlider = imageObject.AddComponent<Image>();
        //if(durabilitySlider != null)
        //{
        //    durabilitySlider.type = Image.Type.Filled;
        //    durabilitySlider.fillMethod = Image.FillMethod.Horizontal;
        //    durabilitySlider.fillOrigin = (int)Image.OriginHorizontal.Left;
        //    durabilitySlider.rectTransform.sizeDelta = new Vector2(100, 10); // 크기 조정
        //    durabilitySlider.rectTransform.anchoredPosition = new Vector2(0, -50); // 위치 조정
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

        //// 버튼 클릭 이벤트 설정
        //if (generatorButton != null)
        //{
        //    generatorButton.onClick.AddListener(OnGeneratorClicked);
        //}
    }

    private void OnDestroy()
    {
        //// 버튼 클릭 이벤트 해제
        //if (generatorButton != null)
        //{
        //    generatorButton.onClick.RemoveListener(OnGeneratorClicked);
        //}
    }

    private void OnGeneratorClicked()
    {
        Debug.Log("isDragging: " + draggableItem.IsDragging() + ", CanClick: " + draggableItem.CanClick());
        // 드래그 중이거나 드래그가 끝난 직후에는 아이템 생성하지 않음
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
                    //나중에 usable아이템 제너레이터도 추가
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
