using UnityEngine;
using BackEnd;
using BackEnd.BackndLitJson;
using LitJson;

public class UserInfo : MonoBehaviour
{
    [System.Serializable]
    public class UserInfoEvent : UnityEngine.Events.UnityEvent { }
    public UserInfoEvent onUserInfoEvent = new UserInfoEvent();
    public static UserInfoData data = new UserInfoData();
    public static UserInfoData Data => data;

    public void GetUseInfoFromBackend()
    {
        //���� �α����� ����� ���� �ҷ�����
        Backend.BMember.GetUserInfo(callback =>
        {
            Debug.Log(callback.IsSuccess());
            //���� �ҷ����� ����
            if(callback.IsSuccess())
            {
                //JSON ������ �Ľ� ����
                try
                {
                    // JSON �����Ͱ� null���� Ȯ��
                    var returnValue = callback.GetReturnValuetoJSON();
                    if (returnValue == null)
                    {
                        throw new System.Exception("Return value is null");
                    }

                    // "row" �����Ͱ� null���� Ȯ��
                    if (!returnValue.ContainsKey("row") || returnValue["row"] == null)
                    {
                        throw new System.Exception("\"row\" key is missing or null");
                    }
                    LitJson.JsonData json = returnValue["row"];
                    // "row"�� Ű���� ���
                    if (json.IsObject)
                    {
                        foreach (string key in json.Keys)
                        {
                            Debug.Log($"Key: {key}, Value: {json[key]}");
                        }
                    }
                    //Debug.Log(json.);
                    // �� ���� null�� �ƴ� ���� �Ҵ�
                    if (json["gamerId"] != null) data.gamerId = json["gamerId"].ToString();
                    if (json["countryCode"] != null) data.countryCode = json["countryCode"].ToString();
                    if (json["nickname"] != null) data.nickname = json["nickname"].ToString();
                    if (json["inDate"] != null) data.inDate = json["inDate"].ToString();
                    if (json["emailForFindPassword"] != null) data.emailForFindPassword = json["emailForFindPassword"].ToString();
                    if (json["subscriptionType"] != null) data.subscriptionType = json["subscriptionType"].ToString();
                    if (json["federationId"] != null) data.federationId = json["federationId"].ToString();


                }
                //JSON ������ �Ľ� ����
                catch (System.Exception e)
                {
                    //���� ������ �⺻ ���·� ����
                    data.Reset();
                    //try-catch ���� ���
                    Debug.LogError(e);
                    Debug.LogError("�������� �Ľ̿���");
                }
            }
            //���� �ҷ����� ����
            else
            {

            }
            //���� ���� �ҷ����Ⱑ �Ϸ�Ǿ��� �� onUSerInfoEvent�� ��ϵǾ� �ִ� �̺�Ʈ �޼ҵ� ȣ��
            onUserInfoEvent?.Invoke();
        }
        );
    }

}
public class UserInfoData
{
    public string gamerId;                  //������ gamerId
    public string countryCode;              //�����ڵ�. ���� �������� null
    public string nickname;                 //�г���. ���� �������� null
    public string inDate;                   //������ inDate
    public string emailForFindPassword;     //�̸��� �ּ�. ���� �������� null
    public string subscriptionType;        //Ŀ����, ������̼� Ÿ��
    public string federationId;             //����, ����, ���̽��� ������̼� ID. Ŀ���� ������ null

    public void Reset()
    {
        gamerId                     = "Offline";
        countryCode                 = "Unknown";
        nickname                    = "Noname";
        inDate                      = string.Empty;
        emailForFindPassword        = string.Empty;
        subscriptionType            = string.Empty;
        federationId                = string.Empty;
    }
}

