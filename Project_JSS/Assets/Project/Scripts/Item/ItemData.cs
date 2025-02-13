using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "MergeGame/ItemData")]
public class ItemData : ScriptableObject
{
    public string id;
    public ItemType type;
    public ItemDetails[] items;

}
