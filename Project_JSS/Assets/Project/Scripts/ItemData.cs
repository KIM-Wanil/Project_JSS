using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "MergeGame/ItemData")]
public class ItemData : ScriptableObject
{
    public string id;
    [System.Serializable]
    public struct ItemInfo
    {
        public Sprite itemSprite;
        public int level;
        public string itemName;
    }
    public ItemInfo[] items;

}
