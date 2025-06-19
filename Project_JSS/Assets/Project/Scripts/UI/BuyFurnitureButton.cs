using UnityEngine;
using UnityEngine.UI;

public class BuyFurnitureButton : MonoBehaviour
{
    BuyFurniture buyFurniture;
    FurniturePlacementManager furniturePlacementManager;
    FurnitureData data;

   [SerializeField] Image image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Buy()
    {

        furniturePlacementManager.AddFurnitureToFloor(data,data.floorNum);
        buyFurniture.SetUI(false);
        StateManager.instance.ButItem = true;
        Destroy(this.gameObject);
    }
    public void Setting(BuyFurniture buyFurniture, FurniturePlacementManager furniturePlacementManager, FurnitureData data)
    {
        this.buyFurniture = buyFurniture;
        this.furniturePlacementManager = furniturePlacementManager;
        this.data = data;
        image.sprite = data.furnitureSprite[0].sprites[0];

    }
}
