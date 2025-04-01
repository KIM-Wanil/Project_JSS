using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "MergeGame/ItemSO")]
public class ItemSO : ScriptableObject
{
    public string id;
    public ItemType type;
    public ItemDetails[] items;

}
