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
        //현재 로그인한 사용자 정보 불러오기
        Backend.BMember.GetUserInfo(callback =>
        {
            Debug.Log(callback.IsSuccess());
            //정보 불러오기 성공
            if(callback.IsSuccess())
            {
                //JSON 데이터 파싱 성공
                try
                {
                    // JSON 데이터가 null인지 확인
                    var returnValue = callback.GetReturnValuetoJSON();
                    if (returnValue == null)
                    {
                        throw new System.Exception("Return value is null");
                    }

                    // "row" 데이터가 null인지 확인
                    if (!returnValue.ContainsKey("row") || returnValue["row"] == null)
                    {
                        throw new System.Exception("\"row\" key is missing or null");
                    }
                    LitJson.JsonData json = returnValue["row"];
                    // "row"의 키들을 출력
                    if (json.IsObject)
                    {
                        foreach (string key in json.Keys)
                        {
                            Debug.Log($"Key: {key}, Value: {json[key]}");
                        }
                    }
                    //Debug.Log(json.);
                    // 각 값이 null이 아닐 때만 할당
                    if (json["gamerId"] != null) data.gamerId = json["gamerId"].ToString();
                    if (json["countryCode"] != null) data.countryCode = json["countryCode"].ToString();
                    if (json["nickname"] != null) data.nickname = json["nickname"].ToString();
                    if (json["inDate"] != null) data.inDate = json["inDate"].ToString();
                    if (json["emailForFindPassword"] != null) data.emailForFindPassword = json["emailForFindPassword"].ToString();
                    if (json["subscriptionType"] != null) data.subscriptionType = json["subscriptionType"].ToString();
                    if (json["federationId"] != null) data.federationId = json["federationId"].ToString();


                }
                //JSON 데이터 파싱 실패
                catch (System.Exception e)
                {
                    //유저 정보를 기본 상태로 설정
                    data.Reset();
                    //try-catch 에러 출력
                    Debug.LogError(e);
                    Debug.LogError("유저정보 파싱에러");
                }
            }
            //정보 불러오기 실패
            else
            {

            }
            //유저 정보 불러오기가 완료되었을 때 onUSerInfoEvent에 등록되어 있는 이벤트 메소드 호출
            onUserInfoEvent?.Invoke();
        }
        );
    }

}
public class UserInfoData
{
    public string gamerId;                  //유저의 gamerId
    public string countryCode;              //국가코드. 설정 안했으면 null
    public string nickname;                 //닉네임. 설정 안했으면 null
    public string inDate;                   //유저의 inDate
    public string emailForFindPassword;     //이메일 주소. 설정 안했으면 null
    public string subscriptionType;        //커스텀, 페더레이션 타입
    public string federationId;             //구글, 애플, 페이스북 페더레이션 ID. 커스텀 계정은 null

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

