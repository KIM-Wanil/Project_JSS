// DraggableItem.cs - �巡�� ������ ������ ������Ʈ
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using static SaveData;
using System;
public class DraggableItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    private Vector2 dragOffset;
    private Vector2Int initialGridPos;
    private RectTransform rectTransform;
    private Canvas canvas;
    private MergeableItem mergeableItem;
    public Generator generator;
    private bool isDragging; // �巡�� ���¸� �����ϴ� ����
    //[SerializeField] private bool canClick = true; // Ŭ�� ���� ���θ� �����ϴ� ����
    private float clickCooldown = 0.1f; // Ŭ���� ������ �ð� (��)
    private bool isSelected => currentlySelectedItem == this; // ���� ���õ� ���������� Ȯ���ϴ� ������Ƽ
    // ���� ���õ� �������� �����ϴ� ���� ����
    private static DraggableItem currentlySelectedItem;

    // Ŭ���� �巡�׸� �����ϱ� ���� ����
    private Vector2 pointerDownPosition;
    private const float dragThreshold = 10f; // �巡�׷� ������ �ּ� �̵� �Ÿ�

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mergeableItem = GetComponent<MergeableItem>();
        canvas = GetComponentInParent<Canvas>();
        initialGridPos = mergeableItem.GridPosition;
    }
    public void PrintGeneratorDesc()
    {
        Managers.Game.infoPanelController.PrintGeneratorDesc(mergeableItem.itemKey, generator.generatableSprites);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // ������ ���õ� �������� selectIcon ��Ȱ��ȭ
        if (currentlySelectedItem != null && currentlySelectedItem != this)
        {
            currentlySelectedItem.mergeableItem.OnDeSelected();
            currentlySelectedItem = null;
        }

        initialGridPos = mergeableItem.GridPosition;
        Managers.Grid.DetatchItemFromGrid(mergeableItem.GridPosition);
        transform.SetParent(Managers.Grid.MergeBoard.transform);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Managers.Grid.MergeBoardRectT, eventData.position, eventData.pressEventCamera, out dragOffset);
        dragOffset = rectTransform.anchoredPosition - dragOffset;

        mergeableItem.OnDeSelected();

        // ���콺 ��ư�� ���� ��ġ ����
        pointerDownPosition = eventData.position;
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
        //StartCoroutine(EnableClickAfterCooldown());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // ���콺 ��ư�� �� ��ġ�� ���� ��ġ�� �Ÿ� ���
        float distance = Vector2.Distance(pointerDownPosition, eventData.position);

        if (distance < dragThreshold)
        {
            // �ܼ� Ŭ������ ����
            HandleClick();
        }
        else
        {
            // �巡�׷� ����
            HandleDragEnd(eventData);
        }
    }

    private void HandleClick()
    {
        // �ܼ� Ŭ�� ó�� ����
        Debug.Log("�ܼ� Ŭ��");

        
        // ���� �������� ���õ� ���������� ����
        
        Managers.Grid.PlaceItem(mergeableItem, initialGridPos);
        switch (mergeableItem.itemData.type)
        {
            case ItemType.Normal:
            case ItemType.Crafted:
                Managers.Game.infoPanelController.PrintItemDesc(mergeableItem.itemKey, mergeableItem.price, mergeableItem.SellThisItem);
                break;

                //Managers.Game.infoPanelController.PrintComponentItems(mergeableItem.itemKey);
                //break;

            case ItemType.Generatable:
                if (generator != null)
                {
                    if(isSelected)
                    {
                        generator.TryGenerateItem();
                    }
                    else
                    {
                        PrintGeneratorDesc();
                    }
                } 
                break;

            default:
                break;

        }
        currentlySelectedItem = this;
        mergeableItem.OnSelected();
    }

    private void HandleDragEnd(PointerEventData eventData)
    {
        //�巡�� ó�� ����
        Debug.Log("�巡�� ��");

        Vector2Int? gridPosition = Managers.Grid.GetGridPosition(rectTransform.anchoredPosition);
        if (gridPosition == null)
        {
            Managers.Grid.PlaceItem(mergeableItem, initialGridPos);
            return;
        }

        Vector2Int nearestEmpty = Managers.Grid.GetNearestEmptyPosition((Vector2Int)gridPosition);

        // ���� ������ �̿� ã��
        MergeableItem neighbor = Managers.Grid.FindMergeableNeighbor((Vector2Int)gridPosition, mergeableItem);

        if (neighbor != null && mergeableItem.itemData.type !=ItemType.Crafted && Managers.Game.TryMergeItems(mergeableItem, neighbor))
        {
            // ���� ����
            Debug.Log("���� ����");
        }
        else
        {
            // �� ��ġ�� ��ġ
            Managers.Grid.PlaceItem(mergeableItem, nearestEmpty); 
        }

        
    }

    //private IEnumerator EnableClickAfterCooldown()
    //{
    //    canClick = false;
    //    yield return new WaitForSeconds(clickCooldown);
    //    canClick = true;
    //}


    //public bool CanClick()
    //{
    //    return canClick;
    //}
}
