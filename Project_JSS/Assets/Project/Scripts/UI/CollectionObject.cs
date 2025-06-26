using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectionObject : MonoBehaviour
{
    Collection collection;
    FurniturePlacementManager furniturePlacementManager;
    int floorIndex;
    [SerializeField] Slider slider;
    [SerializeField] Image image;

    [SerializeField] TextMeshProUGUI floorNmae;
    [SerializeField] TextMeshProUGUI floorDescription;
    [SerializeField] TextMeshProUGUI floorAchievement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public ref Slider Setting(ref FurniturePlacementManager furniturePlacementManager,Collection collection, int index)
    {
        this.furniturePlacementManager = furniturePlacementManager;
        this.collection = collection;
        floorIndex = index;
        slider.maxValue = furniturePlacementManager.floorData[index].furnitureInfos.Length;
        floorNmae.text = furniturePlacementManager.floorData[index].floorNmae;
        image.sprite = furniturePlacementManager.floorData[index].mainSprite;
        floorDescription.text = furniturePlacementManager.floorData[index].floorDescription;
        return ref slider;
    }
    public void UpdateCounting()
    {
        float percentage = (slider.value) / (slider.maxValue) * 100f;
        floorAchievement.text = string.Format("{0:0}%", percentage); ;
    }
    public void SwitchFloor()
    {
        furniturePlacementManager.SwitchFloor(floorIndex);
        collection.SetUI(false);
    }
}
