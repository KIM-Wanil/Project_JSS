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
    public int price;
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
    Crafted,
    Usable
}
public enum RewardType
{
    None = 0,
    Item,
    Gold,
    Energy,
    Crystal
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
