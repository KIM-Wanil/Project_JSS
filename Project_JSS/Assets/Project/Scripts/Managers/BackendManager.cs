using UnityEngine;
using BackEnd;
public class BackendManager : BaseManager
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Init()
    {
        base.Init();
        BackendSetup();
    }

    // Update is called once per frame
    private void BackendSetup()
    {
        var bro = Backend.Initialize();

        if(bro.IsSuccess())
        {
            Debug.Log($"�ʱ�ȭ ���� : {bro}");
        }
        else
        {
            Debug.Log($"�ʱ�ȭ ���� : {bro}");
        }
    }
}
