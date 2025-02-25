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
    private MergeableItem mergeableItem;
    public Generator generator;
    private bool isDragging; // 드래그 상태를 추적하는 변수
    //[SerializeField] private bool canClick = true; // 클릭 가능 여부를 추적하는 변수
    private float clickCooldown = 0.1f; // 클릭을 무시할 시간 (초)
    private bool isSelected => currentlySelectedItem == this; // 현재 선택된 아이템인지 확인하는 프로퍼티
    // 현재 선택된 아이템을 추적하는 정적 변수
    private static DraggableItem currentlySelectedItem;

    // 클릭과 드래그를 구분하기 위한 변수
    private Vector2 pointerDownPosition;
    private const float dragThreshold = 10f; // 드래그로 간주할 최소 이동 거리

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
        // 이전에 선택된 아이템의 selectIcon 비활성화
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

        // 마우스 버튼을 누른 위치 저장
        pointerDownPosition = eventData.position;
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
        //StartCoroutine(EnableClickAfterCooldown());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 마우스 버튼을 뗀 위치와 누른 위치의 거리 계산
        float distance = Vector2.Distance(pointerDownPosition, eventData.position);

        if (distance < dragThreshold)
        {
            // 단순 클릭으로 간주
            HandleClick();
        }
        else
        {
            // 드래그로 간주
            HandleDragEnd(eventData);
        }
    }

    private void HandleClick()
    {
        // 단순 클릭 처리 로직
        Debug.Log("단순 클릭");

        
        // 현재 아이템을 선택된 아이템으로 설정
        
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
        //드래그 처리 로직
        Debug.Log("드래그 끝");

        Vector2Int? gridPosition = Managers.Grid.GetGridPosition(rectTransform.anchoredPosition);
        if (gridPosition == null)
        {
            Managers.Grid.PlaceItem(mergeableItem, initialGridPos);
            return;
        }

        Vector2Int nearestEmpty = Managers.Grid.GetNearestEmptyPosition((Vector2Int)gridPosition);

        // 머지 가능한 이웃 찾기
        MergeableItem neighbor = Managers.Grid.FindMergeableNeighbor((Vector2Int)gridPosition, mergeableItem);

        if (neighbor != null && mergeableItem.itemData.type !=ItemType.Crafted && Managers.Game.TryMergeItems(mergeableItem, neighbor))
        {
            // 머지 수행
            Debug.Log("머지 실행");
        }
        else
        {
            // 빈 위치에 배치
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
