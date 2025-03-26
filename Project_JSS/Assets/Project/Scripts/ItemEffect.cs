using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemEffect : MonoBehaviour
{
    [SerializeField] private Image successParticleImage;
    private Animator successParticleAnimator;
    private void Awake()
    {
        if (!successParticleImage.IsUnityNull())
        {
            successParticleAnimator = successParticleImage.GetComponent<Animator>();
            if(successParticleAnimator.IsUnityNull())
            {
                Debug.LogError("successParticleAnimator is null");
            }
        }
        //successParticleImage.enabled = false;
    }
    private void Start()
    {
        successParticleImage.enabled = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlaySuccessMergeEffect()
    {
        successParticleImage.enabled = true;
        successParticleAnimator.SetTrigger("PlaySuccessParticle");
        Debug.Log("머지 성공 이펙트 실행");
    }
    public void InitEffect()
    {
        successParticleAnimator.Rebind();
        successParticleImage.enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
