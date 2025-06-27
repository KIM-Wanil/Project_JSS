using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DG.Tweening;
using UnityEditor;
using static UnityEngine.Rendering.DebugUI;




public class TutorialManager : MonoBehaviour
{
    [Header("UI 요소들")]
    //[SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private CanvasGroup tutorialPanel;
    [SerializeField] private Image overlayImage; // 검은색 오버레이
    [SerializeField] private RectTransform highlightRect; // 강조영역
    //public RectTransform characterDialogPrefab; // 캐릭터+대화상자 프리팹
    [SerializeField] private RectTransform fingerImage; // 손가락 이미지
    [SerializeField] private TutorialRaycastBlocker blocker; // 손가락 이미지
    private Tween fingerTween;


    [Header("튜토리얼 단계들")]
    [SerializeField] private TutorialDatabase tutorialDatabase;
    private TutorialStep[] currentTutorial;

    private int currentStepIndex = 0;
    private TutorialStep currentStep;
    [SerializeField] private RectTransform characterDialog;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject deaconPeng;
    [SerializeField] private GameObject maidPeng;

    private bool isTutorialActive = false;

    // 완료 조건 체크를 위한 이벤트
    public static event Action<TutorialCondition> OnConditionMet;

    private void Start()
    {
        // 완료 조건 이벤트 구독
        OnConditionMet += CheckCondition;

        // 처음에는 튜토리얼 비활성화
        tutorialPanel.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3) && !isTutorialActive)
        {
            StartTutorial("TUTORIAL1");
        }
        //if (Input.GetKeyDown(KeyCode.F2) && !isTutorialActive)
        //{
        //    StartTutorial("EVENT1");
        //}
        // 스페이스바로도 대화 진행 가능
        if (!isTutorialActive) return;
        if (Input.GetMouseButtonDown(0))
        {
            if (currentStep.completionCondition == TutorialCondition.None && currentStepIndex < currentTutorial.Length)
            {
                NextTutorialStep();
            }
        }

    }


    private void OnDestroy()
    {
        OnConditionMet -= CheckCondition;
    }

    public void StartTutorial(string tutorialId)
    {
        Debug.Log("StartTutorial");
        currentTutorial = tutorialDatabase.GetTutorialEvent(tutorialId)?.dialogues.ToArray();
        if (currentTutorial.Length == 0) return;
        Debug.Log(tutorialId + " 튜토리얼 시작");

        currentStepIndex = 0;
        tutorialPanel.gameObject.SetActive(true);
        blocker.ActivateBlockers();
        ShowTutorialStep(currentTutorial[currentStepIndex]);
        tutorialPanel.DOFade(1f, 0.5f).SetEase(Ease.InOutQuad).OnComplete(() => 
        {
            isTutorialActive = true;
        });
    }

    public void ShowTutorialStep(TutorialStep step)
    {
        currentStep = step;
        

   

        // 대화상자 위치 조정
        characterDialog.anchoredPosition = step.characterPosition;

        // 캐릭터 이미지 선택
        if (step.characterName == "집사 펭귄")
        {
            deaconPeng.SetActive(true);
            maidPeng.SetActive(false);
        }
        else if (step.characterName == "메이드 펭귄")
        {
            deaconPeng.SetActive(false);
            maidPeng.SetActive(true);
        }
        else
        {
            deaconPeng.SetActive(false);
            maidPeng.SetActive(false);
        }


        // 대화 텍스트 컴포넌트 찾기
        dialogueText = characterDialog.GetComponentInChildren<TextMeshProUGUI>();

        // 오버레이 설정 (강조 부분 제외한 검은색 마스크)
        SetupHilightArea(step.highlightPosition, step.highlightSize);

       

        //대사 표시
        ShowCurrentDialogue();
        // 손가락 위치 및 각도 설정
        if(step.fingerAnimType == FingerAnimationType.None)
        {
            fingerImage.gameObject.SetActive(false);
        }
        else
        {
            fingerImage.gameObject.SetActive(true);
            SetupFinger(step.fingerPosition, step.fingerRotation);
            AnimateFinger(step.fingerAnimType, step.fingerAnimationAmount);
        }
        
    }

    private void SetupHilightArea(Vector2 highlightPos, Vector2 highlightSize)
    {
        highlightRect.anchoredPosition = highlightPos;
        highlightRect.sizeDelta = highlightSize;
        blocker.UpdateBlockers(highlightPos, highlightSize);
    }

    private void SetupFinger(Vector2 position, float rotation)
    {
        fingerImage.anchoredPosition = position;
        fingerImage.rotation = Quaternion.Euler(0, 0, rotation);
        fingerImage.gameObject.SetActive(true);
    }

    private void ShowCurrentDialogue()
    {
        if (!string.IsNullOrEmpty(currentStep.dialogue))
        {
            dialogueText.text = currentStep.dialogue;
        }
    }

    


    private void AnimateFinger(FingerAnimationType inputFingerAnimType, float inputFingerAnimAmount)
    {
        // 기존 트윈 중지
        if (fingerTween != null && fingerTween.IsActive())
        {
            fingerImage.localScale = Vector3.one; // 초기화
            fingerTween.Kill();
            fingerTween = null;
        }
        switch (inputFingerAnimType)
        {
            case FingerAnimationType.Zoom:
                fingerTween = fingerImage.DOScale(0.8f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo);
                break;
            case FingerAnimationType.SwipeX:
                {
                    Vector2 originalPos = fingerImage.anchoredPosition;
                    float moveAmount = inputFingerAnimAmount;
                    float moveDuration = inputFingerAnimAmount * 0.01f;
                    fingerTween = fingerImage.DOAnchorPos(originalPos + Vector2.right * moveAmount, moveDuration)
                        .SetLoops(-1, LoopType.Restart)
                        .OnStepComplete(() => fingerImage.anchoredPosition = originalPos);
                }
                break;
            case FingerAnimationType.SwipeY:
                {
                    Vector2 originalPos = fingerImage.anchoredPosition;
                    float moveAmount = inputFingerAnimAmount;
                    float moveDuration = inputFingerAnimAmount * 0.01f;
                    fingerTween = fingerImage.DOAnchorPos(originalPos + Vector2.up * moveAmount, moveDuration)
                        .SetLoops(-1, LoopType.Restart)
                        .OnStepComplete(() => fingerImage.anchoredPosition = originalPos);
                }
                break;
            // 아무 애니메이션 없음
            case FingerAnimationType.None:               
            default:           
                break;
        }
    }

    public void OnHighlightAreaClicked()
    {
        if (!isTutorialActive) return;

        // 조건 완료 이벤트 발생
        OnConditionMet?.Invoke(currentStep.completionCondition);
    }

    private void CheckCondition(TutorialCondition condition)
    {
        if (!isTutorialActive) return;

        if (condition != currentStep.completionCondition) return;

            

        // 조건 완료, 다음 단계로 진행
        NextTutorialStep();
    }

    private void NextTutorialStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= currentTutorial.Length)
        {
            // 튜토리얼 완료
            EndTutorial();
        }
        else
        {
            // 다음 단계 표시
            ShowTutorialStep(currentTutorial[currentStepIndex]);
        }
    }

    private void EndTutorial()
    {   
        tutorialPanel.DOFade(0f, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            isTutorialActive = false;
            blocker.DeactivateBlockers();
            tutorialPanel.gameObject.SetActive(false);
        });
        Debug.Log("튜토리얼 완료!");
    }

    // 다른 스크립트에서 조건 완료를 알릴 때 사용
    public static void TriggerCondition(TutorialCondition condition)
    {
        OnConditionMet?.Invoke(condition);
    }
}
