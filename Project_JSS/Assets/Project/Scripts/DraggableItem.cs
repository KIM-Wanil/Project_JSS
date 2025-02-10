// DraggableItem.cs - 드래그 가능한 아이템 컴포넌트
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
public class DraggableItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    private Vector2 dragOffset;
    private Vector2Int initialGridPos;
    private RectTransform rectTransform;
    private Canvas canvas;
    private MergeableItem mergeableItem;
    private bool isDragging; // 드래그 상태를 추적하는 변수
    [SerializeField]private bool canClick = true; // 클릭 가능 여부를 추적하는 변수
    private float clickCooldown = 0.1f; // 클릭을 무시할 시간 (초)

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mergeableItem = GetComponent<MergeableItem>();
        canvas = GetComponentInParent<Canvas>();
        initialGridPos = mergeableItem.GridPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        initialGridPos = mergeableItem.GridPosition;
        Managers.Grid.RemoveItem(mergeableItem.GridPosition);
        transform.SetParent(Managers.Grid.MergeBoard.transform);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Managers.Grid.MergeBoardRectT, eventData.position, eventData.pressEventCamera, out dragOffset);
        dragOffset = rectTransform.anchoredPosition - dragOffset;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true; // 드래그 시작
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Managers.Grid.MergeBoardRectT, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint + dragOffset;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false; // 드래그 종료
        StartCoroutine(EnableClickAfterCooldown());
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        

        Vector2Int? gridPosition = Managers.Grid.GetGridPosition(rectTransform.anchoredPosition);
        if(gridPosition == null)
        {
            Managers.Grid.PlaceItem(mergeableItem, initialGridPos);
            return;
        }

        Vector2Int nearestEmpty = Managers.Grid.GetNearestEmptyPosition((Vector2Int)gridPosition);

        // 머지 가능한 이웃 찾기
        MergeableItem neighbor = Managers.Grid.FindMergeableNeighbor((Vector2Int)gridPosition, mergeableItem);

        if (neighbor != null)
        {
            // 머지 수행
            Managers.Game.TryMergeItems(mergeableItem, neighbor);
        }
        else
        {
            //Debug.Log("neighbor is null");
            // 빈 위치에 배치
            Managers.Grid.PlaceItem(mergeableItem, nearestEmpty);
        }

        // 가장 가까운 빈 위치 찾기


    }

    private IEnumerator EnableClickAfterCooldown()
    {
        canClick = false;
        yield return new WaitForSeconds(clickCooldown);
        canClick = true;
    }

    public bool IsDragging()
    {
        return isDragging;
    }

    public bool CanClick()
    {
        return canClick;
    }
}
