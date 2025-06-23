

using UnityEngine;
using System.Collections.Generic;



[System.Serializable]
public class TutorialStep
{
    [Header("대화 설정")]
    public string dialogue; // 대사 배열

    [Header("캐릭터 위치")]
    public Vector2 characterPosition;
    public string characterName;

    [Header("강조 영역 설정")]
    public Vector2 highlightPosition; // 강조할 부분의 중심 위치
    public Vector2 highlightSize; // 강조할 부분의 크기

    [Header("손가락 설정")]
    public Vector2 fingerPosition; // 손가락 위치
    public float fingerRotation; // 손가락 각도
    public FingerAnimationType fingerAnimType; // 손가락 애니메이션 타입
    public float fingerAnimationAmount; // 손가락 애니메이션 이동 거리

    [Header("완료 조건")]
    public string completionCondition; // 완료 조건 식별자
}
[System.Serializable]
public class TutorialEvent
{
    public string eventId;
    [Header("대화 데이터")]
    public List<TutorialStep> dialogues = new List<TutorialStep>();

}

[CreateAssetMenu(fileName = "TutorialDatabase", menuName = "Game/Tutorial Database")]
public class TutorialDatabase : ScriptableObject
{
    public List<TutorialEvent> tutorialEvents = new List<TutorialEvent>();
}
