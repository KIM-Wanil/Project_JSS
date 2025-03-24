using Unity.VisualScripting;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEngine;
using UnityEngine.UI;

public class MergeEffect : MonoBehaviour
{
    public RectTransform rectTransform;
    [SerializeField] private Image lightImage;
    private Animator lightAnimator;
    [SerializeField] private Image circleImage;
    private Animator circleAnimator;
    [SerializeField] private Image tryParticleImage;
    private Animator tryParticleAnimator;

    public bool isTryAnimPlaying = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (!lightImage.IsUnityNull())
        {
            lightAnimator = lightImage.GetComponent<Animator>();
            if(lightAnimator.IsUnityNull())
            {
                Debug.LogError("successParticleAnimator is null");
            }
        }
        if (!circleImage.IsUnityNull())
        {
            circleAnimator = circleImage.GetComponent<Animator>();
            if (circleAnimator.IsUnityNull())
            {
                Debug.LogError("circleAnimator is null");
            }
        }
        if (!tryParticleImage.IsUnityNull())
        {
            tryParticleAnimator = tryParticleImage.GetComponent<Animator>();
            if (tryParticleAnimator.IsUnityNull())
            {
                Debug.LogError("tryParticleAnimator is null");
            }
        }

        Init();
        //PlayTryMerge();
    }

    public void Init()
    {
        //this.gameObject.SetActive(false);

        lightImage.enabled = false;
        circleImage.enabled = false;
        tryParticleImage.enabled = false;

        tryParticleAnimator.Rebind();
        circleAnimator.Rebind();
        lightAnimator.Rebind();

        isTryAnimPlaying = false;
    }
    public void PlayTryMerge()
    {
        if (isTryAnimPlaying) return;
        isTryAnimPlaying = true;
        //this.gameObject.SetActive(true);

        lightImage.enabled = true;
        circleImage.enabled = true;
        tryParticleImage.enabled = true;

        tryParticleAnimator.SetTrigger("PlayTryParticle");
        circleAnimator.SetTrigger("PlayCircle");
        lightAnimator.SetTrigger("PlayLight");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
