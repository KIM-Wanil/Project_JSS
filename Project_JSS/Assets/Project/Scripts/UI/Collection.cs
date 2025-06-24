using UnityEngine;
using UnityEngine.UI;

public class Collection : MonoBehaviour
{
    [SerializeField] FurniturePlacementManager furniturePlacementManager;
    [SerializeField] GameObject ui;
    [SerializeField] GameObject prefab;
    [SerializeField] Transform buttonTransform;
    [SerializeField] Slider[] sliders;

    [SerializeField] Image[] buttonImages;
    [SerializeField] Sprite[] sprites;
    [SerializeField] CanvasGroup[] page;
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
    public void SetUp(bool lfet)
    {
        if (StateManager.instance.ButItem)
        {
            return;
        }
        page[0].alpha = lfet ? 1 : 0;
        page[0].blocksRaycasts = lfet ? true : false;
        buttonImages[0].sprite = lfet ? sprites[0] : sprites[1];



        page[1].alpha = lfet ? 0: 1;
        page[1].blocksRaycasts = lfet ? false : true;
        buttonImages[1].sprite = lfet ? sprites[1] : sprites[0];
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
