using System;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFloorData", menuName = "Scriptable Objects/FloorData")]
public class FloorData : ScriptableObject
{
    public int floorNum;
    public bool isUnlock;

    public FurnitureData[] furnitureInfos;

    public string floorNmae;
    public string floorDescription;

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
}
[Serializable]
public class FurnitureData
{
    public int floorNum;
    public string furnitureName; // 가구 이름
    public string furnitureDescription; // 가구 설명
    public Vector2Int gridPosition; // 가구 위치
    public Vector2Int[] size; // 가구 크기
    public Vector2Int[] tartgetPosition; //특수 가구
    public int rotation; // 회전 각도
    public Sprites[] furnitureSprite; // 가구 스프라이트
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