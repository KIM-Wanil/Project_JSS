using UnityEngine;
using UnityEngine.UI;
public class Tile : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    public int x;
    public int y;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Initialize(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
        x = gridPosition.x;
        y = gridPosition.y;
    }

    private void OnMouseDown()
    {
        // 타일이 클릭되었을 때의 동작을 정의합니다.
        Debug.Log($"Tile at {GridPosition} clicked.");
    }
}