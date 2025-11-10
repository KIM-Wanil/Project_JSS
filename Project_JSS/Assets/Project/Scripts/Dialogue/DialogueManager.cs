using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DialogueManager : BaseManager
{
    [Header("UI ������Ʈ")]
    public GameObject dialoguePanel;
    public Image npcImage;
    public ScrollRect scrollRect;
    public Transform messageContent;
    public GameObject playerMessageItemPrefab;
    public GameObject npcMessageItemPrefab;
    public Image backgroundDimmer;

    [Header("�ִϸ��̼� ����")]
    public float panelAnimationDuration = 0.5f;
    public float dimmerAlpha = 0.8f;

    [Header("��ũ�� ����")]
    public float scrollToBottomDuration = 0.3f;

    [Header("�����ͺ��̽�")]
    public DialogueDatabase dialogueDatabase;

    private List<DialogueMessageItem> messageItems = new List<DialogueMessageItem>();
    private DialogueEvent currentDialogueEvent;
    private bool isDialogueActive = false;

    public override void Init()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (backgroundDimmer != null)
        {
            backgroundDimmer.color = new Color(0, 0, 0, 0);
            backgroundDimmer.gameObject.SetActive(false);
        }
    }

    public void StartDialogue(int num)
    {
        Debug.Log("StartDialogue");
        string eventId = $"EVENT{num}";
        DialogueEvent dialogueEvent = dialogueDatabase.GetDialogueEvent(eventId);
        currentDialogueEvent = dialogueEvent;
        if (currentDialogueEvent != null)
        {
            ShowDialogue();
        }
        else
        {
            Debug.LogError($"Dialogue event with ID '{eventId}' not found.");
        }
    }

    private void ShowDialogue()
    {
        Debug.Log("ShowDialogue");
        if (!isDialogueActive)
        {
            OpenDialoguePanel();
            ClearMessages();
        }

        
        if (npcImage != null)
        {
           Sprite npcSprite = dialogueDatabase.GetNPCSprite(currentDialogueEvent.dialogues[0].speakerName);
            if (npcSprite != null)
            {
                npcImage.sprite = npcSprite;
                npcImage.gameObject.SetActive(true);
            }
            else
            {
                npcImage.gameObject.SetActive(false);
            }
        }
        AddMessage(currentDialogueEvent.dialogues[0], true);
    }

    private void OpenDialoguePanel()
    {
        isDialogueActive = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);

            // �г� �����̵� �� �ִϸ��̼�
            RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
            Vector2 originalPos = panelRect.anchoredPosition;
            panelRect.anchoredPosition = new Vector2(originalPos.x, -Screen.height);

            panelRect.DOAnchorPosY(originalPos.y, panelAnimationDuration)
                .SetEase(Ease.OutQuart);
        }
        if (backgroundDimmer != null)
        {
            backgroundDimmer.gameObject.SetActive(true);
            backgroundDimmer.DOFade(dimmerAlpha, panelAnimationDuration);

        }
        // ��� ���� ���̵� ��



    }

    private void AddMessage(DialogueData dialogue, bool isLatest)
    {
        if (playerMessageItemPrefab == null || messageContent == null) return;

        // ���� �޽������� ��ư ��Ȱ��ȭ
        foreach (var item in messageItems)
        {
            if (item.nextArrow != null)
                item.nextArrow.gameObject.SetActive(false);
        }

        // �� �޽��� ����
        GameObject newMessageObj;
        if( dialogue.speakerName == "ME")
            newMessageObj = Instantiate(playerMessageItemPrefab, messageContent);
        else
        {
            newMessageObj = Instantiate(npcMessageItemPrefab, messageContent);
        }
        DialogueMessageItem messageItem = newMessageObj.GetComponent<DialogueMessageItem>();

        if (messageItem != null)
        {
            messageItem.SetupMessage(dialogue, isLatest);
            messageItems.Add(messageItem);
        }

        // ��ũ���� �� �Ʒ���
        DOVirtual.DelayedCall(0.1f, () => ScrollToBottom());
    }

    private void OnContinueButtonClick(DialogueMessageItem messageItem)
    {
        if (messageItem.IsTyping)
        {
            messageItem.CompleteTyping();
            return;
        }

        DialogueData dialogueData = messageItem.GetDialogueData();

        if (dialogueData.nextDialogueId == -1)
        {
            EndDialogue();
        }
        else
        {
            DialogueData nextDialogue = currentDialogueEvent.GetDialogue(dialogueData.nextDialogueId);
            if (nextDialogue != null)
            {
                AddMessage(nextDialogue, true);
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void ScrollToBottom()
    {
        if (scrollRect != null)
        {
            scrollRect.DOVerticalNormalizedPos(0f, scrollToBottomDuration)
                .SetEase(Ease.OutQuart);
        }
    }

    private void ClearMessages()
    {
        foreach (var item in messageItems)
        {
            if (item != null)
                DestroyImmediate(item.gameObject);
        }
        messageItems.Clear();
    }

    public void EndDialogue()
    {
        if (!isDialogueActive) return;

        isDialogueActive = false;

        // �г� �����̵� �ƿ� �ִϸ��̼�
        if (dialoguePanel != null)
        {
            RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
            panelRect.DOAnchorPosY(-Screen.height, panelAnimationDuration)
                .SetEase(Ease.InQuart)
                .OnComplete(() => {
                    dialoguePanel.SetActive(false);
                    panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, 0); // �ʱ� ��ġ�� �ǵ�����
                    ClearMessages();
                });
        }

        // ��� ���� ���̵� �ƿ�
        if (backgroundDimmer != null)
        {
            backgroundDimmer.DOFade(0f, panelAnimationDuration)
                .OnComplete(() => backgroundDimmer.gameObject.SetActive(false));
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && !isDialogueActive)
        {
            StartDialogue(0);
        }
        if (Input.GetKeyDown(KeyCode.F2) && !isDialogueActive)
        {
            StartDialogue(1);
        }
        if (Input.GetKeyDown(KeyCode.F3) && !isDialogueActive)
        {
            StartDialogue(2);
        }
        if (Input.GetKeyDown(KeyCode.F4) && !isDialogueActive)
        {
            StartDialogue(3);
        }
        if (Input.GetKeyDown(KeyCode.F5) && !isDialogueActive)
        {
            StartDialogue(4);
        }
        // �����̽��ٷε� ��ȭ ���� ����
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && isDialogueActive && messageItems.Count > 0)
        {
            var lastMessage = messageItems[messageItems.Count - 1];
            if (lastMessage.nextArrow != null && lastMessage.nextArrow.gameObject.activeInHierarchy)
            {
                OnContinueButtonClick(lastMessage);
            }
        }

        //// ESC�� ��ȭ ����
        //if (Input.GetKeyDown(KeyCode.Escape) && isDialogueActive)
        //{
        //    EndDialogue();
        //}
    }
}