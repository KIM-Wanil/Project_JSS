using UnityEngine;
using UnityEngine.UI;

public class Collection : MonoBehaviour
{
    [SerializeField] FurniturePlacementManager furniturePlacementManager;
    [SerializeField] GameObject ui;
    [SerializeField] GameObject prefab;
    [SerializeField] Transform buttonTransform;
    [SerializeField] Slider[] sliders;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sliders = new Slider[furniturePlacementManager.floorData.Length];
        int i = 0;
        foreach (FloorData data in furniturePlacementManager.floorData)
        {
            GameObject newObj = Object.Instantiate(prefab, buttonTransform);
            sliders[i] = newObj.GetComponent<CollectionObject>().Setting(ref furniturePlacementManager,this, i);
            sliders[i].value = furniturePlacementManager.floorData[i].UnlockCounting();
            i++;
        }
    }
    public void SetUI(bool onoff)
    {
        if (StateManager.instance.ButItem)
        {
            return;
        }
        ui.SetActive(onoff);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateSlider(int index)
    {
        sliders[index].value = furniturePlacementManager.floorData[index].UnlockCounting();
    }
}
