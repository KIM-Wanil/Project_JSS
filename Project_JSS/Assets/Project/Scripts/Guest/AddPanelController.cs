using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;
public class AddPanelController : MonoBehaviour
{
    public RectTransform firstGuestRectT;
    [Header("Guest References")]
    [SerializeField] private GameObject guestBoard;
    [SerializeField] private GameObject guestPrefab;
    public ScrollRect scrollRect;

    public GameObject rewardList;
    public Button rewardListButton;
    public GameObject[] rewardCards = new GameObject[3];
    public Image rewardItmeImage;
    public GameObject countIcon;
    public TextMeshProUGUI countText;

    private void Awake()
    {
        Managers.Game.onRewardQueueChanged.AddListener(UpdateRewardList);
        Managers.Game.onRandomGuestCreated.AddListener(CreateRandomGuest);
        Managers.Game.onGuestCreated.AddListener(CreateGuest);
        rewardListButton.onClick.AddListener(OnClickRewardListButton);

        rewardList.SetActive(false);
        rewardCards[0].SetActive(true);
        rewardCards[1].SetActive(false);
        rewardCards[2].SetActive(false);
        countIcon.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateRandomGuest();
        }
    }

    #region Guest Management
    public void CreateRandomGuest()
    {
        Guest guest = Instantiate(guestPrefab, guestBoard.transform).GetComponent<Guest>();
        if (guest == null)
        {
            throw new InvalidOperationException("Failed to instantiate guestPrefab.");
        }
        int count = UnityEngine.Random.Range(1, 3);
        ItemKey[] tempItems = new ItemKey[count];
        List<string> availableItems = Managers.Game.GetAvailableItemIds();
        if (availableItems.Count <= 0)
        {
            Debug.LogError("제너레이터가 없음");
            return;
        }
        Dictionary<ItemKey, int> goalItems = new Dictionary<ItemKey, int>();
        for (int i = 0; i < count; i++)
        {
            tempItems[i].id = availableItems[UnityEngine.Random.Range(0, availableItems.Count)];
            tempItems[i].lv = UnityEngine.Random.Range(2, 4);

            goalItems[tempItems[i]] = UnityEngine.Random.Range(1, 3);
        }
        int goldAmount = UnityEngine.Random.Range(1, 4);
        goldAmount *= count;
        Debug.Log("골아이템개수" + goalItems.Count());

        guest.Init(goldAmount, goalItems);
        guest.OnGuestCompleted.AddListener(MoveCompletedGuestToLeft);
    }

    public void CreateGuest(int goldAmount, KeyValuePair<ItemKey, int> item0, KeyValuePair<ItemKey, int>? item1 = null)
    {
        Dictionary<ItemKey, int> goalItems = new Dictionary<ItemKey, int>();
        goalItems[item0.Key] = item0.Value;

        if (item1.HasValue)
        {

            goalItems[item1.Value.Key] = item1.Value.Value;
        }

        Guest guest = Instantiate(guestPrefab, guestBoard.transform).GetComponent<Guest>();
        if (guest == null)
        {
            throw new InvalidOperationException("Failed to instantiate guestPrefab.");
        }
        guest.Init(goldAmount,goalItems);
        guest.OnGuestCompleted.AddListener(MoveCompletedGuestToLeft);
    }
    //public void MoveCompletedGuestToLeft(Guest completedGuest)
    //{
    //    // Get the list of all guests
    //    Debug.Log("MoveCompletedGuestToLeft");
    //    RectTransform completedGuestRect = completedGuest.rectT;
    //    int targetCount = completedGuestRect.GetSiblingIndex();
    //    // Check if the completed guest is already at the target position
    //    if (targetCount == 3)
    //    {
    //        Debug.Log("이미첫번째게스트라 리턴");
    //        return; // If already at the target position, do nothing
    //    }
    //    List<RectTransform> guestRects = new List<RectTransform>();
    //    for (int i = 3; i < targetCount; i++)
    //    {
    //        Transform child = guestBoard.transform.GetChild(i);
    //        if (child.GetComponent<Guest>() != null && child.gameObject != completedGuest.gameObject)
    //        {
    //            guestRects.Add(child.GetComponent<RectTransform>());
    //        }
    //    }

    //    // Move the completed guest to the first position
        
    //    float spacing = 20f; // HorizontalLayoutGroup의 spacing 값
    //    float targetX = firstGuestRectT.position.x;

        

    //    Sequence sequence = DOTween.Sequence();
    //    // Scroll to the left with animation
    //    sequence.AppendCallback(() =>
    //    {
    //        DOTween.To(() => scrollRect.horizontalNormalizedPosition, x => scrollRect.horizontalNormalizedPosition = x, 0, 0.5f);
    //    });

    //    // Move the completed guest to the first position if it's not already there
    //    int completedGuestIndex = Managers.Grid.currentGuests.FindIndex(x => x.Equals(completedGuest));
    //    if (completedGuestIndex != 0)
    //    {
    //        // Create a copy of the currentGuests list
    //        List<Guest> currentGuestsCopy = new List<Guest>(Managers.Grid.currentGuests);

    //        // Remove the completed guest from its current position
    //        currentGuestsCopy.RemoveAt(completedGuestIndex);
    //        // Insert the completed guest at the first position
    //        currentGuestsCopy.Insert(0, completedGuest);

    //        // Update the original list
    //        Managers.Grid.currentGuests = currentGuestsCopy;

    //        sequence.Join(completedGuestRect.DOMoveX(targetX, 0.5f));
    //        // Move other guests to the right
    //        for (int i = 0; i < guestRects.Count; i++)
    //        {
    //            float newX = guestRects[i].position.x + (completedGuestRect.rect.width + spacing);
    //            sequence.Join(guestRects[i].DOMoveX(newX, 0.5f));
    //        }
    //    }

    //    sequence.OnComplete(() =>
    //    {
    //        // Set sibling index for completed guest
    //        completedGuestRect.SetSiblingIndex(3);

    //        // Set sibling index for other guests
    //        for (int i = 0; i < guestRects.Count; i++)
    //        {
    //            guestRects[i].SetSiblingIndex(4 + i);
    //        }
    //    });

    //    sequence.Play();
    //}
    public void MoveCompletedGuestToLeft(Guest completedGuest)
    {
        // Get the list of all guests
        Debug.Log("MoveCompletedGuestToLeft");
        RectTransform completedGuestRect = completedGuest.rectT;
        int targetCount = completedGuestRect.GetSiblingIndex();
        // Check if the completed guest is already at the target position
        if (targetCount == 3)
        {
            Debug.Log("이미첫번째게스트라 리턴");
            return; // If already at the target position, do nothing
        }
        List<RectTransform> guestRects = new List<RectTransform>();
        for (int i = 3; i < targetCount; i++)
        {
            Transform child = guestBoard.transform.GetChild(i);
            if (child.GetComponent<Guest>() != null && child.gameObject != completedGuest.gameObject)
            {
                guestRects.Add(child.GetComponent<RectTransform>());
            }
        }

        // Move the completed guest to the first position
        float spacing = 20f; // HorizontalLayoutGroup의 spacing 값
        float targetX = firstGuestRectT.anchoredPosition.x;

        // Check if the completed guest is already at the target position
        
        //if (Mathf.Approximately(completedGuestRect.anchoredPosition.x, targetX))
        //{
        //    return; // If already at the target position, do nothing
        //}

        Sequence sequence = DOTween.Sequence();
        // Scroll to the left with animation
        sequence.AppendCallback(() =>
        {
            DOTween.To(() => scrollRect.horizontalNormalizedPosition, x => scrollRect.horizontalNormalizedPosition = x, 0, 0.5f);
        });

        // Move the completed guest to the first position if it's not already there
        int completedGuestIndex = Managers.Grid.currentGuests.FindIndex(x => x.Equals(completedGuest));
        if (completedGuestIndex != 0)
        {
            // Create a copy of the currentGuests list
            List<Guest> currentGuestsCopy = new List<Guest>(Managers.Grid.currentGuests);

            // Remove the completed guest from its current position
            currentGuestsCopy.RemoveAt(completedGuestIndex);
            // Insert the completed guest at the first position
            currentGuestsCopy.Insert(0, completedGuest);

            // Update the original list
            Managers.Grid.currentGuests = currentGuestsCopy;

            sequence.Join(completedGuestRect.DOAnchorPosX(targetX, 0.5f));
            // Move other guests to the right
            for (int i = 0; i < guestRects.Count; i++)
            {
                float newX = guestRects[i].anchoredPosition.x + (completedGuestRect.rect.width + spacing);
                sequence.Join(guestRects[i].DOAnchorPosX(newX, 0.5f));
            }
        }

        sequence.OnComplete(() =>
        {
            // Set sibling index for completed guest
            completedGuestRect.SetSiblingIndex(3);

            // Set sibling index for other guests
            for (int i = 0; i < guestRects.Count; i++)
            {
                guestRects[i].SetSiblingIndex(4 + i);
            }
        });

        sequence.Play();
    }
    #endregion

    public void OnClickRewardListButton()
    {
        ItemType RewardItemType = Managers.Game.GetItemData(Managers.Game.currentRewardQueue.Peek().id).type;
        if (RewardItemType == ItemType.Generatable)
        {
            if(Managers.Game.SpawnMoveGenerator(Managers.Game.currentRewardQueue.Peek().id, Managers.Game.currentRewardQueue.Peek().lv, rewardListButton.transform.position, (Vector2Int)Managers.Grid.GetEmptyPosition()))
            {
                Managers.Game.DequeueReward();
            };
        }
    }

    public void UpdateRewardList(Queue<ItemKey> rewardQueue)
    {
        if (rewardQueue.Count <= 0)
        {
            rewardList.SetActive(false);
            return;
        }
        else
        {
            rewardList.SetActive(true);
            ItemKey rewardKey = rewardQueue.Peek();
            rewardItmeImage.sprite = Managers.Game.GetItemData(rewardQueue.Peek().id).items[rewardQueue.Peek().lv - 1].itemSprite;
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
            else if (rewardQueue.Count == 3)
            {
                rewardCards[0].SetActive(true);
                rewardCards[1].SetActive(true);
                rewardCards[2].SetActive(true);

                countIcon.SetActive(false);
            }
            else
            {
                rewardCards[0].SetActive(true);
                rewardCards[1].SetActive(true);
                rewardCards[2].SetActive(true);
                countIcon.SetActive(true);
                if (rewardQueue.Count - 3 < 10)
                {
                    countText.text = $"{rewardQueue.Count - 3}";
                }
                else
                {
                    countText.text = "+9";
                }
            }
        }
    }
}
