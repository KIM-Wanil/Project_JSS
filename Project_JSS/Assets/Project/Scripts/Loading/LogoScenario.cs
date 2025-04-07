using UnityEngine;

public class LogoScenario : MonoBehaviour
{
    [SerializeField]
    private Progress progress;
    [SerializeField]
    private SceneNames nextScene;
    private void Awake()
    {
        SystemSetup();
        //Managers.Asset.PlayBGMFadeInSound("MainBGM", 0.5f);
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //SystemSetup();
            progress.Play(OnAfterProgress);
        }
    }
    // Update is called once per frame
    private void SystemSetup()
    {
        //활성화X 상태에서도 게임 계속 진행
        Application.runInBackground = true;

        //해상도 설정 (9:18.5, 1440 X 2960)
        //int width = Screen.width;
        //int height = (int)(Screen.width * 16f / 9);
        //int width = (int)(Screen.height * 9f / 16f);
        //int height = Screen.height;
        //int width = 720;
        //int height = 1280;
        int width = 607;
        int height = 1080;
        Screen.SetResolution(width, height, true);

        //화면이 꺼지지 않도록 설정
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // 로딩 애니메이션 시작, 재생 완료시 OnAfterProgress() 메소드 실행
        //progress.Play(OnAfterProgress);

    }

    private void OnAfterProgress()
    {
        Utils.LoadScene(nextScene);
    }
}
