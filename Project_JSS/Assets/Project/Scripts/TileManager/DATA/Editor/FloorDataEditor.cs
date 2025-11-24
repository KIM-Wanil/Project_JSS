using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FloorData))]
public class FloorDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 Inspector 그리기
        DrawDefaultInspector();
        FloorData floorData = (FloorData)target;

        if (floorData.furniture == null)
        {
            EditorGUILayout.HelpBox("Furniture GameObject를 먼저 할당해주세요.", MessageType.Warning);
            return;
        }

        // FurnitureInfo 컴포넌트 확인
        FurnitureInfo furnitureInfo = floorData.furniture.GetComponent<FurnitureInfo>();
        if (furnitureInfo == null)
        {
            EditorGUILayout.HelpBox("Furniture GameObject에 FurnitureInfo 컴포넌트가 없습니다.", MessageType.Error);
            return;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Furniture Component Copy", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("각 FurnitureData에 furniture의 FurnitureInfo 값을 복사할 수 있습니다.", MessageType.Info);

        if (floorData.furnitureInfos == null || floorData.furnitureInfos.Length == 0)
        {
            EditorGUILayout.HelpBox("FurnitureInfos가 비어있습니다.", MessageType.Warning);
            return;
        }

        // 각 FurnitureData마다 버튼 생성
        for (int i = 0; i < floorData.furnitureInfos.Length; i++)
        {
            FurnitureData data = floorData.furnitureInfos[i];

            EditorGUILayout.BeginVertical("box");

            // FurnitureData 정보 표시
            string displayName = string.IsNullOrEmpty(data.furnitureName)
                ? $"Furniture {i}"
                : data.furnitureName;
            EditorGUILayout.LabelField($"[{i}] {displayName}", EditorStyles.boldLabel);

            // 모든 값 복사 버튼
            if (GUILayout.Button("Copy All Values from FurnitureInfo", GUILayout.Height(25)))
            {
                CopyFromFurnitureInfo(furnitureInfo, data);
                EditorUtility.SetDirty(floorData);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        // 전체 복사 버튼
        EditorGUILayout.Space(10);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Copy to All Furniture Data", GUILayout.Height(30)))
        {
            foreach (var data in floorData.furnitureInfos)
            {
                CopyFromFurnitureInfo(furnitureInfo, data);
            }
            EditorUtility.SetDirty(floorData);
            Debug.Log("모든 FurnitureData에 값이 복사되었습니다.");
        }
        GUI.backgroundColor = Color.white;
    }

    private void CopyFromFurnitureInfo(FurnitureInfo furnitureInfo, FurnitureData data)
    {
        // FurnitureInfo의 saveInfo() 메서드를 이용해서 모든 값 복사
        FurnitureData savedData = furnitureInfo.saveInfo();

        // gridPosition 복사
        data.gridPosition = savedData.gridPosition;

        // size 복사
        data.size = savedData.size;

        // colliderOffset, colliderSize 복사
        data.colliderOffset = savedData.colliderOffset;
        data.colliderSize = savedData.colliderSize;

        // SorterPositionOffset, SorterPositionOffset2 복사
        data.SorterPositionOffset = savedData.SorterPositionOffset;
        data.SorterPositionOffset2 = savedData.SorterPositionOffset2;

        Debug.Log($"{data.furnitureName} - 모든 값 복사 완료:\n" +
                  $"GridPosition: {data.gridPosition}\n" +
                  $"Size: [{string.Join(", ", data.size)}]\n" +
                  $"ColliderOffset: {data.colliderOffset}, ColliderSize: {data.colliderSize}\n" +
                  $"SorterOffset: {data.SorterPositionOffset}, SorterOffset2: {data.SorterPositionOffset2}");
    }
}