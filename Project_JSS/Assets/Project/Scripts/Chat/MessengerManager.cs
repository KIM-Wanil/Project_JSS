using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
[System.Serializable]
public class ChatMessage
{
    public enum MessageType
    {
        PlayerMessage,
        CharacterMessage,
        //SystemMessage,
        //Typing,
        //Action,
        //Image
    }

    public string senderId;
    public string senderName;
    public string messageText;
    public MessageType type;
    public Sprite senderProfileImage;
    public DateTime timestamp;
    public bool isRead;
}

[System.Serializable]
public class ChatContact
{
    public string contactId;
    public string contactName;
    public Sprite profileImage;
    public int unreadMessageCount;
    public List<ChatMessage> messageHistory;
    public CharacterProfile characterProfile;
    public bool isOnline;
    public DateTime lastActiveTime;
}

public class MessengerManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform chatListContent;
    public Transform messageListContent;
    public TMP_InputField messageInputField;
    public ScrollRect messageScrollView;
    public Button sendButton;
    //public TextMeshProUGUI contactNameText;
    //public Image contactProfileImage;

    [Header("Prefabs")]
    public GameObject playerMessagePrefab;
    public GameObject characterMessagePrefab;
    //public GameObject systemMessagePrefab;
    //public GameObject typingIndicatorPrefab;
    //public GameObject actionMessagePrefab;

    [Header("Contacts")]
    public List<ChatContact> contacts = new List<ChatContact>();
    private ChatContact currentSelectedContact;

    [Header("Settings")]
    public float typingDelay = 1.5f;
    public int maxMessageHistory = 100;

    private DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager = GetComponent<DialogueManager>();
        InitializeContacts();
        //UpdateContactList();
    }

    void InitializeContacts()
    {
        // 캐릭터들 추가
        //ChatContact Linaw = CreateChatContact(
        //    "Linaw",
        //    "리나",
        //    Resources.Load<Sprite>("Linaw"),
        //    dialogueManager.characters["리나"]
        //);
        //contacts.Add(Linaw);
        //currentSelectedContact = Linaw;
        // 다른 캐릭터들도 추가 가능
        ChatContact DemonKingPeng = CreateChatContact(
            "DemonkingPeng",
            "마왕 펭귄",
            Resources.Load<Sprite>("마왕 펭귄"),
            dialogueManager.characters["마왕 펭귄"]
        );
        contacts.Add(DemonKingPeng);
        currentSelectedContact = DemonKingPeng;
    }

    ChatContact CreateChatContact(string id, string name, Sprite profileImage, CharacterProfile characterProfile)
    {
        return new ChatContact
        {
            contactId = id,
            contactName = name,
            profileImage = profileImage,
            messageHistory = new List<ChatMessage>(),
            characterProfile = characterProfile,
            isOnline = true,
            lastActiveTime = DateTime.Now
        };
    }

    //public void UpdateContactList()
    //{
    //    // 연락처 목록 UI 업데이트
    //    foreach (Transform child in chatListContent)
    //    {
    //        Destroy(child.gameObject);
    //    }

    //    foreach (var contact in contacts)
    //    {
    //        GameObject contactItem = CreateContactListItem(contact);
    //        contactItem.transform.SetParent(chatListContent, false);
    //    }
    //}

    //GameObject CreateContactListItem(ChatContact contact)
    //{
    //    // 연락처 리스트 아이템 생성 로직
    //    GameObject itemObject = new GameObject(contact.contactName + "ListItem");

    //    // 프로필 이미지
    //    Image profileImage = itemObject.AddComponent<Image>();
    //    profileImage.sprite = contact.profileImage;

    //    // 이름 텍스트
    //    Text nameText = itemObject.AddComponent<Text>();
    //    nameText.text = contact.contactName;

    //    // 최근 메시지
    //    Text lastMessageText = itemObject.AddComponent<Text>();
    //    lastMessageText.text = contact.messageHistory.LastOrDefault()?.messageText ?? "새로운 대화";

    //    // 읽지 않은 메시지 수
    //    Text unreadCountText = itemObject.AddComponent<Text>();
    //    unreadCountText.text = contact.unreadMessageCount.ToString();
    //    unreadCountText.gameObject.SetActive(contact.unreadMessageCount > 0);

    //    // 클릭 이벤트
    //    Button contactButton = itemObject.AddComponent<Button>();
    //    contactButton.onClick.AddListener(() => SelectContact(contact));

    //    return itemObject;
    //}

    //public void SelectContact(ChatContact contact)
    //{
    //    currentSelectedContact = contact;

    //    // UI 업데이트
    //    contactNameText.text = contact.contactName;
    //    contactProfileImage.sprite = contact.profileImage;

    //    // 메시지 히스토리 로드
    //    LoadChatHistory(contact);

    //    // 읽지 않은 메시지 초기화
    //    contact.unreadMessageCount = 0;
    //    //UpdateContactList();
    //}

    void LoadChatHistory(ChatContact contact)
    {
        // 기존 메시지 제거
        foreach (Transform child in messageListContent)
        {
            Destroy(child.gameObject);
        }

        // 메시지 히스토리 로드
        foreach (var message in contact.messageHistory)
        {
            AddMessageToUI(message);
        }

        // 스크롤 최하단으로
        Canvas.ForceUpdateCanvases();
        messageScrollView.verticalNormalizedPosition = 0f;
    }
    public void OnSendButtonClick()
    {
        PrintMessage(messageInputField.text);
        Debug.Log($"{messageInputField.text} OnSendButtonClick");
        messageInputField.text = "";
    }
    public async void PrintMessage(string messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText) || currentSelectedContact == null)
            return;

        // 플레이어 메시지 생성
        ChatMessage playerMessage = new ChatMessage
        {
            senderId = "player",
            senderName = "플레이어",
            messageText = messageText,
            type = ChatMessage.MessageType.PlayerMessage,
            timestamp = DateTime.Now,
            isRead = true
        };

        // UI에 메시지 추가
        AddMessageToUI(playerMessage);

        // 대화 기록에 추가
        currentSelectedContact.messageHistory.Add(playerMessage);

        // 타이핑 표시
        //GameObject typingIndicator = AddTypingIndicator(currentSelectedContact);

        // Claude API 호출
        try
        {
            string characterResponse = await dialogueManager.GetResponseFromClaude(
                currentSelectedContact.contactName,
                messageText
            );

            // 타이핑 표시 제거
            //Destroy(typingIndicator);

            // 캐릭터 응답 메시지 생성
            ChatMessage characterMessage = new ChatMessage
            {
                senderId = currentSelectedContact.contactId,
                senderName = currentSelectedContact.contactName,
                messageText = characterResponse,
                type = ChatMessage.MessageType.CharacterMessage,
                senderProfileImage = currentSelectedContact.profileImage,
                timestamp = DateTime.Now,
                isRead = false
            };

            // UI에 메시지 추가
            AddMessageToUI(characterMessage);

            // 대화 기록에 추가
            currentSelectedContact.messageHistory.Add(characterMessage);

            // 읽지 않은 메시지 카운트 증가
            currentSelectedContact.unreadMessageCount++;

            // 연락처 목록 업데이트
            //UpdateContactList();
        }
        catch (Exception e)
        {
            Debug.LogError($"메시지 전송 중 오류: {e.Message}");
        }
    }

    GameObject AddMessageToUI(ChatMessage message)
    {
        GameObject messagePrefab = GetMessagePrefab(message.type);
        GameObject messageObject = Instantiate(messagePrefab, messageListContent);

        if(message.type == ChatMessage.MessageType.PlayerMessage)
        {
            TextMeshProUGUI messageText = messageObject.GetComponentInChildren<TextMeshProUGUI>();
            if (messageText != null)
            {
                messageText.text = message.messageText;
            }
        }
        else if(message.type == ChatMessage.MessageType.CharacterMessage)
        {
            CharacterChat characterChat = messageObject.GetComponent<CharacterChat>();
            // 프로필 이미지 설정
            Image profileImage = characterChat.profileImage;
            if (profileImage != null && message.senderProfileImage != null)
            {
                profileImage.sprite = message.senderProfileImage;
            }

            // 프로필 이름 설정
            TextMeshProUGUI characterChatName = characterChat.characterName;
            if (characterChatName != null)
            {
                characterChatName.text = message.senderName;
            }
            // 메시지 내용 설정
            TextMeshProUGUI characterChatMessage = characterChat.message;
            if (characterChatMessage != null)
            {
                characterChatMessage.text = message.messageText;
            }

            
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageObject.GetComponent<RectTransform>());
        return messageObject;
    }

    GameObject GetMessagePrefab(ChatMessage.MessageType type)
    {
        switch (type)
        {
            case ChatMessage.MessageType.PlayerMessage:
                return playerMessagePrefab;
            case ChatMessage.MessageType.CharacterMessage:
                return characterMessagePrefab;
            //case ChatMessage.MessageType.SystemMessage:
            //    return systemMessagePrefab;
            //case ChatMessage.MessageType.Typing:
            //    return typingIndicatorPrefab;
            //case ChatMessage.MessageType.Action:
            //    return actionMessagePrefab;
            //default:
            //    return systemMessagePrefab;
            default:
                return playerMessagePrefab;
        }
    }


    // 추가 기능: 대화 내보내기
    public void ExportChatHistory(ChatContact contact)
    {
        string exportPath = $"{Application.persistentDataPath}/{contact.contactName}_chat_history.txt";

        string chatHistory = $"대화 상대: {contact.contactName}\n";
        chatHistory += "---------------\n";

        foreach (var message in contact.messageHistory)
        {
            chatHistory += $"[{message.timestamp}] {message.senderName}: {message.messageText}\n";
        }

        System.IO.File.WriteAllText(exportPath, chatHistory);
        Debug.Log($"대화 기록이 {exportPath}에 저장되었습니다.");
    }

    // 추가 기능: 특정 키워드로 메시지 검색
    public List<ChatMessage> SearchMessages(string keyword)
    {
        return currentSelectedContact.messageHistory
            .Where(m => m.messageText.Contains(keyword))
            .ToList();
    }
}