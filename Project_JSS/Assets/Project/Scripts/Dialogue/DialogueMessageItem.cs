using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DialogueMessageItem : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public TextMeshProUGUI nameText;
    public RectTransform dialogueBox;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI tempDialogueText;
    public GameObject nextArrow;

    //[Header("색상 설정")]
    //public Color npcMessageColor = new Color(1f, 1f, 1f, 0.9f);
    //public Color playerMessageColor = new Color(0.7f, 0.9f, 1f, 0.9f);
    //public Color systemMessageColor = new Color(0.9f, 0.9f, 0.7f, 0.9f);

    private DialogueData dialogueData;
    private bool isTyping = false;
    private Tween typingTween;

    public void SetupMessage(DialogueData dialogue, bool isLatest = false)
    {
        dialogueData = dialogue;

        // 배경색 설정
        //SetBackgroundColor(dialogue.dialogueType);
        
        if(dialogue.speakerName == "ME")
        {
            if (nameText != null)
                nameText.text = string.IsNullOrEmpty(Managers.Backend.user.Data.nickname) ? "플레이어" : Managers.Backend.user.Data.nickname;
        }
        else
        {
            if (nameText != null)
                nameText.text = dialogue.speakerName;
        }
        tempDialogueText.text = dialogue.dialogueText;
        float height = LayoutUtility.GetPreferredHeight(tempDialogueText.rectTransform);
        dialogueText.rectTransform.sizeDelta = new Vector2(450f, height);
        Debug.Log(height);
        //// NPC 이미지 설정
        //if (npcImage != null)
        //{
        //    // DialogueUIManager에서 스프라이트를 가져와야 함
        //    npcImage.gameObject.SetActive(!string.IsNullOrEmpty(dialogue.npcSprite));
        //}

        // 버튼 설정 (최신 메시지에만 표시)
        if (nextArrow != null)
        {
            //nextArrow.SetActive(isLatest && dialogue.nextDialogueId != -1);
            nextArrow.SetActive(false);
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


    private void StartTypingAnimation(string text)
    {
        if (dialogueText == null) return;

        isTyping = true;
        dialogueText.text = "";



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
            LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueBox);
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
            if (nextArrow != null)
                nextArrow.SetActive(true);
        });
    }

    public void CompleteTyping()
    {
        if (isTyping && typingTween != null)
        {
            typingTween.Kill();
            dialogueText.text = dialogueData.dialogueText;
            LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueBox);
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
            isTyping = false;
            if (nextArrow != null)
                nextArrow.SetActive(true);
        }
    }

    public bool IsTyping => isTyping;

    public DialogueData GetDialogueData() => dialogueData;
}
