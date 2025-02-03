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
    //public Dictionary<string, int> acquiredIngredientDict = new Dictionary<string, int>();

    //private IngredientDB ingredientDB = new IngredientDB();
    //public void AcquireIngredient2(string ingredientId, int num)
    //{
    //    if (acquiredIngredientDict.ContainsKey(ingredientId))
    //    {
    //        acquiredIngredientDict[ingredientId] += num;
    //    }
    //    else
    //    {
    //        acquiredIngredientDict.Add(ingredientId, num);
    //    }
    //}
    //public Dictionary<string, int> GetAcquiredIngredientDict()
    //{
    //    return acquiredIngredientDict;
    //}
    // Start is called before the first frame update
    public override void Init()
    {
        Debug.Log("DBManager initialized");
    }

    //재료 정보 가져오는거 
    //public Task<IngredientSO> LoadIngredient(string iconId)
    //{
    //    return ingredientDB.LoadIngredient(iconId);
    //}
}
