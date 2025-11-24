using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class FloorManager : MonoBehaviour
{
    public FloorData floorData;
    public int floorIndex; // 현재 층 인덱스
    public int furnitureNum; // 현재 얻은 가구
    public List<GameObject> availableFurniturePrefabs; // 이 층에서만 사용 가능한 가구 프리팹
    public IsoGird[] grids;
    public GameObject furniturePrefab;

    IsoGird currentGrid;


    [SerializeField] private float fallDistance = 2.0f;
    [SerializeField] private float fallDuration = 0.5f;
    public bool isDrop;
    void Start()
    {
        if (isDrop)
        {
           
        }
        else
            SettingFloor();
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.D))
        {
            StartCoroutine(DopSetting());
        }
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
    IEnumerator DopSetting()
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
                Vector3 originalPosition = new Vector3();
                originalPosition = newObj.transform.position;
                newObj.GetComponent<FurnitureInfo>().SettingData(data);
                newObj.GetComponent<IsoSpriteSorting>().SorterPositionOffset = data.SorterPositionOffset;
                newObj.GetComponent<IsoSpriteSorting>().SorterPositionOffset2 = data.SorterPositionOffset2;
                if (!data.isFloor && !data.isLeft)
                    newObj.transform.localScale = new Vector3(-1, 1, 1);

                if (data.isFloor)
                {

                    Vector3 startPosition = originalPosition + Vector3.up * fallDistance;
                    newObj.transform.position = startPosition;

                    newObj.transform.DOMove(originalPosition, fallDuration)
                        .SetEase(Ease.OutBounce);
                }

            }
            yield return new WaitForSeconds(0.1f);
        }
    }

}