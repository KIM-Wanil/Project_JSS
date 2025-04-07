using System.Collections.Generic;
using UnityEngine;


public class IsometricGrid : MonoBehaviour
{
    public float tileOffsetX = 32f; // 타일 X 오프셋 (픽셀)
    public float tileOffsetY = 16f; // 타일 Y 오프셋 (픽셀)
    public int floorIndex;
    private bool[,] occupiedCells; // 점유된 셀 추적
                                   // 그리드 좌표를 화면 좌표로 변환


    public int gridWidth = 12;
    public int gridHeight = 12;
    public Color gridLineColor = Color.red;

    public void Awake()
    {
        InitializeGrid(12, 12);
    }
    public Vector2 GridToScreenPosition(int x, int y)
    {
        float screenX = (x - y) * tileOffsetX;
        float screenY = (x + y) * tileOffsetY;
        return new Vector2(screenX, screenY);
    }
    public void InitializeGrid(int x, int y)
    {
        occupiedCells = new bool[x, y];
    }
    public bool GetTileData(int x,int y)
    {
        return occupiedCells[x, y];
    }
    public bool CanPlaceFurniture(Vector2Int pos)
    {
        // 그리드 범위 체크
        if (pos.x < 0 || pos.y < 0 ||
            pos.x >= occupiedCells.GetLength(0) ||
            pos.y >= occupiedCells.GetLength(1))
            return false;

        // 이미 점유된 셀인지 체크
        return !occupiedCells[pos.x, pos.y];
    }

    public void OccupiedCell(Vector3 worldPosition, Vector2Int size,bool occupied)
    {
        Vector2Int pos = WorldToGridPosition(worldPosition);
        for (int x= 0;x<size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Debug.Log(new Vector2( pos.x+x, pos.y+y));
                occupiedCells[pos.x+x, pos.y+y] = occupied;
            }
        }
    }
    public void OccupiedCell(Vector2Int gridPosition, Vector2Int size, bool occupied)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                occupiedCells[gridPosition.x+x, gridPosition.y+y] = occupied;
            }
        }
    }
    // 화면 좌표를 그리드 좌표로 변환
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector2 vector = new Vector2(worldPosition.x, worldPosition.y);
        int gridX = Mathf.RoundToInt((vector.x / (tileOffsetX * 0.5f) + vector.y / (tileOffsetY * 0.5f))*0.5f);
        int gridY = Mathf.RoundToInt((vector.y / (tileOffsetY * 0.5f) - vector.x / (tileOffsetX * 0.5f)) * 0.5f);
        if (gridX < 0) { gridX = 0; }
        if (gridY < 0) { gridY = 0; }
        if (gridX > 11) { gridX = 11; }
        if (gridY > 11) { gridY = 11; }
        return new Vector2Int(gridX, gridY);
    }
    public Vector3 GridPositionToWorld(Vector2Int gridPosition)
    {
        float worldX = (gridPosition.x - gridPosition.y) * (tileOffsetX*0.5f);
        float worldY = (gridPosition.y + gridPosition.x) * (tileOffsetY * 0.5f);
        return new Vector3(worldX, worldY , worldY);
    }
    public Vector3 SortGrid(Vector3 worldPosition)
    {
        Debug.Log(WorldToGridPosition(worldPosition));
        return GridPositionToWorld(WorldToGridPosition(worldPosition));
    }

    private void OnDrawGizmos()
    {
        

        Gizmos.color = gridLineColor;

        // Draw horizontal grid lines (left-to-right diagonals in isometric view)
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector2 startPos = GridToScreenPosition(0, y);
            Vector2 endPos = GridToScreenPosition(gridWidth, y);
            Gizmos.DrawLine(startPos, endPos);
        }

        // Draw vertical grid lines (right-to-left diagonals in isometric view)
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector2 startPos = GridToScreenPosition(x, 0);
            Vector2 endPos =GridToScreenPosition(x, gridHeight);
            Gizmos.DrawLine(startPos, endPos);
        }
    }
}