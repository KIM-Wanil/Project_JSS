using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonAnimationHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private Button button;
    [SerializeField] private Image image;
    private RectTransform imageRectT;

    [Header("Animation Settings")]
    public bool useNormalAnimation = true;
    public bool useHighlightedAnimation = true;
    public bool usePressedAnimation = true;
    public bool useDisabledAnimation = true;
    public bool useSelectedAnimation = true;

    [Header("Animation Durations")]
    public float normalDuration = 0.1f;
    public float highlightedDuration = 0.1f;
    public float pressedDuration = 0.1f;
    public float disabledDuration = 0.1f;
    public float selectedDuration = 0.1f;

    private void Awake()
    {
        button = GetComponent<Button>();
        if(!image)
        {
            image = GetComponent<Image>();
        }
        imageRectT = image.GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.interactable && usePressedAnimation)
        {
            imageRectT.DOScale(0.95f, pressedDuration).SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button.interactable && usePressedAnimation)
        {
            // Normal 상태 애니메이션
            Sequence sequence = DOTween.Sequence();
            sequence.Append(imageRectT.DOScale(1.0f, 5f / 60f).SetEase(Ease.OutQuad));
            sequence.Append(imageRectT.DOScale(new Vector3(1.08f, 0.8f, 1f), 13f / 60f).SetEase(Ease.OutQuad))
                    .Append(imageRectT.DOScale(new Vector3(0.9f, 1.25f, 0.95f), 7f / 60f).SetEase(Ease.OutQuad))
                    .Append(imageRectT.DOScale(new Vector3(1.1f, 0.94f, 1f), 12f / 60f).SetEase(Ease.OutQuad))
                    .Append(imageRectT.DOScale(new Vector3(0.96f, 1f, 1f), 10f / 60f).SetEase(Ease.OutQuad))
                    .Append(imageRectT.DOScale(new Vector3(1f, 1f, 1f), 10f / 60f).SetEase(Ease.OutQuad));
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable && useHighlightedAnimation)
        {
            // Highlighted 상태 애니메이션
            imageRectT.DOScale(1.05f, highlightedDuration).SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button.interactable && useHighlightedAnimation)
        {
            // Normal 상태 애니메이션
            imageRectT.DOScale(1f, normalDuration).SetEase(Ease.OutQuad);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (button.interactable && useSelectedAnimation)
        {
            // Selected 상태 애니메이션
            imageRectT.DOScale(1.2f, selectedDuration).SetEase(Ease.OutQuad);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (button.interactable && useSelectedAnimation)
        {
            // Normal 상태 애니메이션
            imageRectT.DOScale(1f, normalDuration).SetEase(Ease.OutQuad);
        }
    }

    private void OnEnableButton()
    {
        if (useDisabledAnimation)
        {
            // Disabled 상태 애니메이션
            imageRectT.DOScale(1.0f, normalDuration).SetEase(Ease.OutQuad);
            image.DOColor(new Color(1.0f, 1.0f, 1.0f, 1.0f), disabledDuration).SetEase(Ease.OutQuad);
        }
    }

    private void OnDisableButton()
    {
        if (useDisabledAnimation)
        {
            // Normal 상태 애니메이션
            //rectTransform.DOScale(0.9f, normalDuration).SetEase(Ease.OutQuad);
            image.DOColor(new Color(0.5f, 0.5f, 0.5f, 1.0f), disabledDuration).SetEase(Ease.OutQuad);
        }
    }
}
