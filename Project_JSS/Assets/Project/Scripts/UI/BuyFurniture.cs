using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyFurniture : MonoBehaviour
{

    [SerializeField] FurniturePlacementManager furniturePlacementManager;
    [SerializeField] GameObject ui;
    [SerializeField] FloorData[] floorDatas;
    [SerializeField] List<FurnitureData>  furnitureInfos;
    [SerializeField] GameObject prefab;
    [SerializeField] Transform buttonTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        furnitureInfos = new List<FurnitureData>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetUI(bool onoff)
    {
        if (StateManager.instance.ButItem)
        {
            return;
        }
        ui.SetActive(onoff);
        if (onoff == true)
        {
            GetInfo();
        }
    }
    void GetInfo()
    {
        foreach (FloorData data in floorDatas)
        {
            if (data.isUnlock)
            {
                foreach (FurnitureData info in data.furnitureInfos)
                {
                    if (info.isUnlocked == false)
                    {
                        if (furnitureInfos.Find(value => info.furnitureName == value.furnitureName) == null)
                        {
                            furnitureInfos.Add(info);
                            GameObject newObj = Object.Instantiate(prefab, buttonTransform);
                            newObj.GetComponent<BuyFurnitureButton>().Setting(this,furniturePlacementManager, info);
                        }
                        break;
                    }

                }
            }
        }
    }


}
