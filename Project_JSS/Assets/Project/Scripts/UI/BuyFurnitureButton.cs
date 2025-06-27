using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyFurnitureButton : MonoBehaviour
{
    [SerializeField] FloorData floorData;
    BuyFurniture buyFurniture;
    FurniturePlacementManager furniturePlacementManager;
    FurnitureData data;
    [SerializeField] TextMeshProUGUI floorNmae;
    [SerializeField] TextMeshProUGUI floorDescription;

    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI furnitureNmae;

    [SerializeField] Slider slider;
    [SerializeField] GameObject lockObject;
    [SerializeField] GameObject buybuttonObject;

    [SerializeField] TextMeshProUGUI requiredLV;
    [SerializeField] TextMeshProUGUI floorAchievement;
    [SerializeField] private UiController uiController; 
    [SerializeField] private TextMeshProUGUI buttonText; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateCounting()
    {
        float percentage = (slider.value) / (slider.maxValue) * 100f;
        floorAchievement.text = string.Format("{0:0}%", percentage); ;
    }
    public void Buy()
    {
        //3 -> 나중에 가구 설치 필요 별 개수로 수정
        if (!Managers.Game.TrySpendStar(3)) return;

        if (Managers.Game.sceneState == SceneState.Hotel)
        {
            furniturePlacementManager.AddFurnitureToFloor(data, data.floorNum);
            buyFurniture.SetUI(false);
            StateManager.instance.ButItem = true;
            Setting();
        }
        else if (Managers.Game.sceneState == SceneState.Merge)
        {
            Sequence sequence = uiController.MoveToHotelSequence();
            sequence.PrependCallback(() =>
            {
                buyFurniture.SetUI(false);
            });
            sequence.OnComplete(() =>
            {
                furniturePlacementManager.AddFurnitureToFloor(data, data.floorNum);
                buyFurniture.SetUI(false);
                StateManager.instance.ButItem = true;
                Setting();
            });
        }
        else
        {
            Debug.LogError("잘못된 씬입니다.");
        }
        
    }
    public void Setting(FloorData floorData, BuyFurniture buyFurniture, FurniturePlacementManager furniturePlacementManager)
    {
        this.floorData = floorData;
        slider.maxValue = floorData.furnitureInfos.Length;
        slider.value = floorData.UnlockCounting();
        floorNmae.text = floorData.floorNmae;
        floorDescription.text = floorData.floorDescription;

        UpdateCounting();
        this.buyFurniture = buyFurniture;
        this.furniturePlacementManager = furniturePlacementManager;
      //  furnitureDescription.text = data.furnitureDescription;

        if (floorData.isUnlock)
        {
            slider.gameObject.SetActive(true);
            buybuttonObject.SetActive(true);
            lockObject.SetActive(false);
        }
        else
        {
            requiredLV.text = "LV" + floorData.floorNum * 5 + " 달성";
        }
        Setting();
    }
    public void UnLock()
    {
        slider.gameObject.SetActive(true);
        buybuttonObject.SetActive(true);
        lockObject.SetActive(false);
        Setting();
        if (Managers.Game.sceneState == SceneState.Hotel)
        {
            buttonText.text = "제작";
        }
        else if (Managers.Game.sceneState == SceneState.Merge)
        {
            buttonText.text = "이동";
        }
        else
        {
            Debug.LogError("잘못된 씬입니다.");
        }
    }
    public void Setting()
    {
        if (!floorData.isUnlock)
            return;
        slider.maxValue = floorData.furnitureInfos.Length;
        slider.value = floorData.UnlockCounting();
        UpdateCounting();
        foreach (FurnitureData info in floorData.furnitureInfos)
        {
            if (info.isUnlocked == false)
            {
                this.data = info;
                image.sprite = data.furnitureSprite[0].sprites[0];
                furnitureNmae.text = data.furnitureName;
                return;
            }

        }
        Destroy(this.gameObject);
        return;

    }
}
