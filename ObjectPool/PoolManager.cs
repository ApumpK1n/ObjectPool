using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : SingletonBehaviour<PoolManager>
{

    //private Dictionary<string, ObjectPool<MonoBehaviour>> pools;

    private PoolManager()
    {
        //pools = new Dictionary<string, ObjectPool<MonoBehaviour>>();
    }

    public void Allocate(string poolName)
    {
        
    }

    public void Recycle<T>(T item) where T:MonoBehaviour
    {
        string key = item.name;
        //if (!pools.ContainsKey(key))
        {
            //pools.Add(key, new ObjectPool<T>(item.gameObject));
        }
    }
}
