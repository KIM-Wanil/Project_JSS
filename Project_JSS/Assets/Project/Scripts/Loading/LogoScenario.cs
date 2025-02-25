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
    }

    // Update is called once per frame
    private void SystemSetup()
    {
        //Ȱ��ȭX ���¿����� ���� ��� ����
        Application.runInBackground = true;

        //�ػ� ���� (9:18.5, 1440 X 2960)
        int width = Screen.width;
        int height = (int)(Screen.width * 18.5f / 9);
        Screen.SetResolution(width, height, true);

        //ȭ���� ������ �ʵ��� ����
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // �ε� �ִϸ��̼� ����, ��� �Ϸ�� OnAfterProgress() �޼ҵ� ����
        progress.Play(OnAfterProgress);

    }

    private void OnAfterProgress()
    {
        Utils.LoadScene(nextScene);
    }
}
