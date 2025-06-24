using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Data;
public class UiController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Button toHotelButton;
    [SerializeField] private Button toMergeButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button listButton;
    [SerializeField] private TextMeshProUGUI starText;
    void Start()
    {
        if (!starText)
        {
            starText = GameObject.Find("StarAmountText").GetComponent<TextMeshProUGUI>();
        }
        SetupButtonListeners();
        // GameManager의 골드 변경 이벤트 구독
        Managers.Game.onStarChanged.AddListener(UpdateStarUI);
        // 초기 골드 UI 설정
        UpdateStarUI(Managers.Game.CurrentStar);
    }
    public void SetupButtonListeners()
    {
        // UI 초기화
        toHotelButton.onClick.AddListener(onClickToHotelButton);
        toHotelButton.onClick.AddListener(onClickToHotelButton);
        shopButton.onClick.AddListener(onClickShopButton);
        listButton.onClick.AddListener(onClickListButton);

    }
    
    public void onClickToHotelButton()
    {
        TutorialManager.TriggerCondition(TutorialCondition.머지이동클릭);
        TutorialManager.TriggerCondition(TutorialCondition.가구설치클릭);
    }
    public void onClickToMergeButton()
    {
        TutorialManager.TriggerCondition(TutorialCondition.머지이동클릭);
        TutorialManager.TriggerCondition(TutorialCondition.가구설치클릭);
    }
    public void onClickShopButton()
    {

    }
    public void onClickListButton()
    {
        TutorialManager.TriggerCondition(TutorialCondition.제작목록클릭);
    }
    private void UpdateStarUI(int currentStar)
    {
        if (starText)
        {
            starText.text = $"{currentStar}";
        }
    }
}
