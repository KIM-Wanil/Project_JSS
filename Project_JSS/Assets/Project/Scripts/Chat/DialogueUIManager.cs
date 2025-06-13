using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DialogueUIManager : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public GameObject dialoguePanel;
    public ScrollRect scrollRect;
    public Transform messageContainer;
    public GameObject messageItemPrefab;
    public Button closeButton;
    public Image backgroundDimmer;

    [Header("애니메이션 설정")]
    public float panelAnimationDuration = 0.5f;
    public float dimmerAlpha = 0.7f;

    [Header("스크롤 설정")]
    public float scrollToBottomDuration = 0.3f;

    [Header("데이터베이스")]
    public DialogueDatabase dialogueDatabase;

    private List<DialogueMessageItem> messageItems = new List<DialogueMessageItem>();
    private DialogueData currentDialogue;
    private bool isDialogueActive = false;

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(EndDialogue);

        // 배경 딤머 초기화
        if (backgroundDimmer != null)
        {
            backgroundDimmer.color = new Color(0, 0, 0, 0);
            backgroundDimmer.gameObject.SetActive(false);
        }
    }

    public void StartDialogue(int dialogueId)
    {
        DialogueData dialogue = dialogueDatabase.GetDialogue(dialogueId);
        if (dialogue != null)
        {
            ShowDialogue(dialogue);
        }
    }

    public void StartDialogue(string npcName)
    {
        var dialogues = dialogueDatabase.GetNPCDialogues(npcName);
        if (dialogues.Count > 0)
        {
            ShowDialogue(dialogues[0]);
        }
    }

    private void ShowDialogue(DialogueData dialogue)
    {
        if (!isDialogueActive)
        {
            OpenDialoguePanel();
            ClearMessages();
        }

        currentDialogue = dialogue;
        AddMessage(dialogue, true);
    }

    private void OpenDialoguePanel()
    {
        isDialogueActive = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);

            // 패널 슬라이드 인 애니메이션
            RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
            Vector2 originalPos = panelRect.anchoredPosition;
            panelRect.anchoredPosition = new Vector2(originalPos.x, -Screen.height);

            panelRect.DOAnchorPosY(originalPos.y, panelAnimationDuration)
                .SetEase(Ease.OutQuart);
        }

        // 배경 딤머 페이드 인
        if (backgroundDimmer != null)
        {
            backgroundDimmer.gameObject.SetActive(true);
            backgroundDimmer.DOFade(dimmerAlpha, panelAnimationDuration);
        }
    }

    private void AddMessage(DialogueData dialogue, bool isLatest)
    {
        if (messageItemPrefab == null || messageContainer == null) return;

        // 이전 메시지들의 버튼 비활성화
        foreach (var item in messageItems)
        {
            if (item.actionButton != null)
                item.actionButton.gameObject.SetActive(false);
        }

        // 새 메시지 생성
        GameObject newMessageObj = Instantiate(messageItemPrefab, messageContainer);
        DialogueMessageItem messageItem = newMessageObj.GetComponent<DialogueMessageItem>();

        if (messageItem != null)
        {
            // NPC 스프라이트 설정
            if (messageItem.npcImage != null)
            {
                Sprite npcSprite = dialogueDatabase.GetNPCSprite(dialogue.npcSprite);
                if (npcSprite != null)
                {
                    messageItem.npcImage.sprite = npcSprite;
                    messageItem.npcImage.gameObject.SetActive(true);
                }
                else
                {
                    messageItem.npcImage.gameObject.SetActive(false);
                }
            }

            messageItem.SetupMessage(dialogue, isLatest);

            // 버튼 이벤트 설정
            if (messageItem.actionButton != null && isLatest)
            {
                messageItem.actionButton.onClick.RemoveAllListeners();
                messageItem.actionButton.onClick.AddListener(() => OnContinueButtonClick(messageItem));
            }

            messageItems.Add(messageItem);
        }

        // 스크롤을 맨 아래로
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
            DialogueData nextDialogue = dialogueDatabase.GetDialogue(dialogueData.nextDialogueId);
            if (nextDialogue != null)
            {
                ShowDialogue(nextDialogue);
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

        // 패널 슬라이드 아웃 애니메이션
        if (dialoguePanel != null)
        {
            RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
            panelRect.DOAnchorPosY(-Screen.height, panelAnimationDuration)
                .SetEase(Ease.InQuart)
                .OnComplete(() => {
                    dialoguePanel.SetActive(false);
                    ClearMessages();
                });
        }

        // 배경 딤머 페이드 아웃
        if (backgroundDimmer != null)
        {
            backgroundDimmer.DOFade(0f, panelAnimationDuration)
                .OnComplete(() => backgroundDimmer.gameObject.SetActive(false));
        }
    }

    private void Update()
    {
        // 스페이스바로도 대화 진행 가능
        if (Input.GetKeyDown(KeyCode.Space) && isDialogueActive && messageItems.Count > 0)
        {
            var lastMessage = messageItems[messageItems.Count - 1];
            if (lastMessage.actionButton != null && lastMessage.actionButton.gameObject.activeInHierarchy)
            {
                OnContinueButtonClick(lastMessage);
            }
        }

        // ESC로 대화 종료
        if (Input.GetKeyDown(KeyCode.Escape) && isDialogueActive)
        {
            EndDialogue();
        }
    }
}