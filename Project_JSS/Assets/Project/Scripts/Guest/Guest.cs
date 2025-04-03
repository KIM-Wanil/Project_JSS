using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using System.Collections.Generic;
using DG.Tweening;
using System;
using UnityEngine.Events;

public class Guest : MonoBehaviour
{
    public RectTransform goldIconRectT;
    public UnityEvent<Guest> OnGuestCompleted;

    public GameObject itemOrderedPrefab;
    public ItemOrdered[] itemsOrdered;
    public TextMeshProUGUI goldText;
    public bool isCompleted = false;
    public Button completeButton;
    public int gold;

    public void Init(int goldAmount, Dictionary<ItemKey, int> goalItems)
    {
        if (goalItems == null)
        {
            throw new ArgumentNullException(nameof(goalItems), "goalItems cannot be null");
        }

        if (itemOrderedPrefab == null)
        {
            throw new InvalidOperationException("itemOrderedPrefab is not assigned.");
        }

        itemsOrdered = new ItemOrdered[goalItems.Count];
        Debug.Log($"goalItems.Count: {goalItems.Count}");
        int i = 0;
        foreach (var itemKey in goalItems)
        {
            var itemOrderedObject = Instantiate(itemOrderedPrefab, this.transform);
            if (itemOrderedObject == null)
            {
                throw new InvalidOperationException("Failed to instantiate itemOrderedPrefab.");
            }

            itemsOrdered[i] = itemOrderedObject.GetComponent<ItemOrdered>();
            if (itemsOrdered[i] == null)
            {
                throw new InvalidOperationException("itemOrderedPrefab does not have an ItemOrdered component.");
            }

            itemsOrdered[i].Init(itemKey.Key, itemKey.Value);
            i++;
        }

        gold = goldAmount;
        goldText.text = $"+{gold}";
        completeButton.onClick.AddListener(OnCompleteButtonClicked);
        completeButton.transform.SetAsLastSibling();
        Managers.Grid.AddGuest(this);

        CheckItemsIsExist();
    }

    public void CheckItemsIsExist()
    {
        int count = 0;
        foreach (ItemOrdered itemOrdered in itemsOrdered)
        {
            itemOrdered.UpdateCountText();
            if (itemOrdered.IsFulfill)
            {
                count++;
            }
        }

        if (count == itemsOrdered.Length)
        {
            if (isCompleted) return;
            foreach (ItemOrdered itemOrdered in itemsOrdered)
            {
                itemOrdered.DeactivateAll();
            }
            OnGuestCompleted?.Invoke(this);
            ActivateCompleteButton();
        }
        else
        {
            DeactivateCompleteButton();
        }
    }

    public void OnCompleteButtonClicked()
    {
        if (!isCompleted) return;
        //Managers.Game.AddGold(gold);
        Debug.Log($"Add Gold: {gold}");
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.1f);
        foreach (ItemOrdered itemOrdered in itemsOrdered)
        {
            List<Vector2Int> targetPositions = Managers.Grid.FindNormalItemsFromGrid(itemOrdered.key, itemOrdered.goalCount);
            foreach (Vector2Int targetPos in targetPositions)
            {
                sequence.Join(Managers.Grid.RemoveItemFromGridToGuest(targetPos, itemOrdered.rectT.position));
            }
            Managers.Grid.UncheckNormalItem(itemOrdered.key);
        }
        //동전 짤랑거리면서 ui동전에 들어가는 거 sequence에 추가
        sequence.AppendCallback( ()=> Managers.Game.SpawnGold(goldIconRectT.position ,gold ,GoodsType.Gold));
        //
        sequence.OnComplete(() =>
        {
            Managers.Grid.RemoveGuest(this);
            Managers.Grid.CheckGuestsOrder();
            DestroyGuest();
        });
        
    }

    public void ActivateCompleteButton()
    {
        if (completeButton != null)
        {
            completeButton.gameObject.SetActive(true);
            completeButton.transform.localScale = Vector3.one * 0.5f; // 초기 크기를 0.5로 설정

            Sequence sequence = DOTween.Sequence();
            sequence.Append(completeButton.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutQuad))
                    .Append(completeButton.transform.DOScale(1.0f, 0.1f).SetEase(Ease.OutQuad))
                    .OnComplete(() =>
                    {
                        isCompleted = true;
                    });
        }
    }

    public void DeactivateCompleteButton()
    {
        if (completeButton != null)
        {
            isCompleted = false;
            completeButton.gameObject.SetActive(false);
        }
    }

    void DestroyGuest()
    {
        Destroy(this.gameObject);
    }

    void Update()
    {

    }
}
