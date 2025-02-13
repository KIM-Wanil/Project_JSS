using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ClickableItem : MonoBehaviour
{
    [SerializeField] private Button button;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void SetButtonEvent(UnityAction action)
    {
        button.onClick.AddListener(action);
    }

}
