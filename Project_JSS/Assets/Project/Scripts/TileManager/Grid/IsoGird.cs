using UnityEngine;

public abstract class IsoGird : MonoBehaviour
{
    public Vector2 startPosition = Vector2.zero;
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
        InitializeGrid(gridWidth, gridHeight);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void InitializeGrid(int x, int y)
    {
        occupiedCells = new bool[x, y];
    }
    public bool GetTileData(int x, int y)
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
    public void OccupiedCell(Vector3 worldPosition, Vector2Int size, bool occupied)
    {
        Vector2Int pos = WorldToGridPosition(worldPosition);
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Debug.Log(new Vector2(pos.x + x, pos.y + y));
                occupiedCells[pos.x + x, pos.y + y] = occupied;
            }
        }
    }
    public void OccupiedCell(Vector2Int gridPosition, Vector2Int size, bool occupied)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                occupiedCells[gridPosition.x + x, gridPosition.y + y] = occupied;
            }
        }
    }

    public abstract Vector2 GridToScreenPosition(float x, float y);
    public abstract Vector2Int WorldToGridPosition(Vector3 worldPosition);
    public abstract Vector3 GridPositionToWorld(Vector2Int gridPosition);

    public Vector3 SortGrid(Vector3 worldPosition)
    {
        Debug.Log(WorldToGridPosition(worldPosition));
        return GridPositionToWorld(WorldToGridPosition(worldPosition));
    }
}
