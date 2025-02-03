// DraggableItem.cs - 드래그 가능한 아이템 컴포넌트
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

        // 가장 가까운 빈 위치 찾기
        Vector2Int nearestEmpty = Managers.Grid.GetNearestEmptyPosition(transform.position);

        // 머지 가능한 이웃 찾기
        MergeableItem neighbor = Managers.Grid.FindMergeableNeighbor(nearestEmpty, mergeableItem);

        if (neighbor != null)
        {
            // 머지 수행
            Managers.Game.TryMergeItems(mergeableItem, neighbor);
        }
        else
        {
            // 빈 위치에 배치
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