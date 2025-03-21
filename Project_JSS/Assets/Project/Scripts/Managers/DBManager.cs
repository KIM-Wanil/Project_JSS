using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
[Serializable]
public class DBManager : BaseManager
{
    private ItemDB itemDB = new ItemDB();
    private GeneratorDB generatorDB = new GeneratorDB();

    public override void Init()
    {
        Debug.Log("DBManager initialized");
    }

    //아이템 정보 가져오는거 
    public Task<ItemSO> LoadItem(string id)
    {
        return itemDB.LoadItem(id);
    }
    //제너레이터 정보 가져오는거
    public Task<GeneratorSO> LoadGenerator(string id)
    {
        return generatorDB.LoadGenerator(id);
    }
}
