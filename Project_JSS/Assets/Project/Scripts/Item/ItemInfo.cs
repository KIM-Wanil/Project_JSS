using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ItemInfo : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject checkIcon;
    [SerializeField] private Button infoButton;

    public ItemData data;
    public ItemDetails details;
    public ItemKey key;
    private bool isExist = false;
    public int lvIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init(ItemKey inputKey)
    {
        key = inputKey;
        data = Managers.Game.GetItemData(inputKey.id);

        if (image != null && data.items.Length > 0)
        {
            
            lvIndex = Mathf.Clamp(key.lv - 1, 0, data.items.Length - 1);

            details = data.items[lvIndex];
            image.sprite = details.itemSprite;
        }  
    }
    // InfoButton Ȱ��ȭ �޼��� �߰�
    public void ActivateNameText()
    {
        if (nameText != null)
        {
            nameText.text = details.itemName;
            nameText.gameObject.SetActive(true);
        }
    }
    // InfoButton Ȱ��ȭ �޼��� �߰�
    public void DeactivateNameText()
    {
        if (nameText != null)
        {
            nameText.gameObject.SetActive(false);
        }
    }
    // InfoButton Ȱ��ȭ �޼��� �߰�
    public void ActivateInfoButton()
    {
        if (infoButton != null)
        {
            //infoButton.onClick.AddListener(() =>
            //{
            //    Managers.UI.infoPanelController.ShowInfoPanel(data);
            //});
            infoButton.gameObject.SetActive(true);
        }
    }
    // checkIcon Ȱ��ȭ �޼��� �߰�
    public void ActivateCheckIcon()
    {
        if (checkIcon != null)
        {
            isExist = true;
            checkIcon.SetActive(true);
        }
    }
    // checkIcon ��Ȱ��ȭ �޼��� �߰�
    public void DeactivateCheckIcon()
    {
        if (checkIcon != null)
        {
            isExist = false;
            checkIcon.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
