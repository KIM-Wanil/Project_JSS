using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginBase : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textMessage;
    
    //�޼��� ����, InputField ���� �ʱ�ȭ
    protected void ResetUI(params Image[] image)
    {
        textMessage.text = string.Empty;

        for(int i=0; i<image.Length; i++) 
        {
            image[i].color = Color.white;
        }
    }

    //�Ű��������ִ� ������ ���
    protected void SetMessage(string msg)
    {
       textMessage.text = msg;
    }

    //�Է� ������ �ִ� InputField�� ���� ����
    //������ ���� �޽��� ���
    protected void GuideForIncorrectlyEnteredData(Image image, string msg)
    {
        textMessage.text = msg;
        image.color = Color.red;
    }

    //�ʵ尪�̺���ִ��� Ȯ��
    protected bool IsFieldDataEmpty(Image image, string field, string result)
    {
        if(field.Trim().Equals(""))
        {
            GuideForIncorrectlyEnteredData(image, $"\"{result}\" �ʵ带 ä���ּ���.");
            return true;
        }
        return false;
    }
}
