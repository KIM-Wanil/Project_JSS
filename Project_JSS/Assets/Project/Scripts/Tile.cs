using UnityEngine;
using UnityEngine.UI;
public class Tile : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Initialize(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
    }

    private void OnMouseDown()
    {
        // Ÿ���� Ŭ���Ǿ��� ���� ������ �����մϴ�.
        Debug.Log($"Tile at {GridPosition} clicked.");
    }
}