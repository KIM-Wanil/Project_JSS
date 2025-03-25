using Unity.VisualScripting;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEngine;
using UnityEngine.UI;

public class MergeEffect : MonoBehaviour
{
    public RectTransform rectTransform;
    [SerializeField] private Image[] effectImage;
    [SerializeField] private string[] effectTrigger;
    private Animator[] effectAnimator;


    //[SerializeField] private Image lightImage;
    //private Animator lightAnimator;
    //[SerializeField] private Image circleImage;
    //private Animator circleAnimator;
    //[SerializeField] private Image particleImage;
    //private Animator particleAnimator;

    public bool isEffectPlaying = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // effectAnimator 배열 초기화
        effectAnimator = new Animator[effectImage.Length];
        for (int i=0; i<effectImage.Length; i++)
        {
            if (!effectImage[i].IsUnityNull())
            {
                effectAnimator[i] = effectImage[i].GetComponent<Animator>();
                if (effectAnimator[i].IsUnityNull())
                {
                    Debug.LogError($"effectAnimator[{i}] is null");
                }
                if(string.IsNullOrEmpty(effectTrigger[i]))
                {
                    Debug.LogError($"effectTrigger[{i}] is null");
                }
            }
        }

        //if (!lightImage.IsUnityNull())
        //{
        //    lightAnimator = lightImage.GetComponent<Animator>();
        //    if(lightAnimator.IsUnityNull())
        //    {
        //        Debug.LogError("lightAnimator is null");
        //    }
        //}
        //if (!circleImage.IsUnityNull())
        //{
        //    circleAnimator = circleImage.GetComponent<Animator>();
        //    if (circleAnimator.IsUnityNull())
        //    {
        //        Debug.LogError("circleAnimator is null");
        //    }
        //}
        //if (!particleImage.IsUnityNull())
        //{
        //    particleAnimator = particleImage.GetComponent<Animator>();
        //    if (particleAnimator.IsUnityNull())
        //    {
        //        Debug.LogError("particleAnimator is null");
        //    }
        //}

        for (int i = 0; i < effectImage.Length; i++)
        {
            effectImage[i].enabled = false;
        }
        isEffectPlaying = false;
        //PlayTryMerge();
    }

    public void Init()
    {
        //this.gameObject.SetActive(false);

        for(int i=0; i < effectImage.Length; i++)
        {
            effectImage[i].enabled = false;
            effectAnimator[i].Rebind();
        }

        //lightImage.enabled = false;
        //circleImage.enabled = false;
        //particleImage.enabled = false;

        //particleAnimator.Rebind();
        //circleAnimator.Rebind();
        //lightAnimator.Rebind();

        isEffectPlaying = false;
    }
    public void PlayEffect()
    {
        if (isEffectPlaying) return;
        isEffectPlaying = true;
        //this.gameObject.SetActive(true);

        for (int i = 0; i < effectImage.Length; i++)
        {
            effectImage[i].enabled = true;
            effectAnimator[i].SetTrigger(effectTrigger[i]);
        }

        //lightImage.enabled = true;
        //circleImage.enabled = true;
        //particleImage.enabled = true;

        //particleAnimator.SetTrigger("PlayParticle");
        //circleAnimator.SetTrigger("PlayCircle");
        //lightAnimator.SetTrigger("PlayLight");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
