using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CraftingRecipe
{
    public ItemKey componentA;
    public ItemKey componentB;
    public ItemKey result;
    
}
[System.Serializable]
public struct GeneratableItem
{
    public ItemKey key;
    public float spawnChance;
}
[System.Serializable]
public struct GeneratorData
{
    public int level;
    public int maxDurability;
    public GeneratableItem[] generatableItems;
}
[System.Serializable]
public struct ItemDetails
{
    public Sprite itemSprite;
    public int level;
    public string itemName;
    public string itemDesc;
    public int price;

    public float[] bubbleChance;
    public float[] adChance;
    public int bubbleCost;
    public float bubbleTime;
}
[System.Serializable]
public struct ItemKey
{
    public ItemKey(string id, int lv)
    {
        this.id = id.ToString();
        this.lv = lv;
    }
    public string id;
    public int lv;
}
public enum ItemType
{
    Normal =0,
    Generatable,
    Usable
}
public enum ItemState
{
    Normal = 0,
    Locked,
    InBox,
    BubbleAd,
    BubbleGem
}
public enum GoodsType
{
    None = 0,
    Star,
    Energy,
    Gem,
    Gold

}
public enum SoundType
{
    BGM = 0,
    UI,
    Effect
}

public enum Direction
{
    Up =0,
    Down,
    Left,
    Right
}
public enum FingerAnimationType
{
    None = 0,
    Zoom,
    SwipeX,
    SwipeY
}
public enum TutorialCondition
{
    None = 0,
    머지이동클릭,
    제작목록클릭,
    제작목록닫기버튼클릭,
    펜치합성,
    스패너합성,
    제너레이터클릭,
    망치합성,
    가구설치클릭,
    퀘스트완료클릭,
    가구선택,
    가구드래그,
    가구방향전환

}


public class EnumList : MonoBehaviour
{

}
