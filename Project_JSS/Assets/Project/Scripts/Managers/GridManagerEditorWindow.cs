#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;
using System.Collections.Generic;

public class GridManagerEditorWindow : EditorWindow
{
    private GridManager gridManager;

    [MenuItem("Window/GridManager Editor")]
    public static void ShowWindow()
    {
        GetWindow<GridManagerEditorWindow>("GridManager Editor");
    }

    private void OnGUI()
    {

        GUILayout.Label("Owned Normal Items:", EditorStyles.boldLabel);
        foreach (var item in Managers.Grid.ownedNormalItems)
        {
            GUILayout.Label($"¿Ã∏ß: {Managers.Game.GetItemName(item.Key)}");
            foreach (var position in item.Value)
            {
                GUILayout.Label($"  Position: {position}");
            }
        }
    }
}

#endif