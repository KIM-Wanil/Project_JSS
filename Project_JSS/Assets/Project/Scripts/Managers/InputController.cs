using System.Collections;
using UnityEngine;
using DG.Tweening;
public class InputController : MonoBehaviour
{
    [Tooltip("입력 비활성 시간 (초)")]
    public float inactivityTime = 3.0f;

    [Tooltip("코루틴 실행 시간 (초)")]
    public float coroutineExecutionTime = 3.0f;

    [Tooltip("디버그 로그 출력 여부")]
    public bool showDebugLogs = true;

    private Vector3 lastMousePosition;
    private float timer = 0f;
    private bool isCoroutineRunning = false;
    private bool isMobileDevice = false;
    private bool wasInputDetected = false;

    DG.Tweening.Sequence announceMovingSequence;
    private MergeableItem itemToAnnounce1;
    private MergeableItem itemToAnnounce2;

    private IEnumerator timerCoroutine;
    void Start()
    {
        // 현재 실행 환경이 모바일인지 확인
        isMobileDevice = Application.isMobilePlatform;

        if (showDebugLogs)
        {
            Debug.Log($"현재 플랫폼: {(isMobileDevice ? "모바일" : "PC")}");
        }

        // PC 환경이라면 초기 마우스 위치를 저장
        if (!isMobileDevice)
        {
            lastMousePosition = Input.mousePosition;
        }

        timerCoroutine = InactivityCoroutine(Vector2Int.zero, Vector2Int.zero);
    }

    void Update()
    {
        if (isMobileDevice)
        {
            // 모바일 환경에서는 터치 입력 감지
            CheckMobileInput();
        }
        else
        {
            // PC 환경에서는 마우스 입력 감지
            CheckPCInput();
        }
    }

    void CheckPCInput()
    {
        // 마우스 버튼 클릭이나 마우스 움직임 감지
        bool mouseInput = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);
        bool mouseMoved = lastMousePosition != Input.mousePosition;

        // 키보드 입력 감지 (아무 키나)
        bool keyboardInput = Input.anyKey;

        if (mouseInput || mouseMoved || keyboardInput)
        {
            // 입력이 감지되었으므로 타이머 초기화
            ResetTimer();
            lastMousePosition = Input.mousePosition;
            wasInputDetected = true;
        }
        else if (wasInputDetected || timer > 0)
        {
            // 입력이 없으면 타이머 증가
            timer += Time.deltaTime;
            wasInputDetected = false;

            CheckInactivity();
        }


    }

    void CheckMobileInput()
    {
        // 터치 입력 감지
        bool touchInput = Input.touchCount > 0;

        if (touchInput)
        {
            // 터치가 감지되었으므로 타이머 초기화
            //ResetTimer();
            wasInputDetected = true;
        }
        else if (wasInputDetected || timer > 0)
        {
            // 터치가 없으면 타이머 증가
            timer += Time.deltaTime;
            wasInputDetected = false;

            CheckInactivity();
        }
    }

    void CheckInactivity()
    {
        // 설정된 비활성 시간이 지나고 코루틴이 실행 중이 아니면 코루틴 시작
        if (timer >= inactivityTime && !isCoroutineRunning)
        {
            (Vector2Int, Vector2Int)? mergeablePosPair = Managers.Grid.FindMergeablePosPair();
            if (mergeablePosPair.HasValue)
            {
                StartCoroutine(InactivityCoroutine(mergeablePosPair.Value.Item1, mergeablePosPair.Value.Item2));
                isCoroutineRunning = true;
                if (showDebugLogs)
                {
                    Debug.Log($"{inactivityTime}초 동안 입력 비활성 감지: 코루틴 시작됨");
                }
            }            
        }
    }

    void ResetTimer()
    {
        timer = 0f;
        ResetItemsPosition();
        // 코루틴이 실행 중이면 중지
        if (isCoroutineRunning)
        {
            StopCoroutine("InactivityCoroutine");
            announceMovingSequence.Kill();
            
            isCoroutineRunning = false;
            Debug.Log("입력 감지: 코루틴 중지됨");
        }
    }

    IEnumerator InactivityCoroutine(Vector2Int pos1, Vector2Int pos2)
    {
        Debug.Log("PerformMovementEffect");
        itemToAnnounce1 = Managers.Grid.GetItemAt(pos1);
        if (itemToAnnounce1 == null)
        {
            Debug.LogError("itemToAnnounce1 is null");
            yield break;
        }
        itemToAnnounce2 = Managers.Grid.GetItemAt(pos2);
        if (itemToAnnounce2 == null)
        {
            Debug.LogError("itemToAnnounce2 is null");
            yield break;
        }

        Vector2 tilePos1 = Managers.Grid.GetTilePosition(pos1);
        Vector2 tilePos2 = Managers.Grid.GetTilePosition(pos2);

        Vector2 direction1 = (tilePos2 - tilePos1).normalized * 10f;
        Vector2 direction2 = (tilePos1 - tilePos2).normalized * 10f;

        Debug.Log($"tilePos1:{tilePos1}/tilePos2:{tilePos2}/direction1:{direction1}/direction2:{direction2}");
        if (announceMovingSequence != null)
        {
            announceMovingSequence.Kill();
        }
        announceMovingSequence = DG.Tweening.DOTween.Sequence();

        announceMovingSequence.Append(itemToAnnounce1.itemImage.rectTransform.DOAnchorPos(direction1, 0.5f));
        announceMovingSequence.Join(itemToAnnounce2.itemImage.rectTransform.DOAnchorPos(direction2, 0.5f));
        announceMovingSequence.Append(itemToAnnounce1.itemImage.rectTransform.DOAnchorPos(Vector2.zero, 0.5f));
        announceMovingSequence.Join(itemToAnnounce2.itemImage.rectTransform.DOAnchorPos(Vector2.zero, 0.5f));

        announceMovingSequence.Append(itemToAnnounce1.itemImage.rectTransform.DOAnchorPos(direction1, 0.5f));
        announceMovingSequence.Join(itemToAnnounce2.itemImage.rectTransform.DOAnchorPos(direction2, 0.5f));
        announceMovingSequence.Append(itemToAnnounce1.itemImage.rectTransform.DOAnchorPos(Vector2.zero, 0.5f));
        announceMovingSequence.Join(itemToAnnounce2.itemImage.rectTransform.DOAnchorPos(Vector2.zero, 0.5f));

        announceMovingSequence.AppendInterval(1.0f);
        announceMovingSequence.SetLoops(-1);
        announceMovingSequence.Play();

        // 여기에 입력 비활성 시 실행할 코드를 넣으세요
        // 예시: 지정된 시간 동안 실행되는 작업
        while (!wasInputDetected)
        {
            // 코루틴에서 수행할 작업을 여기에 추가하세요
            // 예: UI 표시, 게임 상태 변경, 애니메이션 재생 등

            yield return null;
        }
        Debug.Log("코루틴 종료1");
        //ResetTimer();
        yield break;
        //announceMovingSequence.Kill();
        //ResetItemsPosition();
        //isCoroutineRunning = false;
    }
    private void ResetItemsPosition()
    {
        //Debug.Log($"ResetItemsPosition {itemToAnnounce1},{itemToAnnounce2}");
        if (itemToAnnounce1 != null && itemToAnnounce2 != null)
        {
            itemToAnnounce1.itemImage.rectTransform.anchoredPosition = Vector3.zero;
            itemToAnnounce2.itemImage.rectTransform.anchoredPosition = Vector3.zero;

            //Debug.Log($"ItemsPosition {itemToAnnounce1.itemRectT.anchoredPosition},{itemToAnnounce2.itemRectT.anchoredPosition}");

            itemToAnnounce1 = null;
            itemToAnnounce2 = null;
        }
    }
}