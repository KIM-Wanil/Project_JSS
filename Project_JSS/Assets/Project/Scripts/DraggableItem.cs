// DraggableItem.cs - �巡�� ������ ������ ������Ʈ
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
public class DraggableItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    private Vector2 dragOffset;
    private RectTransform rectTransform;
    private Canvas canvas;
    private MergeableItem mergeableItem;
    private bool isDragging; // �巡�� ���¸� �����ϴ� ����
    [SerializeField]private bool canClick = true; // Ŭ�� ���� ���θ� �����ϴ� ����
    private float clickCooldown = 0.1f; // Ŭ���� ������ �ð� (��)

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mergeableItem = GetComponent<MergeableItem>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Managers.Grid.RemoveItem(mergeableItem.GridPosition);
        transform.SetParent(Managers.Grid.MergeBoard.transform);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Managers.Grid.MergeBoardRectT, eventData.position, eventData.pressEventCamera, out dragOffset);
        dragOffset = rectTransform.anchoredPosition - dragOffset;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true; // �巡�� ����
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
        isDragging = false; // �巡�� ����
        StartCoroutine(EnableClickAfterCooldown());
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        

        Vector2Int gridPosition = Managers.Grid.GetGridPosition(rectTransform.anchoredPosition);

        // ���� ����� �� ��ġ ã��
        Vector2Int nearestEmpty = Managers.Grid.GetNearestEmptyPosition(rectTransform.anchoredPosition);

        // ���� ������ �̿� ã��
        MergeableItem neighbor = Managers.Grid.FindMergeableNeighbor(gridPosition, mergeableItem);

        if (neighbor != null)
        {
            // ���� ����
            Managers.Game.TryMergeItems(mergeableItem, neighbor);
        }
        else
        {
            //Debug.Log("neighbor is null");
            // �� ��ġ�� ��ġ
            Managers.Grid.PlaceItem(mergeableItem, nearestEmpty);
        }
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
