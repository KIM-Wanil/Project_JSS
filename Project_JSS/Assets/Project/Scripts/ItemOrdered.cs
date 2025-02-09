using UnityEngine;
using UnityEngine.UI;
using static ItemData;

public class ItemOrdered : MonoBehaviour
{
    [SerializeField] public Image image;
    public bool isExist = false;
    public GameObject checkIcon;
    public ItemData data;
    public int lv;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Init(Item itemInfo)
    {
        data = Managers.Game.GetItemData(itemInfo.id);

        if (image != null && data.items.Length > 0)
        {
            lv = itemInfo.lv;
            int levelIndex = Mathf.Clamp(itemInfo.lv - 1, 0, data.items.Length - 1);
            image.sprite = data.items[levelIndex].itemSprite;
        }

        // GridManager에 자신을 추가
       
    }
    // checkIcon 활성화 메서드 추가
    public void ActivateCheckIcon()
    {
        if (checkIcon != null)
        {
            isExist = true;
            checkIcon.SetActive(true);
        }
    }
    // checkIcon 비활성화 메서드 추가
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
