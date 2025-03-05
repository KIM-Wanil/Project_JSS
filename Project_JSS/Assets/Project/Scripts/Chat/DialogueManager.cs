using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

[System.Serializable]
public class CharacterProfile
{
    public string characterName;
    public string personality;
    public string background;
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

public class Message
{
    public string role;
    public string content;
}

[System.Serializable]
public class ClaudeRequest
{
    public string model = "claude-3-7-sonnet-20250219";
    public List<Message> messages;
    public double temperature = 0.7;
    public int max_tokens = 1000;
}

[System.Serializable]
public class ClaudeResponse
{
    public string id;
    public string type;
    public ContentObject content;
}

[System.Serializable]
public class ContentObject
{
    public List<ContentBlock> blocks;
}

[System.Serializable]
public class ContentBlock
{
    public string type;
    public string text;
}

public class DialogueManager : MonoBehaviour
{
    private string apiKey = "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";
    private string apiUrl = "https://api.anthropic.com/v1/messages";

    public Dictionary<string, CharacterProfile> characters = new Dictionary<string, CharacterProfile>();
    private Dictionary<string, List<Message>> conversationHistory = new Dictionary<string, List<Message>>();

    private string currentGameState = "idle"; // idle, quest, battle, romance_event 등

    void Awake()
    {
        InitializeCharacters();
    }

    void InitializeCharacters()
    {
        // 캐릭터 정보 초기화
        CharacterProfile Linaw = new CharacterProfile
        {
            characterName = "리나",
            personality = "차분하고 지적이며 마법에 대한 깊은 지식을 가진 마법사",
            background = "고대 마법 학교에서 수석으로 졸업했으며, 잃어버린 마법 서적을 찾아 여행 중입니다.",
            interests = new List<string> { "마법", "고대 문명", "책", "별자리" },
            relationships = new Dictionary<string, int> { { "로안", 60 }, { "세린", 80 } },
            affectionLevel = 30
        };

        characters.Add("리나", Linaw);
        conversationHistory.Add("리나", new List<Message>());

        // 다른 캐릭터들도 추가...
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

    // 대화를 Claude API에 전송하고 응답 받기
    public async Task<string> GetResponseFromClaude(string characterName, string playerInput)
    {
        if (!characters.ContainsKey(characterName))
        {
            return "오류: 존재하지 않는 캐릭터입니다.";
        }

        CharacterProfile character = characters[characterName];

        // 시스템 메시지 생성 (캐릭터 페르소나와 현재 상황 포함)
        string systemPrompt = CreateCharacterPrompt(characterName);

        // 대화 이력 가져오기
        List<Message> history = conversationHistory[characterName];

        // 시스템 메시지 추가
        List<Message> messages = new List<Message>
        {
            new Message { role = "system", content = systemPrompt }
        };

        // 대화 이력 추가 (최대 10개 유지)
        int startIdx = Math.Max(0, history.Count - 10);
        for (int i = startIdx; i < history.Count; i++)
        {
            messages.Add(history[i]);
        }

        // 플레이어 메시지 추가
        Message userMessage = new Message { role = "user", content = playerInput };
        messages.Add(userMessage);

        // API 요청 생성
        ClaudeRequest request = new ClaudeRequest
        {
            messages = messages
        };

        string jsonRequest = JsonConvert.SerializeObject(request);

        // API 호출
        using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("x-api-key", apiKey);
            webRequest.SetRequestHeader("anthropic-version", "2023-06-01");

            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
                return "대화 오류가 발생했습니다.";
            }
            else
            {
                string responseJson = webRequest.downloadHandler.text;
                ClaudeResponse response = JsonConvert.DeserializeObject<ClaudeResponse>(responseJson);

                if (response.content.blocks.Count > 0)
                {
                    string responseText = response.content.blocks[0].text;

                    // 대화 이력에 플레이어 메시지와 응답 추가
                    history.Add(userMessage);
                    history.Add(new Message { role = "assistant", content = responseText });

                    return responseText;
                }
                else
                {
                    return "응답을 받을 수 없습니다.";
                }
            }
        }
    }

    // 캐릭터의 현재 상태와 성격을 반영한 프롬프트 생성
    private string CreateCharacterPrompt(string characterName)
    {
        CharacterProfile character = characters[characterName];

        StringBuilder prompt = new StringBuilder();
        prompt.AppendLine($"당신은 로맨스 판타지 게임 속의 캐릭터 '{character.characterName}'입니다.");
        prompt.AppendLine($"성격: {character.personality}");
        prompt.AppendLine($"배경: {character.background}");
        prompt.AppendLine("관심사: " + string.Join(", ", character.interests));
        prompt.AppendLine($"현재 플레이어에 대한 감정: {character.GetAffectionDescription()}");
        prompt.AppendLine($"현재 게임 상태: {currentGameState}");

        // 호감도에 따른 대화 스타일 변경
        if (character.affectionLevel < 30)
        {
            prompt.AppendLine("당신은 플레이어에게 다소 냉담하고 거리를 두는 태도를 보입니다.");
        }
        else if (character.affectionLevel < 60)
        {
            prompt.AppendLine("당신은 플레이어에게 친절하지만 여전히 약간 경계심을 가지고 있습니다.");
        }
        else if (character.affectionLevel < 80)
        {
            prompt.AppendLine("당신은 플레이어에게 친밀하고 마음을 열기 시작했습니다.");
        }
        else
        {
            prompt.AppendLine("당신은 플레이어에게 매우 친밀하고 애정을 표현하는 것을 주저하지 않습니다.");
        }

        // 게임 상태에 따른 대화 컨텍스트 변경
        switch (currentGameState)
        {
            case "battle":
                prompt.AppendLine("현재 전투 중입니다. 긴장감 있고 짧은 대답이 적절합니다.");
                break;
            case "romance_event":
                prompt.AppendLine("현재 로맨스 이벤트 중입니다. 감정적이고 깊은 대화를 나눌 수 있습니다.");
                break;
            case "quest":
                prompt.AppendLine("현재 퀘스트 수행 중입니다. 임무에 집중하면서도 캐릭터의 개성을 드러내세요.");
                break;
            default:
                prompt.AppendLine("평소 일상적인 대화 중입니다.");
                break;
        }

        // 대화 지침
        prompt.AppendLine("\n대화 지침:");
        prompt.AppendLine("1. 항상 1인칭으로 대화하며, 자신이 게임 캐릭터라는 것을 인지하지 않습니다.");
        prompt.AppendLine("2. 대답은 간결하게 1-3문장으로 제한합니다.");
        prompt.AppendLine("3. 캐릭터의 개성과 현재 감정 상태를 반영하여 응답합니다.");
        prompt.AppendLine("4. 게임의 판타지 세계관을 유지하며 현실 세계의 기술이나 개념을 언급하지 않습니다.");

        return prompt.ToString();
    }

    // 게임 내 이벤트를 대화 컨텍스트에 추가
    public void AddEventToContext(string characterName, string eventDescription)
    {
        if (conversationHistory.ContainsKey(characterName))
        {
            Message systemMessage = new Message
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
                conversationHistory[character] = JsonConvert.DeserializeObject<List<Message>>(historyJson);
            }
        }
    }
}
