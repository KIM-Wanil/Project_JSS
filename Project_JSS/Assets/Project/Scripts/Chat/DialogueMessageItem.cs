using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DialogueMessageItem : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public Image backgroundImage;
    public Image npcImage;
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI dialogueText;
    public Button actionButton;
    public TextMeshProUGUI buttonText;

    [Header("색상 설정")]
    public Color npcMessageColor = new Color(1f, 1f, 1f, 0.9f);
    public Color playerMessageColor = new Color(0.7f, 0.9f, 1f, 0.9f);
    public Color systemMessageColor = new Color(0.9f, 0.9f, 0.7f, 0.9f);

    private DialogueData dialogueData;
    private bool isTyping = false;
    private Tween typingTween;

    public void SetupMessage(DialogueData dialogue, bool isLatest = false)
    {
        dialogueData = dialogue;

        // 배경색 설정
        SetBackgroundColor(dialogue.dialogueType);

        // NPC 이름 설정
        if (npcNameText != null)
            npcNameText.text = dialogue.npcName;

        // NPC 이미지 설정
        if (npcImage != null)
        {
            // DialogueUIManager에서 스프라이트를 가져와야 함
            npcImage.gameObject.SetActive(!string.IsNullOrEmpty(dialogue.npcSprite));
        }

        // 버튼 설정 (최신 메시지에만 표시)
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(isLatest && dialogue.nextDialogueId != -1);
            if (buttonText != null)
                buttonText.text = string.IsNullOrEmpty(dialogue.buttonText) ? "계속" : dialogue.buttonText;
        }

        // 텍스트 애니메이션
        if (isLatest)
        {
            StartTypingAnimation(dialogue.dialogueText);
        }
        else
        {
            dialogueText.text = dialogue.dialogueText;
        }

        // 등장 애니메이션
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    private void SetBackgroundColor(DialogueType type)
    {
        if (backgroundImage == null) return;

        Color targetColor = type switch
        {
            DialogueType.Answer => playerMessageColor,
            DialogueType.System => systemMessageColor,
            _ => npcMessageColor
        };

        backgroundImage.color = targetColor;
    }

    private void StartTypingAnimation(string text)
    {
        if (dialogueText == null) return;

        isTyping = true;
        dialogueText.text = "";

        // 버튼 비활성화
        if (actionButton != null)
            actionButton.interactable = false;

        // DOTween으로 타이핑 효과
        typingTween = DOTween.To(() => 0, value => {
            int charCount = Mathf.RoundToInt(value);
            if (charCount <= text.Length)
            {
                dialogueText.text = text.Substring(0, charCount);
            }
        }, text.Length, text.Length * 0.05f)
        .SetEase(Ease.Linear)
        .OnComplete(() => {
            isTyping = false;
            dialogueText.text = text;
            if (actionButton != null)
                actionButton.interactable = true;
        });
    }

    public void CompleteTyping()
    {
        if (isTyping && typingTween != null)
        {
            typingTween.Kill();
            dialogueText.text = dialogueData.dialogueText;
            isTyping = false;
            if (actionButton != null)
                actionButton.interactable = true;
        }
    }

    public bool IsTyping => isTyping;

    public DialogueData GetDialogueData() => dialogueData;
}
