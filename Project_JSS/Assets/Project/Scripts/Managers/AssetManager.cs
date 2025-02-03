using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections.Generic;
using System.Collections;

public class AssetManager : BaseManager
{
    [SerializeField] public AssetReferencesSO assetReferences;
    [SerializeField] public AudioSource audioSourceBGM;
    [SerializeField] public AudioSource audioSourceUI;//환경음
    [SerializeField] public AudioSource audioSourceEffect;//효과음
    private Coroutine coroutine;

    // 캐시를 위한 딕셔너리
    private Dictionary<string, UnityEngine.Object> cachedAssets = new Dictionary<string, UnityEngine.Object>();


    public void LoadBackgroundImage(string backgroundKey, Action<Sprite> onComplete)
    {
        var assetReference = assetReferences.GetBackgroundAssetReference(backgroundKey);
        LoadAsset<Sprite>(backgroundKey, assetReference, onComplete, $"background sprite: {backgroundKey}");
    }

    public void LoadSound(string soundKey, SoundType type, Action<AudioClip> onComplete)
    {
        var assetReference = assetReferences.GetSoundAssetReference(soundKey, type);
        LoadAsset<AudioClip>(soundKey, assetReference, onComplete, $"sound effect: {soundKey}");
    }
    //일반 소리 재생
    public void PlaySound(string soundKey, SoundType type)
    {
        //Debug.Log("play sound" + soundKey);
        AudioSource source = null;
        switch (type)
        {
            case SoundType.BGM:
                source = audioSourceBGM;
                LoadSound(soundKey, type, sound =>
                {
                    source.clip = sound;
                    source.Play();
                });
                break;
            case SoundType.UI:
                source = audioSourceUI;
                LoadSound(soundKey, type, sound =>
                {
                    source.PlayOneShot(sound);
                });
                break;
            case SoundType.Effect:
                source = audioSourceEffect;
                LoadSound(soundKey, type, sound =>
                {
                    source.PlayOneShot(sound);
                });
                break;
            default:
                Debug.Log("SoundType Error");
                return;
        }
        
    }
    //페이드 인 소리 재생
    public void PlayBGMFadeInSound(string soundKey, float duration)
    {
        if(audioSourceBGM.clip == null)
        {
            LoadSound(soundKey, SoundType.BGM, sound =>
            {
                audioSourceBGM.clip = sound;
                audioSourceBGM.Play();
                FadeInSound(audioSourceBGM, duration);
            });
        }

        else
        {
            if(coroutine!=null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(FadeOutAndInSound(audioSourceBGM, duration, soundKey));
        }
    }
    //소리 페이드 인할 때


    public void FadeInSound(AudioSource source, float duration)
    {
        StartFadeSound(source, duration, true);
    }
    //소리 페이드 아웃할 때
    public void FadeOutSoundBGM(float duration)
    {
        StartFadeSound(audioSourceBGM, duration, false);
    }
    public void FadeOutSound(AudioSource source, float duration)
    {
        StartFadeSound(source, duration, false);
    }

    public void StartFadeSound(AudioSource source, float duration, bool isFadeIn)
    {

        StartCoroutine(FadeCoroutine(source, duration, isFadeIn));
    }
    private IEnumerator FadeOutAndInSound(AudioSource source, float duration, string soundKey)
    {
        FadeOutSound(audioSourceBGM, duration);
        yield return new WaitForSeconds(duration);
        LoadSound(soundKey, SoundType.BGM, sound =>
        {
            audioSourceBGM.clip = sound;
            audioSourceBGM.Play();
            FadeInSound(audioSourceBGM, duration);
        });
    }

    private IEnumerator FadeCoroutine(AudioSource source, float duration, bool isFadeIn)
    {
        float currentTime = 0;
        float startVolume;
        float targetVolume;
        if (isFadeIn)
        {
            startVolume = 0f;
            //targetVolume = Managers.Settings.GetCurrentBGMVolume();
        }
        else
        {
            startVolume = source.volume;
            targetVolume = 0f;
        }

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            //source.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }
        //source.volume = targetVolume;
        if (!isFadeIn)
        {
            source.Stop();
        }
    }



    private void LoadAsset<T>(string key, AssetReference assetReference, Action<T> onComplete, string errorMessage) where T : UnityEngine.Object
    {
        // 캐시에 존재하는지 확인
        if (cachedAssets.TryGetValue(key, out var cachedAsset))
        {
            // 다음 프레임에서 콜백 실행
            //Debug.Log("실행전");
            //Debug.Log($"Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            //Debug.Log(onComplete.Target);
            //Debug.Log(onComplete.Method);
            //Debug.Log($"캐시된 에셋: {cachedAsset}");

            // 메인 스레드에서 실행되도록 보장
            StartCoroutine(ExecuteOnNextFrame(() =>
            {
                //Debug.Log("실행후");
                //Debug.Log($"Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                //Debug.Log(onComplete.Target);
                //Debug.Log(onComplete.Method);
                //Debug.Log($"캐시된 에셋: {cachedAsset}");
                onComplete?.Invoke(cachedAsset as T);
            }));
            return;
        }

        if (assetReference != null)
        {
            assetReference.LoadAssetAsync<T>().Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    // 성공 시 캐시에 저장
                    cachedAssets[key] = handle.Result;
                    onComplete?.Invoke(handle.Result);
                }
                else
                {
                    Debug.LogError($"Failed to load {errorMessage}");
                }
            };
        }
        else
        {
            Debug.LogError($"Asset reference not found for {errorMessage}");
        }
    }

    private IEnumerator ExecuteOnNextFrame(Action action)
    {
        yield return null;
        action?.Invoke();
    }
}