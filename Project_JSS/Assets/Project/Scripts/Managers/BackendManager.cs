using UnityEngine;
using BackEnd;
public class BackendManager : BaseManager
{
    [SerializeField]
    public UserInfo user;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Init()
    {
        base.Init();
        BackendSetup();
        GetUserInfoFromBackend();
    }
    private void Update()
    {
        if (Backend.IsInitialized)
        {
            
        }
    }
    // Update is called once per frame
    private void BackendSetup()
    {
        var bro = Backend.Initialize();
        if(bro.IsSuccess())
        {
            Debug.Log($"초기화 성공 : {bro}");
        }
        else
        {
            Debug.Log($"초기화 실패 : {bro}");
        }
    }
    public void GetUserInfoFromBackend()
    {
        user.GetUserInfoFromBackend();
    }
}
