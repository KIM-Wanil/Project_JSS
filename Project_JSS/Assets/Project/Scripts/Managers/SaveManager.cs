using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
[System.Serializable]
public class SaveData
{
    public List<ItemData> items = new List<ItemData>();
    public List<GeneratorSO> generators = new List<GeneratorSO>();
    public List<ItemKey> rewardList = new List<ItemKey>();
    public int energy;
    public int gold;
    public int gem;
    public string playerName;

    [System.Serializable]
    public class ItemData
    {
        public string itemId;
        public int level;
        public ItemType type;
        public Vector2Int position;
        public ItemState state;
    }
}

// SaveManager.cs
public class SaveManager : BaseManager
{
    public bool isClear = false;
    private const string SAVE_KEY = "MergeGameSave";
    private const int A = 0; 
    private const int B = 1; 
    private const int C = 2; 
    private const int D = 3; 
    private const int E = 4; 
    private const int F = 5; 
    private const int G = 6; 
    private const int H = 7;

    public void Update()
    {
       if(Input.GetKeyDown(KeyCode.F5))
        {
            ClearPlayerPrefs();
            isClear = true;
        }
    }
    
    public void SaveGame(SaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public SaveData LoadGame()
    {
        return CreateInitialSaveData();

        //if (PlayerPrefs.HasKey(SAVE_KEY))
        //{
        //    string json = PlayerPrefs.GetString(SAVE_KEY);
        //    return JsonUtility.FromJson<SaveData>(json);
        //}
        //else
        //{
        //    // 저장된 데이터가 없을 경우 기본 데이터 생성
        //    return CreateInitialSaveData();
        //}
    }

    private SaveData CreateInitialSaveData()
    {
        SaveData initialData = new SaveData
        {
            energy = 100, // 초기 에너지 값 설정
            gold = 0,    // 초기 점수 값 설정
            gem = 777,    // 초기 점수 값 설정
            playerName = "Player" // 초기 플레이어 이름 설정
        };

        //// 초기 박스 설정
        ////가장 외곽
        //initialData.items.Add(SetItemData(new Vector2Int(A, 0), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(A, 1), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(A, 2), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(A, 3), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(A, 4), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(A, 5), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(A, 6), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(A, 7), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(A, 8), "N001", 1, ItemType.Normal, ItemState.InBox));

        //initialData.items.Add(SetItemData(new Vector2Int(B, 0), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(C, 0), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(D, 0), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(E, 0), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(F, 0), "N001", 1, ItemType.Normal, ItemState.InBox));

        //initialData.items.Add(SetItemData(new Vector2Int(G, 0), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(G, 1), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(G, 2), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(G, 3), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(G, 4), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(G, 5), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(G, 6), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(G, 7), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(G, 8), "N001", 1, ItemType.Normal, ItemState.InBox));

        //initialData.items.Add(SetItemData(new Vector2Int(B, 8), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(C, 8), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(D, 8), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(E, 8), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(F, 8), "N001", 1, ItemType.Normal, ItemState.InBox));

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////두번째 외곽
        //initialData.items.Add(SetItemData(new Vector2Int(B, 1), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(B, 2), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(B, 4), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(B, 5), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(B, 6), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(B, 7), "N001", 1, ItemType.Normal, ItemState.InBox));

        //initialData.items.Add(SetItemData(new Vector2Int(D, 1), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(E, 1), "N001", 1, ItemType.Normal, ItemState.InBox));

        //initialData.items.Add(SetItemData(new Vector2Int(F, 1), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(F, 2), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(F, 3), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(F, 4), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(F, 6), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(F, 7), "N001", 1, ItemType.Normal, ItemState.InBox));

        //initialData.items.Add(SetItemData(new Vector2Int(C, 7), "N001", 1, ItemType.Normal, ItemState.InBox));
        //initialData.items.Add(SetItemData(new Vector2Int(D, 7), "N001", 1, ItemType.Normal, ItemState.InBox));



        ////잠긴 아이템 설정 
        //initialData.items.Add(SetItemData(new Vector2Int(B, 3), "N002", 2, ItemType.Normal, ItemState.Locked));
        //initialData.items.Add(SetItemData(new Vector2Int(F, 5), "N002", 3, ItemType.Normal, ItemState.Locked));

        //initialData.items.Add(SetItemData(new Vector2Int(C, 1), "N001", 3, ItemType.Normal, ItemState.Locked));
        //initialData.items.Add(SetItemData(new Vector2Int(E, 7), "N001", 2, ItemType.Normal, ItemState.Locked));

        ////제러레이터 설정
        //initialData.items.Add(SetItemData(new Vector2Int(C, 2), "G001", 1, ItemType.Generatable, ItemState.Normal));
        //initialData.items.Add(SetItemData(new Vector2Int(D, 2), "G002", 1, ItemType.Generatable, ItemState.Normal));

        initialData.items.Add(SetItemData(new Vector2Int(A, 0), "G001", 1, ItemType.Generatable, ItemState.Normal));
        initialData.items.Add(SetItemData(new Vector2Int(B, 0), "G002", 1, ItemType.Generatable, ItemState.Normal));

        initialData.rewardList.Add(new ItemKey("G001", 1));
        initialData.rewardList.Add(new ItemKey("G002", 1));

        return initialData;
    }
    public SaveData.ItemData SetItemData(Vector2Int inputPos, string inputId, int inputLv, ItemType inputType, ItemState inputState)
    {
        SaveData.ItemData itemData = new SaveData.ItemData
        {
            position = inputPos,
            itemId = inputId,
            level = inputLv,
            type = inputType,
            state = inputState
        };
        return itemData;
    }
    // PlayerPrefs 초기화 함수 추가
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs has been cleared.");
    }

    // 자동 저장
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !isClear)
        {
            SaveGame(LoadGame());
        }
    }

    private void OnApplicationQuit()
    {
        if (!isClear)
        { SaveGame(LoadGame()); }
    }
}


#if UNITY_EDITOR


public class SaveManagerEditor
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    public static void ClearPlayerPrefs()
    {
        SaveManager saveManager = Managers.Save;
        if (saveManager != null)
        {
            saveManager.ClearPlayerPrefs();
        }
        else
        {
            Debug.LogError("SaveManager instance not found in the scene.");
        }
    }
}
#endif