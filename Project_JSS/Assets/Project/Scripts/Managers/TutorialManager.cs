using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DG.Tweening;
//using UnityEditor;





public class TutorialManager : MonoBehaviour
{
    public static int currentTutorialNum = 0;
    [Header("UI")]
    [SerializeField] private CanvasGroup tutorialPanel;
    [SerializeField] private Image overlayImage; // ������ ��������
    [SerializeField] private RectTransform highlightRect; // ��������
    [SerializeField] private RectTransform fingerImage; // �հ��� �̹���
    [SerializeField] private TutorialRaycastBlocker blocker; // �հ��� �̹���
    [SerializeField] private TutorialDatabase tutorialDatabase;
    [SerializeField] private RectTransform characterDialog;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject deaconPeng;
    [SerializeField] private GameObject maidPeng;

    private TutorialStep[] currentTutorial;
    private Tween fingerTween;
    private int currentStepIndex = 0;
    private TutorialStep currentStep;
    private bool isTutorialActive = false;

    // �Ϸ� ���� üũ�� ���� �̺�Ʈ
    public static event Action<TutorialCondition> OnTriggerCondition;
    public static event Action<int> OnStartTutorial;

    private void Start()
    {
        // �Ϸ� ���� �̺�Ʈ ����
        OnTriggerCondition += CheckCondition;
        OnStartTutorial += StartTutorial;
        // ó������ Ʃ�丮�� ��Ȱ��ȭ
        tutorialPanel.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6) && !isTutorialActive)
        {
            StartTutorial(0);
        }
        if (Input.GetKeyDown(KeyCode.F7) && !isTutorialActive)
        {
            StartTutorial(1);
        }
        //if (Input.GetKeyDown(KeyCode.F2) && !isTutorialActive)
        //{
        //    StartTutorial("EVENT1");
        //}
        // �����̽��ٷε� ��ȭ ���� ����
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
        OnTriggerCondition -= CheckCondition;
        OnStartTutorial -= StartTutorial;
    }
    public void StartTutorial(int num)
    {
        Debug.Log("StartTutorial");
        string tutorialId = $"TUTORIAL{num}";
        currentTutorial = tutorialDatabase.GetTutorialEvent(tutorialId)?.dialogues.ToArray();
        if (currentTutorial.Length == 0) return;
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
        

   

        // ��ȭ���� ��ġ ����
        characterDialog.anchoredPosition = step.characterPosition;

        // ĳ���� �̹��� ����
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


        // ��ȭ �ؽ�Ʈ ������Ʈ ã��
        dialogueText = characterDialog.GetComponentInChildren<TextMeshProUGUI>();

        // �������� ���� (���� �κ� ������ ������ ����ũ)
        SetupHilightArea(step.highlightPosition, step.highlightSize);

       

        //��� ǥ��
        ShowCurrentDialogue();
        // �հ��� ��ġ �� ���� ����
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
        // ���� Ʈ�� ����
        if (fingerTween != null && fingerTween.IsActive())
        {
            fingerImage.localScale = Vector3.one; // �ʱ�ȭ
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
            // �ƹ� �ִϸ��̼� ����
            case FingerAnimationType.None:               
            default:           
                break;
        }
    }

    public void OnHighlightAreaClicked()
    {
        if (!isTutorialActive) return;

        // ���� �Ϸ� �̺�Ʈ �߻�
        OnTriggerCondition?.Invoke(currentStep.completionCondition);
    }

    private void CheckCondition(TutorialCondition condition)
    {
        if (!isTutorialActive) return;

        if (condition != currentStep.completionCondition) return;

            

        // ���� �Ϸ�, ���� �ܰ�� ����
        NextTutorialStep();
    }

    private void NextTutorialStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= currentTutorial.Length)
        {
            // Ʃ�丮�� �Ϸ�
            EndTutorial();
        }
        else
        {
            // ���� �ܰ� ǥ��
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
        for (int i = 0; i < 5; i++)
        {
            Managers.Game.CreateRandomGuest();
        }
        //if (currentTutorialNum == 1)
        //{
        //    for (int i = 0; i < 5; i++)
        //    {
        //        Managers.Game.CreateRandomGuest();
        //    }
        //}
        currentTutorialNum += 1;

        if (currentTutorialNum == 2)
        {
            DialogueManager.OnStartDialogueEvent(3);
        }
    }

    // �ٸ� ��ũ��Ʈ���� ���� �ϷḦ �˸� �� ���
    public static void OnTriggerConditionEvent(TutorialCondition condition)
    {
        OnTriggerCondition?.Invoke(condition);
    }
    public static void OnStartTutorialEvent()
    {
        OnStartTutorial?.Invoke(currentTutorialNum);
    }
}
