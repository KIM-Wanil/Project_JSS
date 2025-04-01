using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GeneratorDB
{
    private Dictionary<string, GeneratorSO> loadedGenerators = new Dictionary<string, GeneratorSO>();
    private Dictionary<string, AsyncOperationHandle<GeneratorSO>> loadOperations = new Dictionary<string, AsyncOperationHandle<GeneratorSO>>();

    public async Task<GeneratorSO> LoadGenerator(string generatorId)
    {
        if (loadedGenerators.TryGetValue(generatorId, out GeneratorSO generator))
        {
            return generator;
        }

        if (loadOperations.TryGetValue(generatorId, out AsyncOperationHandle<GeneratorSO> existingOperation))
        {
            await existingOperation.Task;
            return existingOperation.Result;
        }

        var operation = Addressables.LoadAssetAsync<GeneratorSO>($"Generators/{generatorId}");
        loadOperations[generatorId] = operation;

        await operation.Task;

        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            loadedGenerators[generatorId] = operation.Result;
            loadOperations.Remove(generatorId);
            return operation.Result;
        }
        else
        {
            Debug.LogError($"Failed to load generator: {generatorId}");
            loadOperations.Remove(generatorId);
            return null;
        }
    }

    public void ReleaseGenerator(string generatorId)
    {
        if (loadedGenerators.TryGetValue(generatorId, out GeneratorSO generator))
        {
            Addressables.Release(generator);
            loadedGenerators.Remove(generatorId);
        }
    }

    public void ReleaseAllGenerators()
    {
        foreach (var generator in loadedGenerators.Values)
        {
            Addressables.Release(generator);
        }
        loadedGenerators.Clear();
    }

    private void OnDisable()
    {
        ReleaseAllGenerators();
    }
}