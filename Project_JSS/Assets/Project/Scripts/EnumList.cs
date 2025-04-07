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
}
[System.Serializable]
public struct ItemKey
{
    public ItemKey(string id, int lv)
    {
        this.id = id.ToString();
        this.Lv = lv;
    }
    public string id;
    public int Lv;
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
    InBox
}
public enum GoodsType
{
    None = 0,
    Gold,
    Energy,
    Gem
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
public class EnumList : MonoBehaviour
{

}
