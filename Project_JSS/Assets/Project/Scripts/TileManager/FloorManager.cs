using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public FloorData floorData;
    public int floorIndex; // 현재 층 인덱스
    public int furnitureNum; // 현재 얻은 가구
    public List<GameObject> availableFurniturePrefabs; // 이 층에서만 사용 가능한 가구 프리팹
    public IsoGird[] grids;
    public GameObject furniturePrefab;

    IsoGird currentGrid;
    void Start()
    {
        SettingFloor();
    }
    public GameObject AddFurniture(FurnitureData info)
    {
        foreach (FurnitureData data in floorData.furnitureInfos)
        {
            if (info.furnitureName == data.furnitureName)
            {
                data.isUnlocked = true;
                if (data.isFloor)
                {
                    currentGrid = grids[0];
                }
                else
                {
                    if (!data.isLeft)
                    {
                        currentGrid = grids[1];
                    }
                    else
                    {
                        currentGrid = grids[2];
                    }

                }
                GameObject newObj = Object.Instantiate(furniturePrefab, currentGrid.transform);
                availableFurniturePrefabs.Add(newObj);
                newObj.transform.position = currentGrid.GridPositionToWorld(data.gridPosition);
                newObj.GetComponent<FurnitureInfo>().SettingData(data);
                newObj.GetComponent<IsoSpriteSorting>().SorterPositionOffset = data.SorterPositionOffset;
                newObj.GetComponent<IsoSpriteSorting>().SorterPositionOffset = data.SorterPositionOffset2;
                return newObj;
            }
        }
        return null;
    }
    void SettingFloor()
    {
        foreach (FurnitureData data in floorData.furnitureInfos)
        {
            if (data.isUnlocked)
            {
                if (data.isFloor)
                {
                    currentGrid = grids[0];
                }
                else
                {
                    if (!data.isLeft)
                    {
                        currentGrid = grids[1];
                    }

                    else
                    {
                        currentGrid = grids[2];
                    }

                }
                if (availableFurniturePrefabs.Find(x => x.name == data.furnitureName))
                {
                    continue;
                }
                currentGrid.OccupiedCell(data.gridPosition, data.size[data.rotation], true);

                GameObject newObj = Object.Instantiate(furniturePrefab, currentGrid.transform);
                availableFurniturePrefabs.Add(newObj);
                newObj.transform.position = currentGrid.GridPositionToWorld(data.gridPosition);
                newObj.GetComponent<FurnitureInfo>().SettingData(data);
                newObj.GetComponent<IsoSpriteSorting>().SorterPositionOffset = data.SorterPositionOffset;
                newObj.GetComponent<IsoSpriteSorting>().SorterPositionOffset2 = data.SorterPositionOffset2;
                if (!data.isFloor && !data.isLeft)
                    newObj.transform.localScale = new Vector3(-1, 1, 1);

            }
        }
    }
}