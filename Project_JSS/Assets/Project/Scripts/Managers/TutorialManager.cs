using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DG.Tweening;
using UnityEditor;




public class TutorialManager : MonoBehaviour
{
    [Header("UI 요소들")]
    [SerializeField] public GameObject tutorialCanvas;
    [SerializeField] public Image overlayImage; // 검은색 오버레이
    [SerializeField] public RectTransform highlightRect; // 강조영역
    //public RectTransform characterDialogPrefab; // 캐릭터+대화상자 프리팹
    [SerializeField] public RectTransform fingerImage; // 손가락 이미지
    private Tween fingerTween;
    

    [Header("튜토리얼 단계들")]
    [SerializeField] public TutorialStep[] tutorialSteps;

    private int currentStepIndex = 0;
    private TutorialStep currentStep;
    [SerializeField] private RectTransform characterDialog;
    [SerializeField] private TextMeshProUGUI dialogueText;
    private bool isTutorialActive = false;

    // 완료 조건 체크를 위한 이벤트
    public static event Action<string> OnConditionMet;

    private void Start()
    {
        // 완료 조건 이벤트 구독
        OnConditionMet += CheckCondition;

        // 처음에는 튜토리얼 비활성화
        tutorialCanvas.SetActive(false);
    }

    private void OnDestroy()
    {
        OnConditionMet -= CheckCondition;
    }

    public void StartTutorial()
    {
        if (tutorialSteps.Length == 0) return;

        currentStepIndex = 0;
        tutorialCanvas.SetActive(true);
        ShowTutorialStep(tutorialSteps[currentStepIndex]);
    }

    public void ShowTutorialStep(TutorialStep step)
    {
        currentStep = step;
        isTutorialActive = true;

   

        // 새 캐릭터 대화상자 생성
        //characterDialog = Instantiate(characterDialogPrefab, tutorialCanvas.transform);
        characterDialog.anchoredPosition = step.characterPosition;

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

    public void Update()
    {
        if (!isTutorialActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (string.IsNullOrEmpty(currentStep.completionCondition))
            {
                NextTutorialStep();
            }
        }
    }


    private void AnimateFinger(FingerAnimationType inputFingerAnimType, float inputFingerAnimAmount)
    {
        // 기존 트윈 중지
        if (fingerTween != null && fingerTween.IsActive())
        {
            fingerTween.Kill();
            fingerTween = null;
        }
        switch (inputFingerAnimType)
        {
            case FingerAnimationType.Zoom:
                fingerTween = fingerImage.DOScale(1.2f, 0.5f)
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

    private void CheckCondition(string condition)
    {
        if (!isTutorialActive) return;
        if (condition != currentStep.completionCondition) return;

        // 조건 완료, 다음 단계로 진행
        NextTutorialStep();
    }

    private void NextTutorialStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= tutorialSteps.Length)
        {
            // 튜토리얼 완료
            EndTutorial();
        }
        else
        {
            // 다음 단계 표시
            ShowTutorialStep(tutorialSteps[currentStepIndex]);
        }
    }

    private void EndTutorial()
    {
        tutorialCanvas.SetActive(false);

        // 정리 작업
        if (characterDialog != null)
        {
            Destroy(characterDialog.gameObject);
        }

        fingerImage.gameObject.SetActive(false);

        Debug.Log("튜토리얼 완료!");
    }

    // 다른 스크립트에서 조건 완료를 알릴 때 사용
    public static void TriggerCondition(string conditionId)
    {
        OnConditionMet?.Invoke(conditionId);
    }
}
