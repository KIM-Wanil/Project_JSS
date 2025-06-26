using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Data;
using DG.Tweening;
using System.Collections;
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
    [SerializeField] private RectTransform transitionScreen;
    [SerializeField] private float transitionDuration = 0.5f;
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
        //hotelCanvas.SetActive(true);
        //hotelUiCanvas.SetActive(true);
        //mergeCanvas.gameObject.SetActive(false);
        //mergeCanvas.alpha = 0f;
        //mergeCanvas.interactable = false;
        //mergeCanvas.blocksRaycasts = false;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transitionScreen.DOAnchorPos(new Vector2(0, -1280f), transitionDuration).SetEase(Ease.OutCubic));
        sequence.AppendCallback(() =>
        {
            hotelCanvas.SetActive(true);
            hotelUiCanvas.SetActive(true);
            mergeCanvas.gameObject.SetActive(false);
        });
        sequence.Append(transitionScreen.DOAnchorPos(new Vector2(0, 0f), transitionDuration).SetEase(Ease.Linear));
    }
    public void onClickToMergeButton()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transitionScreen.DOAnchorPos(new Vector2(0, -1280f), transitionDuration).SetEase(Ease.OutCubic));
        sequence.JoinCallback(() =>
        {
            TutorialManager.TriggerCondition(TutorialCondition.머지이동클릭);
            mergeCanvas.gameObject.SetActive(true);
            mergeCanvas.alpha = 0f;
            mergeCanvas.interactable = false;
            Managers.Grid.Init();
            //Managers.Game.Init();
            // 코루틴으로 대기 시작
            StartCoroutine(WaitForDataLoadedAndContinue(sequence));
        }).WaitForCompletion();
        sequence.AppendCallback(() =>
        {
            mergeCanvas.alpha = 1f;
            mergeCanvas.interactable = true;
            hotelCanvas.SetActive(false);
            hotelUiCanvas.SetActive(false);
        });
        sequence.Append(transitionScreen.DOAnchorPos(new Vector2(0, 0f), transitionDuration).SetEase(Ease.Linear));
    }
    private IEnumerator WaitForDataLoadedAndContinue(Sequence sequence)
    {
        // isDataLoaded가 true가 될 때까지 대기
        Debug.Log($"isInit:{Managers.Grid.isInit}");
        yield return new WaitUntil(() => Managers.Grid.isInit);

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
