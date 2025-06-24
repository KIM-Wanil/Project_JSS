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
    private const string itemAssetPath = "Assets/Project/ScriptableObjects/Item/";
    private const string itemSpritePath = "Assets/Project/Sprites/Item/";
    public TextAsset generatorCsvFile;
    private const string generatorAssetPath = "Assets/Project/ScriptableObjects/Generator/";

    public TextAsset dialogueCsvFile;
    private const string dialogueAssetPath = "Assets/Project/ScriptableObjects/Dialogue/";
    private const string npcSpritePath = "Assets/Project/Sprites/Guest/";

    public TextAsset tutorialCsvFile;
    private const string tutorialAssetPath = "Assets/Project/ScriptableObjects/Tutorial/";
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
        dialogueCsvFile = EditorGUILayout.ObjectField("Dialgoue CSV", dialogueCsvFile, typeof(TextAsset), false) as TextAsset;
        tutorialCsvFile = EditorGUILayout.ObjectField("Tutorial CSV", tutorialCsvFile, typeof(TextAsset), false) as TextAsset;

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

        EditorGUILayout.Space(10f);
        if (GUILayout.Button("Parse Dialogue CSV"))
        {
            ParseDialogueCSV();
        }

        EditorGUILayout.Space(10f);
        if (GUILayout.Button("Parse Tutorial CSV"))
        {
            ParseTutorialCSV();
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
            SaveScriptableObject(itemSO, $"{itemAssetPath}{itemSO.id}.asset");
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

    public void ParseDialogueCSV()
    {
#if UNITY_EDITOR
        if (dialogueCsvFile == null)
        {
            Debug.LogError("CSV 파일이 할당되지 않았습니다.");
            return;
        }

        string[] lines = dialogueCsvFile.text.Split('\n');
        List<DialogueEvent> events = new List<DialogueEvent>();
        DialogueEvent currentEvent = null;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] row = line.Split(',');

            // 새 이벤트 시작
            if (!string.IsNullOrEmpty(row[A]))
            {
                if (currentEvent != null)
                    events.Add(currentEvent);

                currentEvent = new DialogueEvent
                {
                    eventId = row[A],
                    dialogues = new List<DialogueData>()
                };
            }

            if (currentEvent == null) continue;

            DialogueData data = new DialogueData
            {
                dialogueId = int.Parse(row[B]),
                speakerName = row[C],
                dialogueText = row[D].Replace("<c>", ","),
                nextDialogueId = int.Parse(row[E])
            };
            currentEvent.dialogues.Add(data);
        }

        // 마지막 이벤트 추가
        if (currentEvent != null)
            events.Add(currentEvent);

        // ScriptableObject 생성 및 저장
        DialogueDatabase db = ScriptableObject.CreateInstance<DialogueDatabase>();
        db.dialogueEvents = events;
        db.npcSprites = new List<NPCSpriteData>();
        // NPC 스프라이트 로드
        string[] npcSpriteFiles = Directory.GetFiles(npcSpritePath, "*.png");
        foreach (string file in npcSpriteFiles)
        {
            string spriteName = Path.GetFileNameWithoutExtension(file);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(file);
            if (sprite != null)
            {
                db.npcSprites.Add(new NPCSpriteData { spriteName = spriteName, sprite = sprite });
            }
            else
            {
                Debug.LogWarning($"Failed to load sprite: {file}");
            }
        }
        // ScriptableObject 저장
        if (db.dialogueEvents.Count == 0)
        {
            Debug.LogError("No dialogue events found in the CSV file.");
            return;
        }
        if (db.npcSprites.Count == 0)
        {
            Debug.LogWarning("No NPC sprites found. Ensure sprites are in the correct path.");
        }
        // ScriptableObject 저장 경로
        string assetPath = $"{dialogueAssetPath}대화이벤트SO.asset";
        SaveScriptableObject(db, assetPath);

        // Addressable 등록
        SetAsAddressable(assetPath, "DialogueDatabase");

        Debug.Log("Dialogue CSV 파싱 및 저장 완료!");
#endif
    }

    public void ParseTutorialCSV()
    {
#if UNITY_EDITOR
        if (tutorialCsvFile == null)
        {
            Debug.LogError("튜토리얼 CSV 파일이 할당되지 않았습니다.");
            return;
        }

        string[] lines = tutorialCsvFile.text.Split('\n');
        List<TutorialEvent> events = new List<TutorialEvent>();
        TutorialEvent currentEvent = null;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            Debug.Log($"Parsing line {i}: {line}");

            string[] row = line.Split(',');
            // 새 이벤트 시작
            if (!string.IsNullOrEmpty(row[A]))
            {
                if (currentEvent != null)
                    events.Add(currentEvent);

                currentEvent = new TutorialEvent
                {
                    eventId = row[A],
                    dialogues = new List<TutorialStep>()
                };
            }

            if (currentEvent == null) continue;

            TutorialStep step = new TutorialStep();

            // dialogue
            step.dialogue = row[D].Replace("<c>", ",");

            // characterPosition
            step.characterPosition = ParseVector2(row[E]);

            // characterName
            step.characterName = row[C];

            // highlightPosition
            step.highlightPosition = ParseVector2(row[F]);

            // highlightSize
            step.highlightSize = ParseVector2(row[G]);

            // fingerAnimType
            step.fingerAnimType = ParseFingerAnimType(row[H]);

            // fingerPosition
            step.fingerPosition = ParseVector2(row[I]);

            // fingerRotation
            step.fingerRotation = ParseFloat(row[J]);

            // fingerAnimationAmount
            step.fingerAnimationAmount = ParseFloat(row[K]);

            // completionCondition
            step.completionCondition = ParseTutorialCondition(row[L]);

            currentEvent.dialogues.Add(step);
        }

        // 마지막 이벤트 추가
        if (currentEvent != null)
            events.Add(currentEvent);

        // ScriptableObject 생성 및 저장
        TutorialDatabase db = ScriptableObject.CreateInstance<TutorialDatabase>();
        db.tutorialEvents = events;

        string assetPath = $"{tutorialAssetPath}튜토리얼SO.asset";
        SaveScriptableObject(db, assetPath);

        SetAsAddressable(assetPath, "TutorialDatabase");

        Debug.Log("튜토리얼 CSV 파싱 및 저장 완료!");
#endif
    }

    // Vector2 파싱 유틸
    private static Vector2 ParseVector2(string value)
    {
        if (string.IsNullOrEmpty(value)) return Vector2.zero;
        var parts = value.Split('/');
        if (parts.Length != 2) return Vector2.zero;
        float.TryParse(parts[0], out float x);
        float.TryParse(parts[1], out float y);
        return new Vector2(x, y);
    }

    // float 파싱 유틸
    private static float ParseFloat(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0f;
        float.TryParse(value, out float result);
        return result;
    }

    // FingerAnimationType 파싱 유틸
    private static FingerAnimationType ParseFingerAnimType(string value)
    {
        if (string.IsNullOrEmpty(value)) return FingerAnimationType.None;
        if (System.Enum.TryParse<FingerAnimationType>(value, out var result))
            return result;
        return FingerAnimationType.None;
    }
    // FingerAnimationType 파싱 유틸
    private static TutorialCondition ParseTutorialCondition(string value)
    {
        if (string.IsNullOrEmpty(value)) return TutorialCondition.None;
        if (System.Enum.TryParse<TutorialCondition>(value, out var result))
            return result;
        return TutorialCondition.None;
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
