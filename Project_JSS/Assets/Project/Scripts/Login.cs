using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using System.Collections;
using Ricimi;

public class Login : LoginBase
{
    [SerializeField]
    private Image               imageID;
    [SerializeField]
    private TMP_InputField      inputFieldID;
    [SerializeField]
    private Image               imagePW;
    [SerializeField]
    private TMP_InputField      inputFieldPW;

    [SerializeField]
    private Button              btnLogin;

    private Popup               popup;
    private void Awake()
    {
        popup = GetComponent<Popup>();
    }
    public void Start()
    {
        popup.Open();
    }
    //�α��� ��ư ������ �� ȣ��
    public void OnClickLogin()
    {
        //�Ű������� �Է��� InputField UI�� ����� Message ���� �ʱ�ȭ
        ResetUI(imageID, imagePW);

        //�ʵ尪�� ����ִ��� üũ
        if (IsFieldDataEmpty(imageID, inputFieldID.text, "���̵�")) ;
        if (IsFieldDataEmpty(imagePW, inputFieldPW.text, "��й�ȣ")) ;

        //�α��� ��ư�� ��Ÿ���� ���ϵ��� ��ȣ�ۿ� ��Ȱ��ȭ
        btnLogin.interactable = false;

        // ������ �α����� ��û�ϴ� ���� ȭ�鿡 ����ϴ� ���� ������Ʈ
        //ex) �α��� ���� �ؽ�Ʈ ���, ��Ϲ��� ������ ȸ�� ��
        StartCoroutine(nameof(LoginProcess));

        //�ڳ� ���� �α��� �õ�
        ResponseToLogin(inputFieldID.text, inputFieldPW.text);
    }

    private void ResponseToLogin(string ID, string PW)
    {
        //������ �α��� ��û
        Backend.BMember.CustomLogin(ID, PW, callback =>
        {
            StopCoroutine(nameof(LoginProcess));
            //�α��� ����
            if(callback.IsSuccess())
            {
                SetMessage($"{inputFieldID.text}�� ȯ���մϴ�.");
                Utils.LoadScene(SceneNames.Main);
            }
            //�α��� ����
            else
            {
                //�α��ο� �������� ���� �ٽ� �α����� �ؾ��ϱ� ������ �α��� ��ư ��ȣ�ۿ� Ȱ��ȭ
                btnLogin.interactable = true;
                string message = string.Empty;
                switch(int.Parse(callback.GetStatusCode()))
                {
                    case 401: //�������� �ʴ� �Ƶ�, �߸��� ��й�ȣ
                        message = callback.GetMessage().Contains("customId") ? "�������� �ʴ� ���̵��Դϴ�." : "�߸��� ��й�ȣ�Դϴ�.";
                        break;
                    case 403:
                        message = callback.GetMessage().Contains("user") ? "���ܴ��� �����Դϴ�." : "���ܴ��� ����̽��Դϴ�.";
                        break;
                    case 410:
                        message = "Ż�� �������� �����Դϴ�.";

                        break;
                    default:
                        message = callback.GetMessage();
                        break;
                }

                //StatusCode 401���� "�߸��� ��й�ȣ�Դϴ�" �϶�
                if(message.Contains("��й�ȣ"))
                {
                    GuideForIncorrectlyEnteredData(imagePW, message);
                }
                else
                {
                    GuideForIncorrectlyEnteredData(imageID, message);

                }
            }
        });
    }
    private IEnumerator LoginProcess()
    {
        float time = 0;
        while (true)
        {
            time += Time.deltaTime;
            SetMessage($"�α��� ���Դϴ�... {time:F1}");
            yield return null;
        }
    }
}
