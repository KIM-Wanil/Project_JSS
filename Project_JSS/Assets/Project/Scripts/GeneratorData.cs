// GeneratorData.cs - 제너레이터 설정을 위한 ScriptableObject
using UnityEngine;

[CreateAssetMenu(fileName = "GeneratorData", menuName = "MergeGame/Generator Data")]
public class GeneratorData : ScriptableObject
{
    public string genId;
    [System.Serializable]
    public struct GeneratableItem
    {
        public Item itemInfo;
        public float spawnChance;
    }
    [System.Serializable]
    public struct GeneratorLevelData
    {
        public int level;
        public int maxDurability;
        public GeneratableItem[] generatableItems;
    }
    public GeneratorLevelData[] generatorLevelDatas;
    public int energyCost = 1;
}
