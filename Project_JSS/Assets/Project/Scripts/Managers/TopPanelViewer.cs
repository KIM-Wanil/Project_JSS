using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;
using TMPro;
[Serializable]
public class TopPanelViewer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI energyText; // TextMeshProUGUI 컴포넌트 추가
    [SerializeField] private TextMeshProUGUI goldText; // TextMeshProUGUI 컴포넌트 추가
    [SerializeField] private TextMeshProUGUI nickNameText; // TextMeshProUGUI 컴포넌트 추가
    // private TextMeshProUGUI maxEnergyText; // TextMeshProUGUI 컴포넌트 추가

    public void Awake()
    {
        //if(energyText)
        // energyText가 null일 때만 Find 수행
        if (!energyText)
        {
            energyText = GameObject.Find("EnergyAmountText").GetComponent<TextMeshProUGUI>();
        }

        // goldText가 null일 때만 Find 수행
        if (!goldText)
        {
            goldText = GameObject.Find("GoldAmountText").GetComponent<TextMeshProUGUI>();
        }
        // nickNameText가 null일 때만 Find 수행
        if (!nickNameText)
        {
            nickNameText = GameObject.Find("NicknameText").GetComponent<TextMeshProUGUI>();
        }
        // GameManager의 에너지 변경 이벤트 구독
        Managers.Game.onEnergyChanged.AddListener(UpdateEnergyUI);
        // 초기 에너지 UI 설정
        UpdateEnergyUI(Mathf.RoundToInt(Managers.Game.CurrentEnergy));

        // GameManager의 에너지 변경 이벤트 구독
        Managers.Game.onGoldChanged.AddListener(UpdateGoldUI);
        // 초기 에너지 UI 설정
        UpdateGoldUI(Managers.Game.CurrentGold);
        
        
    }

    private void Start()
    {
        
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
        if (!energyText)
        {
            energyText.text = $"{currentEnergy}";
        }
    }
    private void UpdateGoldUI(int currentEnergy)
    {
        if (!goldText)
        {
            goldText.text = $"{currentEnergy}";
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

