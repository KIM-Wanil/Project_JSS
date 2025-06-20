using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DialogueManager : BaseManager
{
    [Header("UI 컴포넌트")]
    public GameObject dialoguePanel;
    public Image npcImage;
    public ScrollRect scrollRect;
    public Transform messageContent;
    public GameObject playerMessageItemPrefab;
    public GameObject npcMessageItemPrefab;
    public Image backgroundDimmer;

    [Header("애니메이션 설정")]
    public float panelAnimationDuration = 0.5f;
    public float dimmerAlpha = 0.8f;

    [Header("스크롤 설정")]
    public float scrollToBottomDuration = 0.3f;

    [Header("데이터베이스")]
    public DialogueDatabase dialogueDatabase;

    private List<DialogueMessageItem> messageItems = new List<DialogueMessageItem>();
    private DialogueEvent currentDialogueEvent;
    private bool isDialogueActive = false;

    public override void Init()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        // 배경 딤머 초기화
        if (backgroundDimmer != null)
        {
            backgroundDimmer.color = new Color(0, 0, 0, 0);
            backgroundDimmer.gameObject.SetActive(false);
        }
    }

    public void StartDialogue(string eventId)
    {
        Debug.Log("StartDialogue");
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

            // 패널 슬라이드 인 애니메이션
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
        // 배경 딤머 페이드 인



    }

    private void AddMessage(DialogueData dialogue, bool isLatest)
    {
        if (playerMessageItemPrefab == null || messageContent == null) return;

        // 이전 메시지들의 버튼 비활성화
        foreach (var item in messageItems)
        {
            if (item.nextArrow != null)
                item.nextArrow.gameObject.SetActive(false);
        }

        // 새 메시지 생성
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

        // 패널 슬라이드 아웃 애니메이션
        if (dialoguePanel != null)
        {
            RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
            panelRect.DOAnchorPosY(-Screen.height, panelAnimationDuration)
                .SetEase(Ease.InQuart)
                .OnComplete(() => {
                    dialoguePanel.SetActive(false);
                    panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, 0); // 초기 위치로 되돌리기
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
        if (Input.GetKeyDown(KeyCode.F1) && !isDialogueActive)
        {
            StartDialogue("CH1_EVENT0");
        }
        if (Input.GetKeyDown(KeyCode.F2) && !isDialogueActive)
        {
            StartDialogue("CH1_EVENT1");
        }
        // 스페이스바로도 대화 진행 가능
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButton(0)) && isDialogueActive && messageItems.Count > 0)
        {
            var lastMessage = messageItems[messageItems.Count - 1];
            if (lastMessage.nextArrow != null && lastMessage.nextArrow.gameObject.activeInHierarchy)
            {
                OnContinueButtonClick(lastMessage);
            }
        }

        //// ESC로 대화 종료
        //if (Input.GetKeyDown(KeyCode.Escape) && isDialogueActive)
        //{
        //    EndDialogue();
        //}
    }
}