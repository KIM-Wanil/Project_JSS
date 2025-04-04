using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RippleEffect : MonoBehaviour
{
    private Image rippleImage;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float maxScale = 2f;
    [SerializeField] private Color startColor = new Color(0.7f, 0.9f, 1f, 0.5f);
    [SerializeField] private Color endColor = new Color(0.7f, 0.9f, 1f, 0f);

    private void Awake()
    {
        rippleImage = GetComponent<Image>();
        rippleImage.color = startColor;
    }

    public void Start()
    {
        transform.localScale = Vector3.one;

        // 크기 확대와 투명도 감소를 동시에 실행
        Sequence sequence = DOTween.Sequence();

        sequence.Join(transform.DOScale(maxScale, duration).SetEase(Ease.OutQuad));
        sequence.Join(rippleImage.DOColor(endColor, duration).SetEase(Ease.OutQuad));

        sequence.OnComplete(() => {
            Destroy(gameObject);
        });
    }
}