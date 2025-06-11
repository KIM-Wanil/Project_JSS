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
        percent = 0f;
        start = transform.position;
        duration = Random.Range(minDuration, maxDuration);
        point1 = Utils.GetNewPoint(start, Random.Range(0, 360), r);

        // duration 시간동안 서서히 사라짐
        image.DOFade(0.5f, duration);
    }

    private void Update()
    {
        if (percent >= 1f)
        {
            itemCollector.OnItemCollect(gameObject, goodsType);
            return;
        }

        UpdateEndPoint();
        percent += Time.deltaTime / duration;
        transform.position = Utils.QuadraticCurve(start, point1, end, percent);
    }

    private void UpdateEndPoint()
    {
        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, uiElement.position);

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            uiElement, screenPoint, mainCamera, out Vector3 worldPoint))
        {
            end = worldPoint;
        }
    }
}
