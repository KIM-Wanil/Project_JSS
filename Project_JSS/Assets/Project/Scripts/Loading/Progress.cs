using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections;
using Unity.VisualScripting;

public class Progress : MonoBehaviour
{
    [SerializeField]
    private Slider              sliderProgress;
    [SerializeField]
    private TextMeshProUGUI     textProgressData;
    [SerializeField]
    private float               progressTime;
    public void Play(UnityAction action=null)
    {
        StartCoroutine(OnProgress(action));
    }

    private IEnumerator OnProgress(UnityAction action)
    {
        float current = 0;
        float percent = 0;
        while (percent<1)
        {
            current += Time.deltaTime;
            percent = current / progressTime;
            textProgressData.text = $"Now Loading... {sliderProgress.value*100:F0}%";
            sliderProgress.value = Mathf.Lerp(0, 1, percent);
            yield return null;
        }
        action?.Invoke();
    }
}
