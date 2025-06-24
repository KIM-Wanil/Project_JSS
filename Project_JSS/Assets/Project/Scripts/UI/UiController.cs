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
    [SerializeField] private TextMeshProUGUI[] starTexts;

    [SerializeField] private GameObject hotelCanvas;
    [SerializeField] private GameObject hotelUiCanvas;
    [SerializeField] private CanvasGroup mergeCanvas;
    void Start()
    {

        SetupButtonListeners();
        // GameManager의 골드 변경 이벤트 구독
        Managers.Game.onStarChanged.AddListener(UpdateStarUIs);
        // 초기 골드 UI 설정
        UpdateStarUIs(Managers.Game.CurrentStar);
    }
    public void SetupButtonListeners()
    {
        // UI 초기화
        toHotelButton.onClick.AddListener(onClickToHotelButton);
        toMergeButton.onClick.AddListener(onClickToMergeButton);
        shopButton.onClick.AddListener(onClickShopButton);
        listButton.onClick.AddListener(onClickListButton);

    }

    public void onClickToHotelButton()
    {
        hotelCanvas.SetActive(true);
        hotelUiCanvas.SetActive(true);
        mergeCanvas.gameObject.SetActive(false);
        //mergeCanvas.alpha = 0f;
        //mergeCanvas.interactable = false;
        //mergeCanvas.blocksRaycasts = false;
    }
    public void onClickToMergeButton()
    {
        TutorialManager.TriggerCondition(TutorialCondition.머지이동클릭);
        hotelCanvas.SetActive(false);
        hotelUiCanvas.SetActive(false);
        Managers.Grid.Init();
        Managers.Game.Init();
        mergeCanvas.gameObject.SetActive(true);
        //mergeCanvas.alpha = 1f;
        //mergeCanvas.interactable = true;
        //mergeCanvas.blocksRaycasts = true;
    }
    public void onClickShopButton()
    {

    }
    public void onClickListButton()
    {
        TutorialManager.TriggerCondition(TutorialCondition.제작목록클릭);
    }
    private void UpdateStarUIs(int currentStar)
    {
        foreach (var starText in starTexts)
        {
            if (starText != null)
            {
                starText.text = $"{currentStar}";
            }
        }
    }
}
