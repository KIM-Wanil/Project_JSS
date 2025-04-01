using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public int floorIndex; // 현재 층 인덱스
    public int furnitureNum; // 현재 얻은 가구
    public List<FurnitureInfo> availableFurniturePrefabs; // 이 층에서만 사용 가능한 가구 프리팹
    public Transform furnitureParent; // 가구들을 담을 부모 객체

    private IsometricGrid grid;
    private Dictionary<Vector2Int, FurnitureInfo> placedFurniture = new Dictionary<Vector2Int, FurnitureInfo>();

    void Awake()
    {
        grid = GetComponent<IsometricGrid>();
    }
    private void Start()
    {
        //for(int i =0; i< furnitureNum; i++)
        //{
        //    availableFurniturePrefabs[i].transform.position = grid.GridPositionToWorld(availableFurniturePrefabs[i].GridPosition);
        //    placedFurniture.Add(availableFurniturePrefabs[i].GridPosition, availableFurniturePrefabs[i]);
           
        //}
        //foreach (var info in placedFurniture)
        //{
        //    grid.OccupiedCell(info.Key,info.Value.Size,true);
        //}
      
    }
    // 가구 배치 가능 여부 체크
    public bool CanPlaceFurniture(FurnitureInfo furniturePrefab, Vector2Int gridPosition)
    {
        Vector2Int size = furniturePrefab.Size;
        for (int x =0; x < size.x;x++)
        {
            for (int y = 0;y< size.y; y++)
            {
                if (!grid.CanPlaceFurniture(gridPosition))
                {
                    return false;
                }
            }
        }
        return true;
    }

    // 가구 배치하기
    public void PlaceFurniture(FurnitureInfo furniturePrefab, Vector2Int gridPosition)
    {
        if (CanPlaceFurniture(furniturePrefab,gridPosition))
        {
            Vector3 worldPos = grid.GridPositionToWorld(gridPosition);
            GameObject furniture = Instantiate(furniturePrefab.gameObject, furnitureParent);
            furniture.transform.position = new Vector3(worldPos.x, worldPos.y, worldPos.y);

            // 가구 정렬 순서 설정
            SpriteRenderer renderer = furniture.GetComponent<SpriteRenderer>();
            renderer.sortingLayerName = "Floor" + floorIndex;
            renderer.sortingOrder = gridPosition.x + gridPosition.y;

            // 배치된 가구 등록
            //placedFurniture[gridPosition] = furniture;
        }
    }

    // 가구 제거하기
    public void RemoveFurniture(Vector2Int gridPosition)
    {
        if (placedFurniture.ContainsKey(gridPosition))
        {
            Destroy(placedFurniture[gridPosition]);
            placedFurniture.Remove(gridPosition);
        }
    }

    // 특정 가구 접근 가능 위치 찾기
    public Vector2Int GetAccessPosition(Vector2Int furniturePosition)
    {
        // 가구 주변 위치 체크
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (var dir in directions)
        {
            Vector2Int checkPos = furniturePosition + dir;
            //if (grid.IsValidPosition(checkPos) && !placedFurniture.ContainsKey(checkPos))
            //{
            //    return checkPos;
            //}
        }

        // 접근 불가능하면 원래 위치 반환
        return furniturePosition;
    }
}