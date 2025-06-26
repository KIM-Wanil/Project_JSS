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

        furniturePlacementManager.AddFurnitureToFloor(data,data.floorNum);
        buyFurniture.SetUI(false);
        StateManager.instance.ButItem = true;
        Setting();
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
            requiredLV.text = "LV" + floorData.floorNum * 5 + " ´Þ¼º";
        }
        Setting();
    }
    public void UnLock()
    {
            slider.gameObject.SetActive(true);
            buybuttonObject.SetActive(true);
            lockObject.SetActive(false);
        Setting();
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
