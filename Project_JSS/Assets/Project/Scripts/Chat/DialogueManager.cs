using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Rendering.VirtualTexturing;

[System.Serializable]
public class CharacterProfile
{
    public string characterName;
    public string personality;
    public string background;
    public string speechStyle;
    public List<string> interests;
    public Dictionary<string, int> relationships; // 다른 캐릭터와의 관계
    public int affectionLevel; // 플레이어에 대한 호감도

    // 호감도 레벨에 따른 캐릭터의 행동 변화
    public string GetAffectionDescription()
    {
        if (affectionLevel < 30) return "당신에게 무관심합니다.";
        else if (affectionLevel < 50) return "당신에게 약간의 관심을 보입니다.";
        else if (affectionLevel < 70) return "당신에게 호의적입니다.";
        else if (affectionLevel < 90) return "당신에게 깊은 애정을 느낍니다.";
        else return "당신에게 완전히 마음을 열었습니다.";
    }
}

// Claude API 요청 형식 수정
[Serializable]
public class ClaudeRequest
{
    public string model;
    public string system;  // 문자열로 정의
    public List<MessageRequest> messages;
    public float temperature;
    public int max_tokens;
    public bool stream = false; // 명시적으로 기본값 설정
}

[Serializable]
public class MessageRequest
{
    public string role;  // "user" 또는 "assistant"
    public List<ContentItem> content;  // 배열로 변경
}

[Serializable]
public class ContentItem
{
    public string type;  // "text"
    public string text;
}

// Claude API 응답 형식
[Serializable]
public class ClaudeResponse
{
    public string id;
    public string type;
    public long created_at;
    public string model;
    public List<ResponseContentItem> content;
    public string role;
    public string stop_reason;
    public Usage usage;
}

[Serializable]
public class ResponseContentItem
{
    public string type;
    public string text;
}

[Serializable]
public class Usage
{
    public int input_tokens;
    public int output_tokens;
}

// 대화 메시지 저장용 클래스 (내부 저장용)
[Serializable]
public class ConversationMessage
{
    public string role;
    public string content;
}

public class DialogueManager : MonoBehaviour
{
    private string apiKey = "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";
    private string apiUrl = "https://api.anthropic.com/v1/messages";

    public Dictionary<string, CharacterProfile> characters = new Dictionary<string, CharacterProfile>();
    private Dictionary<string, List<ConversationMessage>> conversationHistory = new Dictionary<string, List<ConversationMessage>>();
    private string currentGameState = "idle"; // idle, quest, battle, romance_event 등

    // 게임 상태 변화 감지를 위한 변수들
    private bool gameStateChanged = false;
    private bool affectionLevelChanged = false;
    private string lastGameState = "";
    private Dictionary<string, int> lastAffectionLevels = new Dictionary<string, int>();

    void Awake()
    {
        InitializeCharacters();
    }

    void InitializeCharacters()
    {
        CharacterProfile DemonKingPeng = new CharacterProfile
        {
            characterName = "마왕 펭귄",
            personality = "거만한, 중2병스러운, 암흑존재를 자처하는, 사실은 소심한",
            background = "‘암흑의 얼음’을 지배한다고 주장하며 수많은 차원을 떠돌아다니는 자칭 마왕. 실제로는 펭귄으로서의 본능과 사회성이 남아 있어 추위에 약하거나 사소한 일에도 쉽게 동요한다. 위엄을 유지하려고 늘 허세를 부리며 남들이 자신을 무시한다고 느끼면 극도로 예민해진다.현재 호텔 관리인(플레이어)이 꾸며준 '마왕의 안식처'라는 방에 만족하며 머무르고 있다.",
            interests = new List<string> { "흑마법", "어둠의 힘", "차원 여행", "멋있어 보이는 호칭", "펭귄용 간식" },
            speechStyle = "늘 자신을 마왕이라 칭하며 장엄하고 과장된 말투를 쓰지만 가끔 펭귄 특유의 귀여운 습관이 드러난다. 중요한 말마다 '후훗', '크하하', '<c>' 등의 느껴지는 기합을 넣으며 허세를 유지하려 한다.",
            affectionLevel = 45
        };
        characters.Add("마왕 펭귄", DemonKingPeng);
        conversationHistory.Add("마왕 펭귄", new List<ConversationMessage>());
    }

    // 호감도 변경 함수
    public void ChangeAffection(string characterName, int amount)
    {
        if (characters.ContainsKey(characterName))
        {
            characters[characterName].affectionLevel += amount;
            characters[characterName].affectionLevel = Mathf.Clamp(characters[characterName].affectionLevel, 0, 100);
            Debug.Log($"{characterName}의 호감도가 {amount}만큼 변화하여 {characters[characterName].affectionLevel}가 되었습니다.");
        }
    }

    // 게임 상태 업데이트
    public void UpdateGameState(string newState)
    {
        currentGameState = newState;
        Debug.Log($"게임 상태가 {newState}로 변경되었습니다.");
    }

    //API 호출 함수
    public async Task<string> GetResponseFromClaude(string characterName, string playerInput)
    {
        if (!characters.ContainsKey(characterName))
        {
            return "오류: 존재하지 않는 캐릭터입니다.";
        }
        CharacterProfile character = characters[characterName];

        // 대화 이력 가져오기 (내부 저장용)
        List<ConversationMessage> history = conversationHistory[characterName];
        bool isNewConversation = history.Count == 0 || gameStateChanged || affectionLevelChanged;
        if (isNewConversation)
        {
            history.Clear();
            gameStateChanged = false;
            affectionLevelChanged = false;
        }

        // API 요청용 메시지 포맷팅
        List<MessageRequest> apiMessages = new List<MessageRequest>();

        // 대화 이력 추가 (최대 10개 유지)
        int startIdx = Math.Max(0, history.Count - 10);
        for (int i = startIdx; i < history.Count; i++)
        {
            ConversationMessage historyMsg = history[i];
            MessageRequest apiMsg = new MessageRequest
            {
                role = historyMsg.role,
                content = new List<ContentItem>
                {
                    new ContentItem { type = "text", text = historyMsg.content }
                }
            };
            apiMessages.Add(apiMsg);
        }

        // 현재 사용자 메시지 추가
        MessageRequest userMessage = new MessageRequest
        {
            role = "user",
            content = new List<ContentItem>
            {
                new ContentItem { type = "text", text = playerInput }
            }
        };
        apiMessages.Add(userMessage);

        try
        {
            ClaudeRequest request2;
            if (isNewConversation)
            {
                request2 = new ClaudeRequest
                {
                    model = "claude-3-7-sonnet-20250219",
                    messages = apiMessages,
                    temperature = 0.7f,
                    max_tokens = 1000,
                    stream = false,
                    system = CreatePengCharacterPrompt(characterName)
                };
            }
            else
            {
                request2 = new ClaudeRequest
                {
                    model = "claude-3-7-sonnet-20250219",
                    messages = apiMessages,
                    temperature = 0.7f,
                    max_tokens = 1000,
                    stream = false,
                    system = ""
                    // system 필드를 포함하지 않음
                };
            }

            string jsonRequest = JsonConvert.SerializeObject(request2);
            Debug.Log($"Request JSON: {jsonRequest}");

            using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("x-api-key", apiKey);
                webRequest.SetRequestHeader("anthropic-version", "2023-06-01");

                // 비동기 요청 전송 및 완료 대기
                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Delay(100); // 100ms 간격으로 체크
                }

                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"API 요청 오류: {webRequest.error}");
                    Debug.LogError($"응답 내용: {webRequest.downloadHandler.text}");

                    // 오류 응답을 더 자세히 로깅
                    if (!string.IsNullOrEmpty(webRequest.downloadHandler.text))
                    {
                        try
                        {
                            var errorJson = JObject.Parse(webRequest.downloadHandler.text);
                            Debug.LogError($"오류 상세: {errorJson}");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"오류 응답 파싱 실패: {ex.Message}");
                        }
                    }

                    return "대화 오류가 발생했습니다. 잠시 후 다시 시도해주세요.";
                }
                else
                {
                    string responseJson = webRequest.downloadHandler.text;
                    Debug.Log($"API 응답: {responseJson}");

                    try
                    {
                        ClaudeResponse response = JsonUtility.FromJson<ClaudeResponse>(responseJson);

                        // Newtonsoft.Json을 사용한 대체 파싱 방법
                        if (response == null || response.content == null || response.content.Count == 0)
                        {
                            response = JsonConvert.DeserializeObject<ClaudeResponse>(responseJson);
                        }

                        if (response != null && response.content != null && response.content.Count > 0)
                        {
                            string responseText = response.content[0].text;

                            // 대화 이력에 메시지 추가 (내부 저장용)
                            history.Add(new ConversationMessage { role = "user", content = playerInput });
                            history.Add(new ConversationMessage { role = "assistant", content = responseText });

                            return responseText;
                        }
                        else
                        {
                            Debug.LogError("응답 내용이 비어 있습니다.");
                            Debug.LogError($"전체 응답: {responseJson}");
                            return "응답을 받을 수 없습니다.";
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"응답 처리 중 오류 발생: {ex.Message}");
                        Debug.LogError($"전체 응답: {responseJson}");
                        return "대화 처리 중 오류가 발생했습니다.";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"요청 준비 중 오류 발생: {ex.Message}");
            return "시스템 오류가 발생했습니다.";
        }
    }

    private string CreateLinawCharacterPrompt(string characterName)
    {
        CharacterProfile character = characters[characterName];
        StringBuilder prompt = new StringBuilder();
        prompt.AppendLine("당신은 이제부터 다음 캐릭터를 연기해주세요:");
        prompt.AppendLine($"캐릭터명: '{character.characterName}'");
        prompt.AppendLine($"역할: 판타지 세계의 모험가");
        prompt.AppendLine($"성격: {character.personality}");
        prompt.AppendLine($"배경 요약: {character.background}");
        prompt.AppendLine($"감정 상태: {character.GetAffectionDescription()}");
        prompt.AppendLine($"현재 상황: {currentGameState}");
        prompt.AppendLine($"대화 중인 상대방: 유스타스대륙, 갈바테인길드의 접수원(플레이어)");
        prompt.AppendLine("지침: 1인칭으로 1-3문장 응답, 판타지 세계관 유지, 행동지시문 없이");
        prompt.AppendLine("당신은 모험가 캐릭터로서 접수원(플레이어)과 대화해야 합니다.");
        return prompt.ToString();
    }
    private string CreatePengCharacterPrompt(string characterName)
    {
        CharacterProfile character = characters[characterName];
        StringBuilder prompt = new StringBuilder();
        prompt.AppendLine("당신은 이제부터 다음 캐릭터를 연기해주세요:");
        prompt.AppendLine($"캐릭터명: '{character.characterName}'");
        prompt.AppendLine($"역할: 여러 차원을 떠도는 자칭 마왕이자, 펭귄호텔의 특별 손님");
        prompt.AppendLine($"성격: {character.personality}");
        prompt.AppendLine($"배경 요약: {character.background}");
        prompt.AppendLine($"감정 상태: {character.GetAffectionDescription()}");
        prompt.AppendLine($"현재 상황: {currentGameState}");
        prompt.AppendLine("대화 중인 상대방: 펭귄호텔의 관리인(플레이어)");
        prompt.AppendLine("지침: 1인칭으로 1-3문장 응답, 행동지시문 없이");
        prompt.AppendLine("당신은 호텔에 머무는 마왕 캐릭터로서 관리인(플레이어)과 대화해야 합니다.");
        return prompt.ToString();
    }


    // 게임 상태 설정 메서드 - 상태가 변경될 때 플래그 설정
    public void SetGameState(string newState)
    {
        if (currentGameState != newState)
        {
            currentGameState = newState;
            gameStateChanged = true;
        }
    }

    // 호감도 변경 메서드 - 호감도가 변할 때 플래그 설정
    public void UpdateAffectionLevel(string characterName, int newLevel)
    {
        if (!lastAffectionLevels.ContainsKey(characterName))
        {
            lastAffectionLevels[characterName] = 0;
        }

        CharacterProfile character = characters[characterName];

        if (character.affectionLevel != newLevel)
        {
            // 호감도 단계가 바뀌었는지 확인 (30, 60, 80 기준점)
            bool thresholdChanged =
                (character.affectionLevel < 30 && newLevel >= 30) ||
                (character.affectionLevel < 60 && newLevel >= 60) ||
                (character.affectionLevel < 80 && newLevel >= 80) ||
                (character.affectionLevel >= 30 && newLevel < 30) ||
                (character.affectionLevel >= 60 && newLevel < 60) ||
                (character.affectionLevel >= 80 && newLevel < 80);

            character.affectionLevel = newLevel;
            lastAffectionLevels[characterName] = newLevel;

            if (thresholdChanged)
            {
                affectionLevelChanged = true;
            }
        }
    }

    // 게임 내 이벤트를 대화 컨텍스트에 추가
    public void AddEventToContext(string characterName, string eventDescription)
    {
        if (conversationHistory.ContainsKey(characterName))
        {
            ConversationMessage systemMessage = new ConversationMessage
            {
                role = "system",
                content = $"게임 이벤트: {eventDescription}. 이 상황을 인지하고 적절히 반응하세요."
            };

            conversationHistory[characterName].Add(systemMessage);
            Debug.Log($"{characterName}의 대화 컨텍스트에 이벤트 추가: {eventDescription}");
        }
    }

    // 대화 이력 저장 및 불러오기 기능
    public void SaveConversationHistory()
    {
        // JSON으로 변환하여 PlayerPrefs나 파일로 저장
        foreach (var character in conversationHistory.Keys)
        {
            string historyJson = JsonConvert.SerializeObject(conversationHistory[character]);
            PlayerPrefs.SetString($"ConversationHistory_{character}", historyJson);
        }
        PlayerPrefs.Save();
    }

    public void LoadConversationHistory()
    {
        foreach (var character in characters.Keys)
        {
            if (PlayerPrefs.HasKey($"ConversationHistory_{character}"))
            {
                string historyJson = PlayerPrefs.GetString($"ConversationHistory_{character}");
                conversationHistory[character] = JsonConvert.DeserializeObject<List<ConversationMessage>>(historyJson);
            }
        }
    }
}
