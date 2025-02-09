using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;
using TMPro;
[Serializable]
public class UIManager : BaseManager
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] public float targetAspect = 16f / 9f;
    private TextMeshProUGUI energyText; // TextMeshProUGUI ������Ʈ �߰�
    private TextMeshProUGUI goldText; // TextMeshProUGUI ������Ʈ �߰�
    // private TextMeshProUGUI maxEnergyText; // TextMeshProUGUI ������Ʈ �߰�

    private void Awake()
    {
        mainCamera = Camera.main;
        //UpdateLetterbox();
        energyText = GameObject.Find("EnergyAmountText").GetComponent<TextMeshProUGUI>();
        goldText = GameObject.Find("GoldAmountText").GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        // GameManager�� ������ ���� �̺�Ʈ ����
        Managers.Game.onEnergyChanged.AddListener(UpdateEnergyUI);
        // �ʱ� ������ UI ����
        UpdateEnergyUI(Mathf.RoundToInt(Managers.Game.CurrentEnergy));

        // GameManager�� ������ ���� �̺�Ʈ ����
        Managers.Game.onGoldChanged.AddListener(UpdateGoldUI);
        // �ʱ� ������ UI ����
        UpdateGoldUI(Managers.Game.CurrentGold);
    }

    private void OnDestroy()
    {
        // GameManager�� ������ ���� �̺�Ʈ ���� ����
        if (Managers.Game != null)
        {
            Managers.Game.onEnergyChanged.RemoveListener(UpdateEnergyUI);
        }
    }

    // ������ UI ������Ʈ �޼���
    private void UpdateEnergyUI(int currentEnergy)
    {
        if (energyText != null)
        {
            energyText.text = $"{currentEnergy}";
        }
    }
    private void UpdateGoldUI(int currentEnergy)
    {
        if (goldText != null)
        {
            goldText.text = $"{currentEnergy}";
        }
    }



    //private void Start()
    //{
    //    mainCamera = Camera.main;
    //    UpdateLetterbox();
    //}

    //private StringBuilder sb = new StringBuilder();
    //public string ProcessText(string text)
    //{
    //    sb.Clear();
    //    sb.Append(text);

    //    //sb.Replace("<p>", Managers.Save.playerName);
    //    sb.Replace("<n>", "\n");
    //    sb.Replace("<c>", ",");

    //    return sb.ToString();
    //}

    //public void UpdateLetterbox()
    //{
    //    mainCamera = Camera.main;
    //    float windowAspect = (float)Screen.width / (float)Screen.height;
    //    float scaleHeight = windowAspect / targetAspect;

    //    if (scaleHeight < 1.0f)
    //    {
    //        Rect rect = mainCamera.rect;
    //        rect.width = 1.0f;
    //        rect.height = scaleHeight;
    //        rect.x = 0;
    //        rect.y = (1.0f - scaleHeight) / 2.0f;
    //        mainCamera.rect = rect;
    //    }
    //    else
    //    {
    //        float scaleWidth = 1.0f / scaleHeight;
    //        Rect rect = mainCamera.rect = new Rect(0, 0, 1, 1);
    //        rect.width = scaleWidth;
    //        rect.height = 1.0f;
    //        rect.x = (1.0f - scaleWidth) / 2.0f;
    //        rect.y = 0;
    //        mainCamera.rect = rect;
    //    }
    //}
    //private void OnRectTransformDimensionsChange()
    //{
    //    UpdateLetterbox();
    //}

}

