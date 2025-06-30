using UnityEngine;

public class IsoGridWall : IsoGird
{
    [SerializeField] bool isRight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override Vector2 GridToScreenPosition(float x, float y)
    {
        float screenX = x * tileOffsetX + startPosition.x;
        float screenY =  (y + x * 0.5f) * tileOffsetY + startPosition.y + floorHeight;
        return new Vector2(screenX, screenY);
    }
    public override Vector2Int WorldToGridPosition(Vector3 worldPosition ,int heightLimit = 0)
    {
        Vector2 vector = new Vector2(worldPosition.x, worldPosition.y- floorHeight);
        int gridX = Mathf.RoundToInt((vector.x - startPosition.x) / (tileOffsetX));
        int gridY = Mathf.RoundToInt((vector.y-(gridX* tileOffsetY*0.5f + startPosition.y ) ) / tileOffsetY);

        if (gridX < 0) { gridX = 0; }
        if (gridY < heightLimit) { gridY = heightLimit; }
        if (gridX >= gridWidth) { gridX = gridWidth-1; }
        if (gridY >= gridHeight) { gridY = gridHeight-1; }
        return new Vector2Int(gridX, gridY);
    }
    public override Vector2Int WorldToGridPosition(Vector3 worldPosition, Vector2Int size, int heightLimit = 0)
    {
        Vector2 vector = new Vector2(worldPosition.x, worldPosition.y- floorHeight);
        int gridX = Mathf.RoundToInt((vector.x - startPosition.x) / (tileOffsetX));
        int gridY = Mathf.RoundToInt((vector.y - (gridX * tileOffsetY * 0.5f + startPosition.y)) / tileOffsetY);

        if (gridX < 0) { gridX = 0; }
        if (gridY < heightLimit) { gridY = heightLimit; }
        if (gridX >= gridWidth - size.x) { gridX = gridWidth - size.x; }
        if (gridY >= gridHeight - size.y) { gridY = gridHeight - size.y; }
        return new Vector2Int(gridX, gridY);
    }
    public override Vector3 GridPositionToWorld(Vector2Int gridPosition)
    {
        float worldX =  gridPosition.x * tileOffsetX + startPosition.x;
        float worldY = (gridPosition.y + gridPosition.x * 0.5f) * tileOffsetY + startPosition.y + floorHeight;
        return new Vector3(worldX, worldY, 0);
    }

    //private void OnDrawGizmos()
    //{

    //    Gizmos.color = gridLineColor;
    //    // 수평 선 그리기 (X 축 방향)
    //    for (int y = 0; y <= gridHeight; y++)
    //    {
    //        Vector3 startPos = GridPositionToWorld(new Vector2Int(0, y));
    //        Vector3 endPos = GridPositionToWorld(new Vector2Int(gridWidth, y));
    //        Handles.DrawLine(startPos, endPos);
    //    }

    //    // 수직 선 그리기 (Y 축 방향)
    //    for (int x = 0; x <= gridWidth; x++)
    //    {
    //        Vector3 startPos = GridPositionToWorld(new Vector2Int(x, 0));
    //        Vector3 endPos = GridPositionToWorld(new Vector2Int(x, gridHeight));
    //        Handles.DrawLine(startPos, endPos);
    //    }
    //}
}
