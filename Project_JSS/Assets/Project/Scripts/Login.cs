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
    //로그인 버튼 눌렀을 때 호출
    public void OnClickLogin()
    {
        //매개변수로 입력한 InputField UI의 색상과 Message 내용 초기화
        ResetUI(imageID, imagePW);

        //필드값이 비어있는지 체크
        if (IsFieldDataEmpty(imageID, inputFieldID.text, "아이디")) ;
        if (IsFieldDataEmpty(imagePW, inputFieldPW.text, "비밀번호")) ;

        //로그인 버튼을 연타하지 못하도록 상호작용 비활성화
        btnLogin.interactable = false;

        // 서버에 로그인을 요청하는 동안 화면에 출력하는 내용 업데이트
        //ex) 로그인 관련 텍스트 출력, 톱니바퀴 아이콘 회전 등
        StartCoroutine(nameof(LoginProcess));

        //뒤끝 서버 로그인 시도
        ResponseToLogin(inputFieldID.text, inputFieldPW.text);
    }

    private void ResponseToLogin(string ID, string PW)
    {
        //서버에 로그인 요청
        Backend.BMember.CustomLogin(ID, PW, callback =>
        {
            StopCoroutine(nameof(LoginProcess));
            //로그인 성공
            if(callback.IsSuccess())
            {
                SetMessage($"{inputFieldID.text}님 환영합니다.");
                Utils.LoadScene(SceneNames.Main);
            }
            //로그인 실패
            else
            {
                //로그인에 실패했을 땐느 다시 로그인을 해야하기 때문에 로그인 버튼 상호작용 활성화
                btnLogin.interactable = true;
                string message = string.Empty;
                switch(int.Parse(callback.GetStatusCode()))
                {
                    case 401: //존재하지 않는 아디리, 잘못된 비밀번호
                        message = callback.GetMessage().Contains("customId") ? "존재하지 않는 아이디입니다." : "잘못된 비밀번호입니다.";
                        break;
                    case 403:
                        message = callback.GetMessage().Contains("user") ? "차단당한 유저입니다." : "차단당한 디바이스입니다.";
                        break;
                    case 410:
                        message = "탈퇴가 진행중인 유저입니다.";

                        break;
                    default:
                        message = callback.GetMessage();
                        break;
                }

                //StatusCode 401에서 "잘못된 비밀번호입니다" 일때
                if(message.Contains("비밀번호"))
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
            SetMessage($"로그인 중입니다... {time:F1}");
            yield return null;
        }
    }
}
