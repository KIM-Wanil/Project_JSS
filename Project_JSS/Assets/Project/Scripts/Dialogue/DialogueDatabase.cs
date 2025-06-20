

using UnityEngine;
using System.Collections.Generic;



// 1. 대화 데이터 구조체
[System.Serializable]
public class DialogueData
{
    public int dialogueId;
    public string speakerName;
    //public string npcSprite;
    public string dialogueText;
    public int nextDialogueId; // -1이면 대화 종료
    //public string buttonText;
    //public DialogueType dialogueType;
}

public enum DialogueType
{
    Normal,     // 일반 대화
    Question,   // 질문
    Answer,     // 답변
    System      // 시스템 메시지
}
[System.Serializable]
public class DialogueEvent
{
    public string eventId;
    [Header("대화 데이터")]
    public List<DialogueData> dialogues = new List<DialogueData>();
    public DialogueData GetDialogue(int dialogueId)
    {
        return dialogues.Find(d => d.dialogueId == dialogueId);
    }

}

[CreateAssetMenu(fileName = "DialogueDatabase", menuName = "Game/Dialogue Database")]
public class DialogueDatabase : ScriptableObject
{
    public List<DialogueEvent> dialogueEvents = new List<DialogueEvent>();

    [Header("NPC 스프라이트")]
    public List<NPCSpriteData> npcSprites = new List<NPCSpriteData>();

    public DialogueEvent GetDialogueEvent(string eventId)
    {
        return dialogueEvents.Find(d => d.eventId == eventId);
    }

    public Sprite GetNPCSprite(string spriteName)
    {
        var spriteData = npcSprites.Find(s => s.spriteName == spriteName);
        return spriteData?.sprite;
    }
}

[System.Serializable]
public class NPCSpriteData
{
    public string spriteName;
    public Sprite sprite;
}