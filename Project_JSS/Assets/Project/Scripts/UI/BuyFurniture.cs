using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyFurniture : MonoBehaviour
{

    [SerializeField] FurniturePlacementManager furniturePlacementManager;
    [SerializeField] GameObject ui;
    [SerializeField] FloorData[] floorDatas;
    [SerializeField] BuyFurnitureButton[] objects;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // for (int i = 0; i < floorDatas.Length; i++)
        // {
        //     objects[i].Setting(floorDatas[i], this, furniturePlacementManager);
        // }
        GetInfo();
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
        if (onoff)
        {
            GetInfo();
        }
        else
        {
            TutorialManager.TriggerCondition(TutorialCondition.제작목록클릭);
        }
    }
    void GetInfo()
    {
        for (int i =0;i < floorDatas.Length; i++ )
        {
            if (!floorDatas[i].isUnlock)
            {
                floorDatas[i].isUnlock = true;
                objects[i].UnLock();
            }
        }
        SetObjectButton();
    }
    void SetObjectButton()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].SetButtonText();
        }
    }


}
