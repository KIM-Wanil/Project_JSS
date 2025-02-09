using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct Item
{
    public string id;
    public int lv;
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
