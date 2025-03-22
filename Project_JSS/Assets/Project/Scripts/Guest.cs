using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using System.Collections.Generic;

public class Guest : MonoBehaviour
{
    //public GameObject orderInsideBox;
    public GameObject itemOrderedPrefab;
    public ItemInfo[] itemsOrdered;
    public TextMeshProUGUI goldText;
    public bool isCompleted = false;
    public Button completeButton;
    public int gold;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init(Dictionary<ItemKey, int> goalItems, int goldAmount)
    {
        itemsOrdered = new ItemInfo[goalItems.Count];
        int i = 0;
        foreach (var itemKey in goalItems)
        {
            itemsOrdered[i] = Instantiate(itemOrderedPrefab, this.transform).GetComponent<ItemInfo>();
            itemsOrdered[i].Init(itemKey.Key, itemKey.Value);
            i++;
        }

        gold = goldAmount;
        goldText.text = $"+{gold}";
        completeButton.onClick.AddListener(OnCompleteButtonClicked);
        Managers.Grid.AddGuest(this);
    }
    public void CheckItemsIsExist()
    {
        int count = 0;
        foreach (ItemInfo itemOrdered in itemsOrdered)
        {
            if (itemOrdered.IsComplete)
            {
                itemOrdered.UpdateCountText();
                count++;
            }
        }

        if (count == itemsOrdered.Length)
        {
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
        foreach (ItemInfo itemOrdered in itemsOrdered)
        {
            for (int i = 0; i < itemOrdered.goalCount; i++)
            {
                Managers.Grid.FindAndRemoveItemFromGrid(itemOrdered.key);
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
            isCompleted = true;
            completeButton.gameObject.SetActive(true);
        }
    }
    // checkIcon 비활성화 메서드 추가
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
    // Update is called once per frame
    void Update()
    {
        
    }
}
