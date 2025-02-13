// GeneratorData.cs - 제너레이터 설정을 위한 ScriptableObject
using UnityEngine;

[CreateAssetMenu(fileName = "GeneratorData", menuName = "MergeGame/Generator Data")]
public class GeneratorDB : ScriptableObject
{
    public string genId;
    
    public GeneratorData[] generatorDatas;
    public int energyCost = 1;
}
