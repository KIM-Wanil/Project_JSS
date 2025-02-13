// DraggableItem.cs - �巡�� ������ ������ ������Ʈ
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
    public Generator generator;
    private bool isDragging; // �巡�� ���¸� �����ϴ� ����
    [SerializeField]private bool canClick = true; // Ŭ�� ���� ���θ� �����ϴ� ����
    private float clickCooldown = 0.1f; // Ŭ���� ������ �ð� (��)

    // ���� ���õ� �������� �����ϴ� ���� ����
    private static DraggableItem currentlySelectedItem;
    private bool isSelected => currentlySelectedItem == this;

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
        Managers.Grid.DetatchItemFromGrid(mergeableItem.GridPosition);
        transform.SetParent(Managers.Grid.MergeBoard.transform);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Managers.Grid.MergeBoardRectT, eventData.position, eventData.pressEventCamera, out dragOffset);
        dragOffset = rectTransform.anchoredPosition - dragOffset;

        mergeableItem.OnDeSelected();
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


        Vector2Int? gridPosition = Managers.Grid.GetGridPosition(rectTransform.anchoredPosition);
        if(gridPosition == null)
        {
            Managers.Grid.PlaceItem(mergeableItem, initialGridPos);
            return;
        }

        Vector2Int nearestEmpty = Managers.Grid.GetNearestEmptyPosition((Vector2Int)gridPosition);

        // ���� ������ �̿� ã��
        MergeableItem neighbor = Managers.Grid.FindMergeableNeighbor((Vector2Int)gridPosition, mergeableItem);

        if (neighbor != null && Managers.Game.TryMergeItems(mergeableItem, neighbor))
        {
            // ���� ����
            //Managers.Game.TryMergeItems(mergeableItem, neighbor);
            Debug.Log("���� ����");
        }

        else
        {
            //Debug.Log("neighbor is null");
            // �� ��ġ�� ��ġ
            
            Managers.Grid.PlaceItem(mergeableItem, nearestEmpty);

            if (mergeableItem.itemData.type == ItemType.Generatable && !IsDragging() && CanClick() && IsSelected())
            {
                generator.TryGenerateItem();
            }
        }

        

        // ������ ���õ� �������� selectIcon ��Ȱ��ȭ
        if (currentlySelectedItem != null && currentlySelectedItem != this)
        {
            currentlySelectedItem.mergeableItem.selectIcon.SetActive(false);
        }
        // ���� �������� ���õ� ���������� ����
        currentlySelectedItem = this;
        mergeableItem.OnSelected();
    }

    private IEnumerator EnableClickAfterCooldown()
    {
        canClick = false;
        yield return new WaitForSeconds(clickCooldown);
        canClick = true;
    }

    public bool IsSelected()
    {
        return isSelected;
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
