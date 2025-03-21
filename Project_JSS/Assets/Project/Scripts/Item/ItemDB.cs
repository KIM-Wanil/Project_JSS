using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ItemDB
{
    private Dictionary<string, ItemSO> loadedItems = new Dictionary<string, ItemSO>();
    private Dictionary<string, AsyncOperationHandle<ItemSO>> loadOperations = new Dictionary<string, AsyncOperationHandle<ItemSO>>();

    public async Task<ItemSO> LoadItem(string itemId)
    {
        if (loadedItems.TryGetValue(itemId, out ItemSO item))
        {
            return item;
        }

        if (loadOperations.TryGetValue(itemId, out AsyncOperationHandle<ItemSO> existingOperation))
        {
            await existingOperation.Task;
            return existingOperation.Result;
        }

        var operation = Addressables.LoadAssetAsync<ItemSO>($"Items/{itemId}");
        loadOperations[itemId] = operation;

        await operation.Task;

        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            loadedItems[itemId] = operation.Result;
            loadOperations.Remove(itemId);
            return operation.Result;
        }
        else
        {
            Debug.LogError($"Failed to load item: {itemId}");
            loadOperations.Remove(itemId);
            return null;
        }
    }

    public void ReleaseItem(string itemId)
    {
        if (loadedItems.TryGetValue(itemId, out ItemSO item))
        {
            Addressables.Release(item);
            loadedItems.Remove(itemId);
        }
    }

    public void ReleaseAllItems()
    {
        foreach (var item in loadedItems.Values)
        {
            Addressables.Release(item);
        }
        loadedItems.Clear();
    }

    private void OnDisable()
    {
        ReleaseAllItems();
    }
}
