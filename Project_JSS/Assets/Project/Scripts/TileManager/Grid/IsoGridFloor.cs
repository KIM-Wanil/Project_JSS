using UnityEngine;

public class IsoGridFloor : IsoGird
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    public override Vector2 GridToScreenPosition(float x, float y)
    {
        float screenX = (x - y) * tileOffsetX + startPosition.x;
        float screenY = (x + y) * tileOffsetY + startPosition.y;
        return new Vector2(screenX, screenY);
    }
    public override Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector2 vector = new Vector2(worldPosition.x, worldPosition.y);
        int gridX = Mathf.RoundToInt((vector.x / (tileOffsetX * 0.5f) + vector.y / (tileOffsetY * 0.5f)) * 0.5f);
        int gridY = Mathf.RoundToInt((vector.y / (tileOffsetY * 0.5f) - vector.x / (tileOffsetX * 0.5f)) * 0.5f);
        if (gridX < 0) { gridX = 0; }
        if (gridY < 0) { gridY = 0; }
        if (gridX >= gridWidth) { gridX = gridWidth-1; }
        if (gridY >= gridHeight) { gridY = gridHeight-1; }
        return new Vector2Int(gridX, gridY);
    }
    public override Vector2Int WorldToGridPosition(Vector3 worldPosition, Vector2Int size)
    {
        Vector2 vector = new Vector2(worldPosition.x, worldPosition.y);
        int gridX = Mathf.RoundToInt((vector.x / (tileOffsetX * 0.5f) + vector.y / (tileOffsetY * 0.5f)) * 0.5f);
        int gridY = Mathf.RoundToInt((vector.y / (tileOffsetY * 0.5f) - vector.x / (tileOffsetX * 0.5f)) * 0.5f);
        if (gridX < 0) { gridX = 0; }
        if (gridY < 0) { gridY = 0; }

        if (gridX >= gridWidth-size.x) { gridX = gridWidth - size.x; }
        if (gridY >= gridHeight - size.y) { gridY = gridHeight - size.y; }
        return new Vector2Int(gridX, gridY);
    }
    public override Vector3 GridPositionToWorld(Vector2Int gridPosition)
    {
        float worldX = (gridPosition.x - gridPosition.y) * (tileOffsetX * 0.5f);
        float worldY = (gridPosition.y + gridPosition.x) * (tileOffsetY * 0.5f);
        return new Vector3(worldX, worldY, worldY);
    }
    private void OnDrawGizmos()
    {


        Gizmos.color = gridLineColor;
            // Draw horizontal grid lines (left-to-right diagonals in isometric view)
            for (float y = 0; y <= gridHeight; y++)
            {
                Vector2 startPos = GridToScreenPosition(0, y / 2);
                Vector2 endPos = GridToScreenPosition(gridWidth / 2, y / 2);
                Gizmos.DrawLine(startPos, endPos);
            }

            // Draw vertical grid lines (right-to-left diagonals in isometric view)
            for (float x = 0; x <= gridWidth; x++)
            {
                Vector2 startPos = GridToScreenPosition(x / 2, 0);
                Vector2 endPos = GridToScreenPosition(x / 2, gridHeight / 2);
                Gizmos.DrawLine(startPos, endPos);
            }
    }
}
