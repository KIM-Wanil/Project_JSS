using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public class ItemCollectEffect : MonoBehaviour
{
    public GoodsType goodsType; // 아이템 종류
    private Camera mainCamera;
    private Image image;
    private ItemCollector itemCollector;
    private RectTransform uiElement;
    private Vector3 start, end, point1;
    private float percent, duration;
    private readonly float minDuration = 0.5f, maxDuration = 1f, r = 3f;

    private void Awake()
    {
        mainCamera = Camera.main;
        image = GetComponent<Image>();
    }

    public void Setup(ItemCollector itemCollector, RectTransform uiElement)
    {
        this.itemCollector = itemCollector;
        this.uiElement = uiElement;
        duration = Random.Range(minDuration, maxDuration);
        start = transform.position;

        // 타겟 위치 계산 (UI의 월드 좌표)
        Vector3 targetWorldPos = uiElement.position;

        // 곡선 제어점 계산
        point1 = Utils.GetNewPoint(start, Random.Range(0, 360), 10f);

        // 3점(시작, 제어점, 끝점)으로 경로 배열 생성
        Vector3[] path = new Vector3[] { start, point1, targetWorldPos };

        // DOTween 곡선 이동
        image.DOFade(0.5f, duration);
        transform.DOPath(path, duration, PathType.CatmullRom)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                itemCollector.OnItemCollect(gameObject, goodsType);
            });
        //this.itemCollector = itemCollector;
        //this.uiElement = uiElement;
        //percent = 0f;
        //start = transform.position;
        //duration = Random.Range(minDuration, maxDuration);

        //// 타겟 위치 계산 (UI의 월드 좌표)
        //Vector3 targetWorldPos = uiElement.position;

        //// DOTween으로 이동 및 페이드
        //image.DOFade(0.5f, duration);
        //transform.DOMove(targetWorldPos, duration)
        //    .SetEase(Ease.InQuad)
        //    .OnComplete(() =>
        //    {
        //        itemCollector.OnItemCollect(gameObject, goodsType);
        //    });

        ////point1 = Utils.GetNewPoint(start, Random.Range(0, 360), r);

        ////// duration 시간동안 서서히 사라짐
        ////image.DOFade(0.5f, duration);
    }

    //private void Update()
    //{
    //    if (percent >= 1f)
    //    {
    //        itemCollector.OnItemCollect(gameObject, goodsType);
    //        return;
    //    }

    //    UpdateEndPoint();
    //    percent += Time.deltaTime / duration;
    //    transform.position = Utils.QuadraticCurve(start, point1, end, percent);
    //}

    //private void UpdateEndPoint()
    //{
    //    Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, uiElement.position);

    //    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
    //        uiElement, screenPoint, mainCamera, out Vector3 worldPoint))
    //    {
    //        end = worldPoint;
    //    }
    //}
}
