// GeneratorData.cs - 제너레이터 설정을 위한 ScriptableObject
using UnityEngine;

[CreateAssetMenu(fileName = "GeneratorData", menuName = "MergeGame/Generator Data")]
public class GeneratorData : ScriptableObject
{
    public string generatorId;
    public int maxDurability = 10;
    public int generatorLevel = 1;
    [System.Serializable]
    public struct GeneratableItem
    {
        public string itemPrefabId;
        public float spawnChance;
        public int itemLevel;
    }
    public GeneratableItem[] generateableItems;
    public int energyCost = 1;
}
