using log4net.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CSVParser : EditorWindow
{
    public TextAsset itemCsvFile;
    private const string itemAdressablePath = "Assets/Project/ScriptableObjects/Item/";
    private const string itemSpritePath = "Assets/Project/Sprites/Item/";
    public TextAsset generatorCsvFile;
    private const string generatorAdressablePath = "Assets/Project/ScriptableObjects/Generator/";
    const int A = 0, B = 1, C = 2, D = 3, E = 4,
              F = 5, G = 6, H = 7, I = 8, J = 9,
              K = 10, L = 11, M = 12, N = 13, O = 14;

    [MenuItem("Tools/CSV Parsing Tools")]
    public static void ShowWindow()
    {
        EditorWindow wnd = GetWindow<CSVParser>();
        wnd.titleContent = new GUIContent("CSV Parsing Tools");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("CSV File", EditorStyles.boldLabel);
        EditorGUILayout.Space(10f);

        itemCsvFile = EditorGUILayout.ObjectField("Item CSV", itemCsvFile, typeof(TextAsset), false) as TextAsset;
        generatorCsvFile = EditorGUILayout.ObjectField("Generator CSV", generatorCsvFile, typeof(TextAsset), false) as TextAsset;

        EditorGUILayout.Space(20f);
        EditorGUILayout.LabelField("Parsing", EditorStyles.boldLabel);

        EditorGUILayout.Space(10f);
        if (GUILayout.Button("Parse Item CSV"))
        {
            ParseItemCSV();
        }

        EditorGUILayout.Space(10f);
        if (GUILayout.Button("Parse Generator CSV"))
        {
            ParseGeneratorCSV();
        }
    }

    private void ParseItemCSV()
    {
        if (itemCsvFile == null)
        {
            Debug.LogError("Item CSV file is not assigned.");
            return;
        }

        string[] itemLines = itemCsvFile.text.Split('\n');
        List<ItemSO> itemSOs = ParseItems(itemLines);

        foreach (var itemSO in itemSOs)
        {
            string assetPath = AssetDatabase.GetAssetPath(itemSO);
            string addressableName = $"Items/{Path.GetFileNameWithoutExtension(assetPath)}";
            SetAsAddressable(assetPath, addressableName);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void ParseGeneratorCSV()
    {
        if (generatorCsvFile == null)
        {
            Debug.LogError("Generator CSV file is not assigned.");
            return;
        }

        if (string.IsNullOrEmpty(generatorCsvFile.text))
        {
            Debug.LogError("Generator CSV file is empty or null.");
            return;
        }

        string[] generatorLines = generatorCsvFile.text.Split('\n');
        List<GeneratorSO> generatorSOs = ParseGenerators(generatorLines);
        Debug.Log(generatorSOs.Count);

        foreach (var generatorSO in generatorSOs)
        {
            string assetPath = AssetDatabase.GetAssetPath(generatorSO);
            string addressableName = $"Generators/{Path.GetFileNameWithoutExtension(assetPath)}";
            SetAsAddressable(assetPath, addressableName);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static List<ItemSO> ParseItems(string[] lines)
    {
        List<ItemSO> itemSOs = new List<ItemSO>();
        Dictionary<string, ItemSO> itemDictionary = new Dictionary<string, ItemSO>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] row = lines[i].Split(',');
            if (row.Length < 7)
            {
                continue;
            }

            string id = row[A];

            string[] bubbleChanceArray;
            float[] tempBubbleChance = new float[3];

            string[] adChanceArray;
            float[] tempAdChance = new float[3];

            if (string.IsNullOrEmpty(row[H]))
            {
                bubbleChanceArray = null;
            }
            else
            {
                bubbleChanceArray = row[H].Split('/');
                if (bubbleChanceArray.Length != 3)
                {
                    Debug.LogError($"{bubbleChanceArray.Length}Invalid bubble chance format in line {i + 1}");
                    continue;
                }

                
                if (string.IsNullOrEmpty(row[I]))
                {
                    adChanceArray = null;
                }
                else
                {
                    adChanceArray = row[I].Split('/');
                    if (adChanceArray.Length != 3)
                    {
                        Debug.LogError($"Invalid ad chance format in line {i + 1}");
                        continue;
                    }
                }
                for (int level = 0; level < 3; level++)
                {
                    if (float.TryParse(bubbleChanceArray[level], out float bubbleChance) && bubbleChance >= 0)
                    {
                        tempBubbleChance[level] = bubbleChance * 0.01f;
                    }
                    if (float.TryParse(adChanceArray[level], out float adChance) && adChance >= 0)
                    {
                        tempAdChance[level] = adChance * 0.01f;
                    }
                }
            }
            

            
          

            
            ItemDetails itemDetails = new ItemDetails
            {
                level = int.Parse(row[C]),
                itemName = row[D],
                itemDesc = row[E],
                price = string.IsNullOrEmpty(row[F]) ? -1 : int.Parse(row[F]),
                itemSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{itemSpritePath}/{row[G]}.png"),

                bubbleChance = tempBubbleChance.ToArray(),
                adChance = tempAdChance.ToArray(),
                //bubbleChance = string.IsNullOrEmpty(row[H]) ? 0 : float.Parse(row[H]) * 0.01f,
                //adChance = string.IsNullOrEmpty(row[I]) ? 0 : float.Parse(row[I]) * 0.01f,
                bubbleCost = string.IsNullOrEmpty(row[J]) ? 0 : int.Parse(row[J]),
                bubbleTime = string.IsNullOrEmpty(row[K]) ? 0 : float.Parse(row[K]),
            };

            if (itemDictionary.ContainsKey(id))
            {
                // 기존 ItemSO에 ItemDetails 추가
                List<ItemDetails> existingDetails = new List<ItemDetails>(itemDictionary[id].items);
                existingDetails.Add(itemDetails);
                itemDictionary[id].items = existingDetails.ToArray();
            }
            else
            {
                // 새로운 ItemSO 생성
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();
                itemSO.id = id;
                itemSO.type = (ItemType)Enum.Parse(typeof(ItemType), row[B]);
                itemSO.items = new ItemDetails[] { itemDetails };
                itemDictionary[id] = itemSO;
                itemSOs.Add(itemSO);
            }
        }

        // 모든 ItemSO를 저장
        foreach (var itemSO in itemSOs)
        {
            SaveScriptableObject(itemSO, $"{itemAdressablePath}{itemSO.id}.asset");
        }

        return itemSOs;
    }

    private static List<GeneratorSO> ParseGenerators(string[] lines)
    {
        List<GeneratorSO> generatorSOs = new List<GeneratorSO>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] row = lines[i].Split(',');
            if (row.Length < 7)
            {
                Debug.LogError($"Line {i + 1} does not have enough columns.");
                continue;
            }

            GeneratorSO generatorSO = ScriptableObject.CreateInstance<GeneratorSO>();
            generatorSO.genId = row[A];
            generatorSO.generatorDatas = new GeneratorData[]
            {
                    new GeneratorData
                    {
                        level = int.Parse(row[B]),
                        maxDurability = int.Parse(row[C]),
                        generatableItems = GetGeneratableItems(row)
                    }
            };
            generatorSOs.Add(generatorSO);
            SaveScriptableObject(generatorSO, $"Assets/Project/ScriptableObjects/Generators/{generatorSO.genId}.asset");
        }

        return generatorSOs;
    }

    private static GeneratableItem[] GetGeneratableItems(string[] values)
    {
        List<GeneratableItem> items = new List<GeneratableItem>();

        AddGeneratableItems(items, values[D], values[E]);
        AddGeneratableItems(items, values[F], values[G]);

        return items.ToArray();
    }

    private static void AddGeneratableItems(List<GeneratableItem> items, string itemId, string chances)
    {
        if (string.IsNullOrEmpty(itemId) || string.IsNullOrEmpty(chances))
            return;

        string[] chanceArray = chances.Split('/');
        for (int level = 0; level < chanceArray.Length; level++)
        {
            if (float.TryParse(chanceArray[level], out float chance) && chance > 0)
            {
                items.Add(new GeneratableItem
                {
                    key = new ItemKey(itemId, level + 1),
                    spawnChance = chance * 0.01f
                });
            }
        }
    }

    private static void SaveScriptableObject(ScriptableObject so, string path)
    {
        if (so == null)
        {
            Debug.LogError("ScriptableObject is null.");
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        AssetDatabase.CreateAsset(so, path);
    }

    private void SetAsAddressable(string assetPath, string address)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Asset Settings not found.");
            return;
        }

        var groupName = "GeneratedAssets";
        var group = settings.FindGroup(groupName);
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
        }

        var guid = AssetDatabase.AssetPathToGUID(assetPath);
        var entry = settings.CreateOrMoveEntry(guid, group);

        entry.address = address;

        AssetDatabase.SaveAssets();
    }
}
