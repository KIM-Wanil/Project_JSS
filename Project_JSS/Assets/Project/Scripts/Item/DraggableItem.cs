// DraggableItem.cs - 드래그 가능한 아이템 컴포넌트
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using static SaveData;
using System;
using Unity.VisualScripting;
using DG.Tweening;
public class DraggableItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    private Vector2 dragOffset;
    private Vector2Int initialGridPos;
    private RectTransform rectTransform;
    private Canvas canvas;
    public MergeableItem mergeableItem;
    public Generator generator;
    private bool isDragging;
    private float clickCooldown = 0.1f;
    private bool isSelected => currentlySelectedItem == this;
    public static DraggableItem currentlySelectedItem;

    private Vector2 pointerDownPosition;
    private const float dragThreshold = 10f;

    private MergeableItem potentialMergeTarget;
    private float mergeEffectDelay = 0.2f;
    private Coroutine mergeEffectCoroutine;
    private float checkInterval = 0.1f; // 머지 가능 여부를 확인하는 간격
    private float lastCheckTime;


    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (mergeableItem.IsUnityNull())
        {
            mergeableItem = GetComponent<MergeableItem>();
        }
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

        



        pointerDownPosition = eventData.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (mergeableItem.state == ItemState.Locked)
        {
            return;
        }
        //////
        transform.SetParent(Managers.Grid.MergeBoard.transform);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Managers.Grid.MergeBoardRectT, eventData.position, eventData.pressEventCamera, out dragOffset);
        dragOffset = rectTransform.anchoredPosition - dragOffset;
        /////


        isDragging = true;
        mergeableItem.OnDeSelected();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (mergeableItem.state == ItemState.Locked)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Managers.Grid.MergeBoardRectT, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint + dragOffset;
        }

        if (Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time;
            CheckMergeable();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (mergeableItem.state == ItemState.Locked)
        {
            return;
        }

        isDragging = false;
        //HandleDragEnd(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float distance = Vector2.Distance(pointerDownPosition, eventData.position);

        if (distance < dragThreshold)
        {
            HandleClick();
        }
        else if (mergeableItem.state != ItemState.Locked)
        {
            HandleDragEnd(eventData);
        }
    }

    private void HandleClick()
    {
        Debug.Log("단순 클릭");

        Managers.Grid.PlaceItem(mergeableItem, initialGridPos);
        ItemType type = mergeableItem.itemData.type;
        if (type == ItemType.Generatable && !isSelected)
        {
            generator.TryGenerateItem();
            DG.Tweening.Sequence sequence = DOTween.Sequence();
            sequence.Append(mergeableItem.itemRectT.DOScale(new Vector3(1.08f, 0.8f, 1f), 13f / 60f).SetEase(Ease.OutQuad))
                    .Append(mergeableItem.itemRectT.DOScale(new Vector3(0.9f, 1.25f, 0.95f), 7f / 60f).SetEase(Ease.OutQuad))
                    .Append(mergeableItem.itemRectT.DOScale(new Vector3(1.1f, 0.94f, 1f), 12f / 60f).SetEase(Ease.OutQuad))
                    .Append(mergeableItem.itemRectT.DOScale(new Vector3(0.96f, 1f, 1f), 10f / 60f).SetEase(Ease.OutQuad))
                    .Append(mergeableItem.itemRectT.DOScale(new Vector3(1f, 1f, 1f), 10f / 60f).SetEase(Ease.OutQuad));
            sequence.Play();
        }
        else
        {
            SelectItem();
        }
    }

    public void SelectItem()
    {
        switch (mergeableItem.itemData.type)
        {
            case ItemType.Normal:
                if (mergeableItem.state == ItemState.Locked)
                {
                    Managers.Game.SelectLockedItem(mergeableItem.itemKey);
                }
                else

                {
                    Managers.Game.SelectSellableItem(mergeableItem.itemKey, mergeableItem.price, mergeableItem.SellThisItem);
                }
                break;

            case ItemType.Generatable:
                Managers.Game.SelectUnsellableItem(mergeableItem.itemKey);
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

        if (!neighbor.IsUnityNull() && Managers.Game.TryMergeItems(mergeableItem, neighbor))
        {
            
            neighbor.draggableItem.SelectItem();
            Debug.Log("머지 실행");
        }
        else
        {
            SelectItem();
            Managers.Grid.DetatchItemFromGrid(initialGridPos);
            Managers.Grid.PlaceItem(mergeableItem, nearestEmpty);
        }

        potentialMergeTarget = null;
        if (mergeEffectCoroutine != null)
        {
            StopCoroutine(mergeEffectCoroutine);
            mergeEffectCoroutine = null;
            Managers.Grid.StopMergeEffect();
        }
    }

    private void CheckMergeable()
    {
        Vector2Int? gridPosition = Managers.Grid.GetGridPosition(rectTransform.anchoredPosition);
        if (gridPosition != null)
        {
            MergeableItem neighbor = Managers.Grid.FindMergeableNeighbor((Vector2Int)gridPosition, mergeableItem);
            if (!neighbor.IsUnityNull() && Managers.Game.CanMerge(mergeableItem, neighbor))
            {
                if (potentialMergeTarget != neighbor)
                {
                    potentialMergeTarget = neighbor;
                    if (mergeEffectCoroutine != null)
                    {
                        StopCoroutine(mergeEffectCoroutine);
                        Managers.Grid.StopMergeEffect();
                    }
                    mergeEffectCoroutine = StartCoroutine(ShowMergeEffectAfterDelay(neighbor.GridPosition));
                }
            }
            else
            {
                potentialMergeTarget = null;
                if (mergeEffectCoroutine != null)
                {
                    StopCoroutine(mergeEffectCoroutine);
                    mergeEffectCoroutine = null;
                    Managers.Grid.StopMergeEffect();
                }
            }
        }
    }

    private IEnumerator ShowMergeEffectAfterDelay(Vector2Int gridPos)
    {
        yield return new WaitForSeconds(mergeEffectDelay);
        if (potentialMergeTarget != null)
        {
            Managers.Grid.PlayMergeEffect(gridPos);
            Debug.Log("머지 이펙트 발생");
        }
    }
}
