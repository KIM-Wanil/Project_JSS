// DraggableItem.cs - �巡�� ������ ������ ������Ʈ
using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    private Vector3 dragOffset;
    private Camera mainCamera;
    private MergeableItem mergeableItem;

    private void Start()
    {
        mainCamera = Camera.main;
        mergeableItem = GetComponent<MergeableItem>();
    }

    private void OnMouseDown()
    {
        dragOffset = transform.position - GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        Vector2Int gridPosition = Managers.Grid.GetGridPosition(transform.position);

        // ���� ����� �� ��ġ ã��
        Vector2Int nearestEmpty = Managers.Grid.GetNearestEmptyPosition(transform.position);

        // ���� ������ �̿� ã��
        MergeableItem neighbor = Managers.Grid.FindMergeableNeighbor(nearestEmpty, mergeableItem);

        if (neighbor != null)
        {
            // ���� ����
            Managers.Game.TryMergeItems(mergeableItem, neighbor);
        }
        else
        {
            // �� ��ġ�� ��ġ
            Managers.Grid.PlaceItem(mergeableItem, nearestEmpty);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }
}