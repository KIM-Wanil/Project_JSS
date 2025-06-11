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
    public static DraggableItem currentlySelectedItem;

    [Header("고정 레퍼런스")]
    public MergeableItem mergeableItem;
    private RectTransform rectTransform;
    private Canvas canvas;
    public Generator generator;
    private Coroutine mergeEffectCoroutine;

    [Header("고정 값")]
    private const float dragThreshold = 10f;
    private const float clickCooldown = 0.1f;
    private const float mergeEffectDelay = 0.2f;
    private const float checkInterval = 0.1f; // 머지 가능 여부를 확인하는 간격

    [Header("변수")]
    private Vector2 dragOffset;
    private Vector2Int initialGridPos;
    private Vector2 pointerDownPosition;

    private MergeableItem potentialMergeTarget;

    private float lastClickTime;
    private float lastCheckTime;

    public bool isSelected => currentlySelectedItem == this;
    private bool isDragging;
    public bool isInteractionEnabled = true;
    private void Awake()
    {
        if (!mergeableItem)
        {
            mergeableItem = GetComponent<MergeableItem>();
        }
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        //Initialize();
    }
    public void Initialize()
    {
        dragOffset = Vector2.zero;
        initialGridPos = Vector2Int.zero;
        pointerDownPosition = Vector2.zero;
        lastClickTime = Time.time;
        lastCheckTime = Time.time;

        isDragging = false;
        isInteractionEnabled = true;
    }
    public void SetInteractionEnabled(bool isEnabled)
    {
        isInteractionEnabled = isEnabled;
        
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractionEnabled) return; // 상호작용이 비활성화된 경우 메서드 종료

        if (currentlySelectedItem != null && currentlySelectedItem != this)
        {
            currentlySelectedItem.mergeableItem.OnDeSelected();
            currentlySelectedItem = null;
        }

        initialGridPos = mergeableItem.gridPosition;
        pointerDownPosition = eventData.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isInteractionEnabled) return; // 상호작용이 비활성화된 경우 메서드 종료

        if (mergeableItem.state == ItemState.Locked)
        {
            return;
        }

        transform.SetParent(Managers.Grid.MergeBoard.transform);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Managers.Grid.MergeBoardRectT, eventData.position, eventData.pressEventCamera, out dragOffset);
        dragOffset = rectTransform.anchoredPosition - dragOffset;

        isDragging = true;
        mergeableItem.OnDeSelected();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isInteractionEnabled) return; // 상호작용이 비활성화된 경우 메서드 종료

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
        if (!isInteractionEnabled) return; // 상호작용이 비활성화된 경우 메서드 종료

        if (mergeableItem.state == ItemState.Locked)
        {
            return;
        }

        isDragging = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isInteractionEnabled) return; // 상호작용이 비활성화된 경우 메서드 종료

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
        if (!isInteractionEnabled) return;

        if (Time.time - lastClickTime < clickCooldown)
        {
            return;
        }

        lastClickTime = Time.time;

        Debug.Log("단순 클릭");

        Managers.Grid.PlaceItem(mergeableItem, initialGridPos);
        ItemType type = mergeableItem.itemData.type;
        if (type == ItemType.Generatable && isSelected)
        {
            if (generator)
            {
                generator.TryGenerateItem();
            }
        }
        else
        {
            SelectItem();
        }
    }

    public void SelectItem()
    {
        if (!isInteractionEnabled) return; // 상호작용이 비활성화된 경우 메서드 종료

        switch (mergeableItem.itemData.type)
        {
            case ItemType.Normal:
                if (mergeableItem.state == ItemState.Locked)
                {
                    Managers.Game.SelectLockedItem(mergeableItem.itemKey);
                }
                if (mergeableItem.state == ItemState.BubbleAd)
                {
                    Managers.Game.SelectAdBubbleItem(mergeableItem.itemKey, mergeableItem.PopBubbleItemByAd, mergeableItem.GiveUpBubbleItem);
                }
                else if (mergeableItem.state == ItemState.BubbleGem)
                {
                    Managers.Game.SelectGemBubbleItem(mergeableItem.itemKey, mergeableItem.PopBubbleItemByGem, mergeableItem.SkipBubbleItem);
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
        if (!isInteractionEnabled) return;
        Debug.Log("드래그 끝");

        Vector2Int? gridPosition = Managers.Grid.GetGridPosition(rectTransform.anchoredPosition);
        if (!gridPosition.HasValue || gridPosition.Value == initialGridPos)
        {
            SelectItem();
            Managers.Grid.PlaceItem(mergeableItem, initialGridPos);
            return;
        }



        Vector2Int? nearestEmpty = Managers.Grid.GetNearestPosition(gridPosition.Value);

        MergeableItem neighbor = Managers.Grid.FindMergeableNeighbor(gridPosition.Value, mergeableItem);

        if (!neighbor.IsUnityNull() && Managers.Game.TryMergeItems(mergeableItem, neighbor))
        {
            neighbor.draggableItem.SelectItem();
            Debug.Log("머지 실행");
        }
        else
        {
            if (nearestEmpty.HasValue)
            {
                SelectItem();
                Managers.Grid.DetatchItemFromGrid(initialGridPos);
                Managers.Grid.PlaceItem(mergeableItem, nearestEmpty.Value);
            }
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
                    mergeEffectCoroutine = StartCoroutine(ShowMergeEffectAfterDelay(neighbor.gridPosition));
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
