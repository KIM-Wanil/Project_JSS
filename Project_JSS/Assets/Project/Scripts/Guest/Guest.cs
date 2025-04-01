using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using System.Collections.Generic;
using DG.Tweening;
using System;

public class Guest : MonoBehaviour
{
    public GameObject itemOrderedPrefab;
    public ItemOrdered[] itemsOrdered;
    public TextMeshProUGUI goldText;
    public bool isCompleted = false;
    public Button completeButton;
    public int gold;

    public void Init(Dictionary<ItemKey, int> goalItems, int goldAmount)
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
        Managers.Game.AddGold(gold);
        Debug.Log($"Add Gold: {gold}");
        foreach (ItemOrdered itemOrdered in itemsOrdered)
        {
            for (int i = 0; i < itemOrdered.goalCount; i++)
            {
                Managers.Grid.FindAndRemoveNormalItemFromGrid(itemOrdered.key);
            }
        }
        Managers.Grid.CheckGuestsOrder();
        Managers.Grid.RemoveGuest(this);
        Invoke("DestroyGuest", 0.2f);
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
