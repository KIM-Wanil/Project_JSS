using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public abstract class IsoGird : MonoBehaviour
{
    public Vector2 startPosition = Vector2.zero;
    public float tileOffsetX = 32f; // 타일 X 오프셋 (픽셀)
    public float tileOffsetY = 16f; // 타일 Y 오프셋 (픽셀)
    public float floorHeight;
    public int floorIndex;
    private bool[,] occupiedCells; // 점유된 셀 추적
                                   // 그리드 좌표를 화면 좌표로 변환
    public int gridWidth = 12;
    public int gridHeight = 12;
    public Color gridLineColor = Color.red;

    Vector2Int furnitureSize;
    [SerializeField] GameObject tile;
    List<GameObject> tiles;
    Queue<GameObject> tilesQueue;
    public void Awake()
    {
        InitializeGrid(gridWidth, gridHeight);
        tiles = new List<GameObject>();
        tilesQueue = new Queue<GameObject>();
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
    public void Refresh(Vector2Int size, Vector2Int gridPosition)
    {
        furnitureSize = size;
        int num = 0;
        for (int x = 0; x < furnitureSize.x; x++)
        {
            for (int y = 0; y < furnitureSize.y; y++)
            {
                Vector3 vector = GridPositionToWorld(new Vector2Int(gridPosition.x + x, gridPosition.y + y));
                tiles[num].transform.position = vector;
                num++;
            }
        }
    }
    public void TileSetting(Transform furniture, Vector2Int size,Vector2Int gridPosition)
    {
        furnitureSize = size;
        for (int x = 0; x < furnitureSize.x; x++)
        {
            for (int y = 0; y < furnitureSize.y; y++)
            {
                Vector3 vector = GridPositionToWorld(new Vector2Int( gridPosition.x + x, gridPosition.y + y));
                GameObject obj;
                if (!tilesQueue.TryDequeue(out obj))
                    obj = Instantiate(tile, vector, furniture.rotation, furniture);
                else
                {
                    obj.transform.SetParent(furniture);
                    obj.transform.position = vector;
                    obj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    obj.SetActive(true);
                }
                tiles.Add(obj);
            }
        }
    }
    public bool CanPlaceFurniture(Vector2Int pos)
    {
        bool IsCan = true;
        int num = 0;
        for (int x = 0; x < furnitureSize.x; x++)
        {
            for (int y = 0; y < furnitureSize.y; y++)
            {
                if (!CanPlaceTile(new Vector2Int(pos.x + x, pos.y + y)))
                {
                    Debug.Log("Can not Place");
                    tiles[num].GetComponent<SpriteRenderer>().color = Color.red;
                    IsCan = false;
                }
                else
                {
                    tiles[num].GetComponent<SpriteRenderer>().color = Color.green;
                }
                num++;
            }
        }
        return IsCan;
    }
    public bool CanPlaceTile(Vector2Int pos)
    {
        // 그리드 범위 체크
        if (pos.x < 0 || pos.y < 0 ||
            pos.x >= occupiedCells.GetLength(0) ||
            pos.y >= occupiedCells.GetLength(1))
            return false;

        // 이미 점유된 셀인지 체크
        return !occupiedCells[pos.x, pos.y];
    }
    public void FreeViewOff()
    {
        foreach (GameObject gameObject in tiles)
        {
            gameObject.gameObject.SetActive(false);
            tilesQueue.Enqueue(gameObject);
        }
        tiles.Clear();
    }
    public void OccupiedCell(Vector3 worldPosition, Vector2Int size, bool occupied)
    {
        Vector2Int pos = WorldToGridPosition(worldPosition);
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Debug.Log(new Vector2(pos.x + x, pos.y + y));
                if (pos.x + x >= gridWidth || pos.y + y >= gridHeight)
                    continue;
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
    public abstract Vector2Int WorldToGridPosition(Vector3 worldPosition, int heightLimit = 0);
    public abstract Vector2Int WorldToGridPosition(Vector3 worldPosition, Vector2Int size, int heightLimit = 0);
    public abstract Vector3 GridPositionToWorld(Vector2Int gridPosition);

    public Vector3 SortGrid(Vector3 worldPosition)
    {
        Debug.Log(WorldToGridPosition(worldPosition));
        return GridPositionToWorld(WorldToGridPosition(worldPosition));
    }
}
