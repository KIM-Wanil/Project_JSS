using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using System.Buffers;
[Serializable]
public class TopPanelViewer : MonoBehaviour
{
    [SerializeField] private ItemCollector goldItemCollector; 

    [SerializeField] private TextMeshProUGUI energyText; 
    [SerializeField] private TextMeshProUGUI energyRegenTimeText; 
    [SerializeField] private TextMeshProUGUI goldText; 
    [SerializeField] private TextMeshProUGUI gemText; 
    [SerializeField] private TextMeshProUGUI nickNameText; 
    // private TextMeshProUGUI maxEnergyText; 

    public void Awake()
    {
        //if(energyText)
        // energyText가 null일 때만 Find 수행
        if (!energyText)
        {
            energyText = GameObject.Find("EnergyAmountText").GetComponent<TextMeshProUGUI>();
        }
        if (!energyRegenTimeText)
        {
            energyText = GameObject.Find("EnergyRegenTimeText").GetComponent<TextMeshProUGUI>();
        }
        // goldText가 null일 때만 Find 수행
        if (!goldText)
        {
            goldText = GameObject.Find("GoldAmountText").GetComponent<TextMeshProUGUI>();
        }
        // goldText가 null일 때만 Find 수행
        if (!gemText)
        {
            gemText = GameObject.Find("GemAmountText").GetComponent<TextMeshProUGUI>();
        }
        // nickNameText가 null일 때만 Find 수행
        if (!nickNameText)
        {
            nickNameText = GameObject.Find("NicknameText").GetComponent<TextMeshProUGUI>();
        }
        // GameManager의 에너지 변경 이벤트 구독

    }

    private void Start()
    {
        // GameManager의 에너지 변경 이벤트 구독
        Managers.Game.onEnergyChanged.AddListener(UpdateEnergyUI);
        // 초기 에너지 UI 설정
        UpdateEnergyUI(Managers.Game.CurrentEnergy);

        // GameManager의 에너지 리젠 타임 변경 이벤트 구독
        Managers.Game.onEnergyRegenTimeChanged.AddListener(UpdateEnergyRegenTimeUI);
        // 초기 에너지 리젠 타임 UI 설정
        UpdateEnergyRegenTimeUI(Mathf.RoundToInt(Managers.Game.EnergyRegenRemainSec));

        // GameManager의 골드 변경 이벤트 구독
        Managers.Game.onGoldChanged.AddListener(UpdateGoldUI);
        // 초기 골드 UI 설정
        UpdateGoldUI(Managers.Game.CurrentGold);

        // GameManager의 골드 변경 이벤트 구독
        Managers.Game.onGemChanged.AddListener(UpdateGemUI);
        // 초기 골드 UI 설정
        UpdateGemUI(Managers.Game.CurrentGem);
    }

    //private void OnDestroy()
    //{
    //    // GameManager의 에너지 변경 이벤트 구독 해제
    //    if (Managers.Game != null)
    //    {
    //        Managers.Game.onEnergyChanged.RemoveListener(UpdateEnergyUI);
    //    }
    //}
    //닉네임 업데이트 메서드
    public void UpdateNickname()
    {
        Debug.Log("update nickname");
        //닉네임이 없으면 gamer_id를 출력하고, 닉네임이 있으면 닉네임 출력
        nickNameText.text = UserInfo.Data.nickname == null ?
                            UserInfo.Data.gamerId : UserInfo.Data.nickname;
    }
    // 에너지 UI 업데이트 메서드
    private void UpdateEnergyUI(int currentEnergy)
    {
        if (energyText)
        {
            energyText.text = $"{currentEnergy}";
        }
    }
    private void UpdateEnergyRegenTimeUI(int currentEnergyRegenTime)
    {
        if (energyRegenTimeText.IsUnityNull())
        {
            Debug.LogError("에너지 회복 시간 텍스트가 없습니다.");
            return;
        }

        if (Managers.Game.IsEnergyRegening)
        {
            energyRegenTimeText.gameObject.SetActive(true);
            TimeSpan timeSpan = TimeSpan.FromSeconds(currentEnergyRegenTime);
            energyRegenTimeText.text = $"+ {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        else
        {
            energyRegenTimeText.gameObject.SetActive(false);
        }
        
    }
    private void UpdateGoldUI(int currentEnergy)
    {
        if (goldText)
        {
            goldText.text = $"{currentEnergy}";
        }
    }
    private void UpdateGemUI(int currentGem)
    {
        if (gemText)
        {
            gemText.text = $"{currentGem}";
        }
    }



    //private void Start()
    //{
    //    mainCamera = Camera.main;
    //    UpdateLetterbox();
    //}

    //private StringBuilder sb = new StringBuilder();
    //public string ProcessText(string text)
    //{
    //    sb.Clear();
    //    sb.Append(text);

    //    //sb.Replace("<p>", Managers.Save.playerName);
    //    sb.Replace("<n>", "\n");
    //    sb.Replace("<c>", ",");

    //    return sb.ToString();
    //}

    //public void UpdateLetterbox()
    //{
    //    mainCamera = Camera.main;
    //    float windowAspect = (float)Screen.width / (float)Screen.height;
    //    float scaleHeight = windowAspect / targetAspect;

    //    if (scaleHeight < 1.0f)
    //    {
    //        Rect rect = mainCamera.rect;
    //        rect.width = 1.0f;
    //        rect.height = scaleHeight;
    //        rect.x = 0;
    //        rect.y = (1.0f - scaleHeight) / 2.0f;
    //        mainCamera.rect = rect;
    //    }
    //    else
    //    {
    //        float scaleWidth = 1.0f / scaleHeight;
    //        Rect rect = mainCamera.rect = new Rect(0, 0, 1, 1);
    //        rect.width = scaleWidth;
    //        rect.height = 1.0f;
    //        rect.x = (1.0f - scaleWidth) / 2.0f;
    //        rect.y = 0;
    //        mainCamera.rect = rect;
    //    }
    //}
    //private void OnRectTransformDimensionsChange()
    //{
    //    UpdateLetterbox();
    //}

}

