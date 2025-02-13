using UnityEngine;

[CreateAssetMenu(fileName = "CraftingItemDB", menuName = "Scriptable Objects/CraftingItemDB")]
public class CraftingDB : ScriptableObject
{
    public CraftingRecipe[] craftingRecipe;

    // �� ���� ������ Ű�� �޾� �ϼ� ������ Ű�� ��ȯ�ϴ� �Լ�
    public ItemKey? FindCraftingResult(ItemKey itemKey1, ItemKey itemKey2)
    {
        foreach (CraftingRecipe recipe in craftingRecipe)
        {
            if ( (IsSameItemKey(recipe.componentA,itemKey1) && IsSameItemKey(recipe.componentB, itemKey2) ||
                 (IsSameItemKey(recipe.componentA, itemKey2) && IsSameItemKey(recipe.componentB, itemKey1)) ))
            {
                return recipe.result;
            }
        }
        return null;
    }
    public bool IsSameItemKey(ItemKey itemKey1, ItemKey itemKey2)
    {
        return itemKey1.id == itemKey2.id && itemKey1.lv == itemKey2.lv;
    }
}
