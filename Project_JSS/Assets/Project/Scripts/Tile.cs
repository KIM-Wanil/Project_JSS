using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class Tile : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    public int x;
    public int y;
    public GameObject imageEven;
    public GameObject imageOdd;

    private void Awake()
    {
        if (imageEven.IsUnityNull())
        {
            imageEven = transform.GetChild(0).gameObject;
        }
        if (imageOdd.IsUnityNull())
        { 
            imageOdd = transform.GetChild(1).gameObject;
        }
    }

    public void Initialize(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
        x = gridPosition.x;
        y = gridPosition.y;

        if ((x + y) % 2 == 0)
        {
            imageEven.SetActive(true);
        }
        else
        {
            imageOdd.SetActive(true);
        }
    }

    private void OnMouseDown()
    {
        // 타일이 클릭되었을 때의 동작을 정의합니다.
        Debug.Log($"Tile at {GridPosition} clicked.");
    }
}