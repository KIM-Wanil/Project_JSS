// Generator.cs - 아이템을 생성하는 제너레이터
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static GeneratorSO;
using Unity.VisualScripting;
using DG.Tweening;

public class Generator : MonoBehaviour
{
    public GeneratorSO genDB;
    private MergeableItem mergeableItem;
    private int currentDurability;
    private int maxDurability;
    private DraggableItem draggableItem; // DraggableItem 컴포넌트 참조

    [SerializeField] private GameObject durablilty;
    [SerializeField] private Image durabilityGauge; // 내구도를 표시할 Image 컴포넌트

    public GeneratorData genData;
    public List<Sprite> generatableSprites;

    public MergeEffect genEffect;
    private bool canGenerate = true;
    private void Awake()
    {
        mergeableItem = GetComponent<MergeableItem>();
        draggableItem = GetComponent<DraggableItem>(); // DraggableItem 컴포넌트 가져오기

        // Image 컴포넌트 추가
        //durablilty = transform.GetChild(1).gameObject;
        durablilty.SetActive(true);
        //durabilityGauge = durablilty.transform.Find("DurabilityGauge").GetComponent<Image>();

        if (genEffect.IsUnityNull())
        {
            genEffect = GetComponent<MergeEffect>();
            //genEffect.Init();
        }
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

    public void Initialize(float syncTime)
    {
        ////저렙 생산못하는 제너레이터 생기면 레벨 맞춰서 수정하기
        //if(mergeableItem.Lv < 4)
        //{
        //    canGenerate = false;
        //    mergeEffect.Init();
        //}
        //else
        //{
        //    canGenerate = true;
        //    mergeEffect.PlayMerge();
        //}
        genEffect.PlayEffectAtTime(syncTime);
        genData = genDB.generatorDatas[mergeableItem.Lv - 1];
        maxDurability = genData.maxDurability;
        currentDurability = maxDurability;
        UpdateDurabilityUI();

        generatableSprites = new List<Sprite>();

        foreach (var item in genData.generatableItems)
        {
            Debug.Log(item.key.id + item.key.Lv);
            generatableSprites.Add(Managers.Game.GetItemSprite(item.key));
        }
        //mergeableItem.button.onClick.AddListener(() =>
        //        Managers.Game.infoPanelController.PrintGeneratorDesc(mergeableItem.itemKey, generatableSprites));

        //// 버튼 클릭 이벤트 설정
        //if (generatorButton != null)
        //{
        //    generatorButton.onClick.AddListener(OnGeneratorClicked);
        //}
    }
    public void OnReuturnToItemPool()
    {
        mergeableItem = null;
        draggableItem = null;
        maxDurability = 0;
        currentDurability = 0;
        generatableSprites = null;

        durablilty.SetActive(false);
        durablilty = null;
        durabilityGauge = null; 
    }
    private void OnDestroy()
    {
        //// 버튼 클릭 이벤트 해제
        //if (generatorButton != null)
        //{
        //    generatorButton.onClick.RemoveListener(OnGeneratorClicked);
        //}
    }

    //private void OnGeneratorClicked()
    //{
    //    Debug.Log("isDragging: " + draggableItem.IsDragging() + ", CanClick: " + draggableItem.CanClick());
    //    // 드래그 중이거나 드래그가 끝난 직후에는 아이템 생성하지 않음
    //    if (draggableItem != null && (draggableItem.IsDragging() || !draggableItem.CanClick()))
    //    {
    //        return;
    //    }
    //    if(!draggableItem.IsSelected())
    //    {
    //       return;
    //    }
    //    TryGenerateItem();
    //}

    public bool TryGenerateItem()
    {
        if (currentDurability <= 0 || !Managers.Game.TrySpendEnergy(1))
            return false;

       Managers.Asset.PlaySound("Pop_Item", SoundType.Effect);

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
                Vector2Int? pos = Managers.Grid.GetNearestPosition(mergeableItem.gridPosition);
                if (pos == null)
                {
                    Debug.Log("No empty position");
                    return false;
                }
                else
                {
                    //나중에 usable아이템 제너레이터도 추가
                    if (Managers.Game.SpawnMoveItem(item.key.id, item.key.Lv, this.transform.position, (Vector2Int)pos)) { }
                    //if (Managers.Game.SpawnMoveItem(item.key.id, item.key.Lv, Managers.Grid.GetTilePosition(mergeableItem.GridPosition), (Vector2Int)pos)) { }
                }
                break;
            }
        }

        DG.Tweening.Sequence sequence = DOTween.Sequence();
        sequence.Append(mergeableItem.rectT.DOScale(new Vector3(1.08f, 0.8f, 1f), 13f / 60f).SetEase(Ease.OutQuad))
                .Append(mergeableItem.rectT.DOScale(new Vector3(0.9f, 1.25f, 0.95f), 7f / 60f).SetEase(Ease.OutQuad))
                .Append(mergeableItem.rectT.DOScale(new Vector3(1.1f, 0.94f, 1f), 12f / 60f).SetEase(Ease.OutQuad))
                .Append(mergeableItem.rectT.DOScale(new Vector3(0.96f, 1f, 1f), 10f / 60f).SetEase(Ease.OutQuad))
                .Append(mergeableItem.rectT.DOScale(new Vector3(1f, 1f, 1f), 10f / 60f).SetEase(Ease.OutQuad));
        sequence.Play();
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
