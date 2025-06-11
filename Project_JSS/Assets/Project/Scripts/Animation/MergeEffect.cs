using System.Collections;
using Unity.VisualScripting;//
//using UnityEditor.AdaptivePerformance.Editor;
using UnityEngine;
using UnityEngine.UI;

public class MergeEffect : MonoBehaviour
{
    [SerializeField] private float effectTime = 1f;
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
    }
    public void PlayEffectAtTime(float time)
    {
        if (isEffectPlaying) return;
        isEffectPlaying = true;

        for (int i = 0; i < effectImage.Length; i++)
        {
            effectImage[i].enabled = true;
            effectAnimator[i].Play(effectTrigger[i], -1, time / effectTime); // 트리거 대신 상태 이름을 사용하여 재생
        }
        Debug.Log($"PlayEffectAtTime: {time}");
        Debug.Log($"PlayEffectAtPercent: {time / effectTime}");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
