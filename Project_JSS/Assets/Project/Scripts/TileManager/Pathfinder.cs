using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileData
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public bool IsWalkable { get; set; }

    // A* 알고리즘을 위한 변수들
    public float GCost { get; set; } // 시작점에서의 비용
    public float HCost { get; set; } // 목표까지의 추정 비용
    public float FCost => GCost + HCost;
    public TileData Parent { get; set; }

    public TileData(int x, int y, bool isWalkable)
    {
        X = x;
        Y = y;
        IsWalkable = isWalkable;
    }
}

public class Pathfinder : MonoBehaviour
{
    private IsoGridFloor isometricGrid;
    private TileData[,] tileDatas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isometricGrid = GetComponent<IsoGridFloor>();
        tileDatas = new TileData[12,12];
        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 12; y++)
            {
                tileDatas[x, y] = new TileData(x,y, !isometricGrid.GetTileData(x, y));
            }
        }
    }
    public List<TileData> GetNeighbors(TileData tile)
    {
        List<TileData> neighbors = new List<TileData>();

        // 4방향 이웃 확인 (상하좌우)
        CheckAndAddNeighbor(neighbors, tile.X + 1, tile.Y);
        CheckAndAddNeighbor(neighbors, tile.X - 1, tile.Y);
        CheckAndAddNeighbor(neighbors, tile.X, tile.Y + 1);
        CheckAndAddNeighbor(neighbors, tile.X, tile.Y - 1);

        return neighbors;
    }
    private void CheckAndAddNeighbor(List<TileData> neighbors, int x, int y)
    {
        if (x < 0 || y < 0 ||x >= 12 || y >= 12)
            return;
        if (tileDatas[x, y] != null && tileDatas[x, y].IsWalkable)
        {
            neighbors.Add(tileDatas[x, y]);
        }
    }
    public List<TileData> FindPath(TileData startTile, TileData targetTile)
    {
        if (startTile == null || targetTile == null || !targetTile.IsWalkable)
            return null;
        List<TileData> openSet = new List<TileData>();
        HashSet<TileData> closedSet = new HashSet<TileData>();

        openSet.Add(startTile);
        // 모든 타일의 비용 초기화
        for (int x = 0; x < isometricGrid.gridWidth; x++)
        {
            for (int y = 0; y < isometricGrid.gridHeight; y++)
            {
                if (isometricGrid.GetTileData(x,y))
                    tileDatas[x,y].IsWalkable = false;
                else
                    tileDatas[x, y].IsWalkable = true;

                tileDatas[x, y].GCost = float.MaxValue;
                tileDatas[x, y].Parent = null;
            }
        }

        startTile.GCost = 0;
        startTile.HCost = CalculateHCost(startTile, targetTile);

        while (openSet.Count > 0)
        {
            TileData currentTile = openSet.OrderBy(x => x.FCost).First();

            if (currentTile == targetTile)
            {
                // 경로 찾음
                return ReconstructPath(targetTile);
            }

            openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            foreach (TileData neighbor in GetNeighbors(currentTile))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                float tentativeGCost = currentTile.GCost + CalculateDistance(currentTile, neighbor);

                if (tentativeGCost < neighbor.GCost)
                {
                    neighbor.Parent = currentTile;
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = CalculateHCost(neighbor, targetTile);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        // 경로를 찾지 못함
        return null;
    }

    // 맨해튼 거리를 사용한 휴리스틱 비용 계산
    private float CalculateHCost(TileData from, TileData to)
    {
        return Mathf.Abs(from.X - to.X) + Mathf.Abs(from.Y - to.Y);
    }

    // 타일 간 거리 계산 (직선은 1, 대각선은 1.4)
    private float CalculateDistance(TileData from, TileData to)
    {
        int distX = Mathf.Abs(from.X - to.X);
        int distY = Mathf.Abs(from.Y - to.Y);
        return distY + distX;
    }

    // 경로 재구성
    private List<TileData> ReconstructPath(TileData targetTile)
    {
        List<TileData> path = new List<TileData>();
        TileData currentTile = targetTile;

        while (currentTile != null)
        {
            path.Add(currentTile);
            currentTile = currentTile.Parent;
        }

        path.Reverse();
        return path;
    }

    // 두 월드 위치 사이의 경로 찾기
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        TileData startTile, targetTile;
        Vector2Int startGrid = isometricGrid.WorldToGridPosition(startPos);
        Vector2Int endGrid = isometricGrid.WorldToGridPosition(targetPos);
        startTile = tileDatas[startGrid.x, startGrid.y];
        targetTile = tileDatas[endGrid.x, endGrid.y];


        List<TileData> tilePath = FindPath(startTile, targetTile);
        Debug.Log(tilePath);
        if (tilePath == null)
            return null;

        List<Vector3> worldPath = new List<Vector3>();
        foreach (TileData tile in tilePath)
        {
            worldPath.Add(isometricGrid.GridPositionToWorld(new Vector2Int( tile.X, tile.Y)));
        }

        return worldPath;
    }

    // 특정 위치까지 길이 있는지 확인
    public bool IsReachable(Vector3 startPos, Vector3 targetPos)
    {
        List<Vector3> path = FindPath(startPos, targetPos);
        return path != null && path.Count > 0;
    }
}

