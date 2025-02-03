// Generator.cs - 아이템을 생성하는 제너레이터
using UnityEngine;

public class Generator : MergeableItem
{
    [SerializeField] private GeneratorData data;
    private int currentDurability;

    private void Start()
    {
        itemId = data.generatorId;
        level = data.generatorLevel;
        currentDurability = data.maxDurability;
    }

    public bool TryGenerateItem(GameManager gameManager)
    {
        if (currentDurability <= 0 || !gameManager.TrySpendEnergy(data.energyCost))
            return false;

        currentDurability--;

        float randomValue = Random.value;
        float accumulatedChance = 0;

        foreach (var item in data.generateableItems)
        {
            accumulatedChance += item.spawnChance;
            if (randomValue <= accumulatedChance)
            {
                gameManager.SpawnItem(item.itemPrefabId, item.itemLevel, gridPosition);
                break;
            }
        }

        return true;
    }
}