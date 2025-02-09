using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEditor.Localization.Plugins.XLIFF.V12;

public class Guest : MonoBehaviour
{
    public Image image;
    public GameObject orderBox;
    public GameObject itemOrderedPrefab;
    public ItemOrdered[] itemsOrdered;
    public TextMeshProUGUI goldText;
    public bool isCompleted = false;
    public Button completeButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init(Sprite guestSprite, Item[] itemInfo, int goldAmount)
    {
        image.sprite = guestSprite;
        itemsOrdered = new ItemOrdered[itemInfo.Length];
        for (int i = 0; i < itemInfo.Length; i++)
        {
            itemsOrdered[i] = Instantiate(itemOrderedPrefab, orderBox.transform).GetComponent<ItemOrdered>();
            itemsOrdered[i].Init(itemInfo[i]);
        }
        goldText.text = goldAmount.ToString();
        completeButton.onClick.AddListener(OnCompleteButtonClicked);
        Managers.Grid.AddGuest(this);
    }
    public void CheckItemsIsExist()
    {
        int count = 0;
        foreach (var itemOrdered in itemsOrdered)
        {
            Item itemInfo = new Item { id = itemOrdered.data.id, lv = itemOrdered.lv };
            if (Managers.Grid.DoesItemExist(itemInfo))
            {
                itemOrdered.ActivateCheckIcon();
                count++;
            }
            else
            {
                itemOrdered.DeactivateCheckIcon();
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
        Managers.Game.AddGold(int.Parse(goldText.text));
        foreach (var itemOrdered in itemsOrdered)
        {
            Item itemInfo = new Item { id = itemOrdered.data.id, lv = itemOrdered.lv };
            Managers.Grid.RemoveItemFromGrid(itemInfo);
            Managers.Grid.CheckGuestsOrder();
        }
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
