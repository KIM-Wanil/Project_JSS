using System;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFloorData", menuName = "Scriptable Objects/FloorData")]
public class FloorData : ScriptableObject
{
    public string floorName;
    public string floorDescription;
    public string floorBGM;
    public int floorNum;
    public bool isUnlock;
    public Sprite mainSprite;
    public FurnitureData[] furnitureInfos;
    public GameObject furniture;
    public int UnlockCounting()
    { 
        int count = 0;
        foreach (FurnitureData data in furnitureInfos)
        {
            if (data.isUnlocked)
            {
                count++;
            }
        }
        return count;
    }
    public void Copy(FloorData floorData)
    {
        floorName = floorData.floorName;
        floorDescription = floorData.floorDescription;
        floorBGM = floorData.floorBGM;
        floorNum = floorData.floorNum;
        isUnlock = floorData.isUnlock;
        mainSprite = floorData.mainSprite;
        furnitureInfos = floorData.furnitureInfos;
        furniture = floorData.furniture;
    }
}
[Serializable]
public class FurnitureData
{
    public string furnitureName; // 가구 이름
    public string furnitureDescription; // 가구 설명
    public int floorNum;
    public Vector2Int gridPosition; // 가구 위치
    public Vector2Int[] size; // 가구 크기
    public Vector2Int[] tartgetPosition; //특수 가구
    public int rotation; // 회전 각도
    public Sprites[] furnitureSprite; // 가구 스프라이트

    public int heightLimit;
    public int spriteNumber; // 가구 스프라이트 번호
    public bool isFloor; // 바닥용 가구
    public bool isLeft; // 바닥용 가구

    public Vector2 SorterPositionOffset;
    public Vector2 SorterPositionOffset2;

    public Vector2 colliderOffset;
    public Vector2 colliderSize;

    public bool isUnlocked; // 가구 해금 상태
    public int price;
}