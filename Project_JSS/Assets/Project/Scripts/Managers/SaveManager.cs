// SaveData.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<ItemData> items = new List<ItemData>();
    public List<GeneratorData> generators = new List<GeneratorData>();
    public float energy;
    public int score;
    public string playerName;

    [System.Serializable]
    public class ItemData
    {
        public string itemId;
        public int level;
        public Vector2Int position;
    }
}

// SaveManager.cs
public class SaveManager : BaseManager
{
    private const string SAVE_KEY = "MergeGameSave";

    public void SaveGame(SaveData saveData)
    {
        //SaveData saveData = new SaveData();
        // 현재 게임 상태를 SaveData에 저장

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public SaveData LoadGame()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return new SaveData();
    }

    // 자동 저장
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame(LoadGame());
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame(LoadGame());
    }
}