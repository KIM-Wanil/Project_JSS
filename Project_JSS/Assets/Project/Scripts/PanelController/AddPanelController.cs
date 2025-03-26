using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class AddPanelController : MonoBehaviour
{
    public GameObject rewardList;
    public Button rewardListButton;
    public GameObject[] rewardCards = new GameObject[3];
    public Image rewardItmeImage;
    public GameObject countIcon;
    public TextMeshProUGUI countText;

    private void Awake()
    {
        Managers.Game.onRewardQueueChanged.AddListener(UpdateRewardList);
        rewardListButton.onClick.AddListener(OnClickRewardListButton);

        rewardList.SetActive(false);
        rewardCards[0].SetActive(true);
        rewardCards[1].SetActive(false);
        rewardCards[2].SetActive(false);
        countIcon.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void OnClickRewardListButton()
    {
        ItemType RewardItemType = Managers.Game.GetItemData(Managers.Game.currentRewardQueue.Peek().id).type;
        if (RewardItemType == ItemType.Generatable)
        {
            Managers.Game.SpawnMoveGenerator(Managers.Game.currentRewardQueue.Peek().id, Managers.Game.currentRewardQueue.Peek().lv, new Vector2(-115f, 515f), (Vector2Int)Managers.Grid.GetEmptyPosition());
            Managers.Game.DequeueReward();
        }
    }
    public void UpdateRewardList(Queue<ItemKey> rewardQueue)
    {
        Debug.Log("UpdateRewardList");
        Debug.Log(rewardQueue.Count);
        if (rewardQueue.Count <= 0)
        {
            rewardList.SetActive(false);
            return;
        }
        else
        {
            rewardList.SetActive(true);
            ItemKey rewardKey = rewardQueue.Peek();
            rewardItmeImage.sprite = Managers.Game.GetItemData(rewardQueue.Peek().id).items[rewardQueue.Peek().lv-1].itemSprite;
            if (rewardQueue.Count == 1)
            {
                rewardCards[0].SetActive(true);
                rewardCards[1].SetActive(false);
                rewardCards[2].SetActive(false);

                countIcon.SetActive(false);
            }
            else if (rewardQueue.Count == 2)
            {
                rewardCards[0].SetActive(true);
                rewardCards[1].SetActive(true);
                rewardCards[2].SetActive(false);

                countIcon.SetActive(false);
            }
            else//if (rewardQueue.Count >= 3)
            {
                rewardCards[0].SetActive(true);
                rewardCards[1].SetActive(true);
                rewardCards[2].SetActive(true);

                countIcon.SetActive(true);

            }

        }
        
        

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
