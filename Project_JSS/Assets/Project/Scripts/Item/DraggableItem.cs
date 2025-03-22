// DraggableItem.cs - 드래그 가능한 아이템 컴포넌트
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
    public MergeableItem mergeableItem;
    public Generator generator;
    private bool isDragging; // 드래그 상태를 추적하는 변수
    private float clickCooldown = 0.1f; // 클릭을 무시할 시간 (초)
    private bool isSelected => currentlySelectedItem == this; // 현재 선택된 아이템인지 확인하는 프로퍼티
    public static DraggableItem currentlySelectedItem;

    private Vector2 pointerDownPosition;
    private const float dragThreshold = 10f; // 드래그로 간주할 최소 이동 거리

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mergeableItem = GetComponent<MergeableItem>();
        canvas = GetComponentInParent<Canvas>();
        initialGridPos = mergeableItem.GridPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
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

        

        pointerDownPosition = eventData.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true; // 드래그 시작
        
        mergeableItem.OnDeSelected();
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
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float distance = Vector2.Distance(pointerDownPosition, eventData.position);

        if (distance < dragThreshold)
        {
            HandleClick();
        }
        else
        {
            HandleDragEnd(eventData);
        }
    }

    private void HandleClick()
    {
        Debug.Log("단순 클릭");

        Managers.Grid.PlaceItem(mergeableItem, initialGridPos);
        switch (mergeableItem.itemData.type)
        {
            case ItemType.Normal:
                Managers.Game.infoPanelController.PrintItemDesc(mergeableItem.itemKey, mergeableItem.price, mergeableItem.SellThisItem);
                break;

            case ItemType.Generatable:
                if (generator != null)
                {
                    if (isSelected)
                    {
                        generator.TryGenerateItem();
                    }
                    else
                    {
                        Managers.Game.infoPanelController.PrintItemDesc(mergeableItem.itemKey);
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
        Debug.Log("드래그 끝");

        Vector2Int? gridPosition = Managers.Grid.GetGridPosition(rectTransform.anchoredPosition);
        if (gridPosition == null)
        {
            Managers.Grid.PlaceItem(mergeableItem, initialGridPos);
            return;
        }

        Vector2Int nearestEmpty = Managers.Grid.GetNearestEmptyPosition((Vector2Int)gridPosition);

        MergeableItem neighbor = Managers.Grid.FindMergeableNeighbor((Vector2Int)gridPosition, mergeableItem);

        if (neighbor != null && mergeableItem.itemData.type != ItemType.Crafted && Managers.Game.TryMergeItems(mergeableItem, neighbor))
        {
            Debug.Log("머지 실행");
        }
        else
        {
            Managers.Grid.PlaceItem(mergeableItem, nearestEmpty);
        }
    }
}
