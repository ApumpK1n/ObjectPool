using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : SingletonBehaviour<PoolManager>
{

    private readonly Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();
    [HideInInspector]
    public List<ObjectPool> objectPoolCollection = new List<ObjectPool>();

    private void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(Instance);

        InitializePrefabPools();
        SceneManager.activeSceneChanged += activeSceneChanged;
    }

    #region Private
    private void activeSceneChanged(Scene oldScene, Scene newScene)
    {
        if (oldScene.name == null)
            return;

        for (var i = objectPoolCollection.Count - 1; i >= 0; i--)
        {
            if (!objectPoolCollection[i].persistBetweenScenes)
                RemovePool(objectPoolCollection[i]);
        }
    }

    private void InitializePrefabPools()
    {
        if (objectPoolCollection == null)
            return;

        foreach (ObjectPool objectPool in objectPoolCollection)
        {
            if (objectPool == null || objectPool.prefab == null)
                continue;

            objectPool.Initialize();
            pools.Add(objectPool.prefab.name, objectPool);
        }
    }

    #endregion Private

    #region Public
    public void AddObjectPoolToManager(ObjectPool objectPool)
    {
        string Name = objectPool.prefab.name;
        if (pools.ContainsKey(Name))
        {
            Debug.LogError("重复管理 (" + Name + ")！");
            return;
        }
        objectPool.Initialize();
        pools.Add(Name, objectPool);
    }

    /// <summary>
    /// 从对象池获取一个对象，对象池设置了有上限及达到上限时返回空
    /// </summary>
    /// <param name="go"></param>
    /// <param name="parent"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public GameObject Generate(GameObject go, Transform parent, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion)) 
    {
        if(pools.ContainsKey(go.name))
        {
            ObjectPool pool = pools[go.name];
            GameObject ge = pool.Generate();
            if (ge != null)
            {
                ge.transform.parent = parent;
                ge.transform.position = position;
                ge.transform.rotation = rotation;
            }
            return ge;
        }
        else
        {
            Debug.LogError("尝试生成对象池对象 (" + go.name + ") 但是对象池并未添加到管理器！");
            return null;
        }
    }

    /// <summary>
    /// 回收对象到对象池
    /// </summary>
    /// <param name="go"></param>
    public void Recycle(GameObject go)
    {
        if (go == null)
            return;
        string goName = go.name;
        if (!pools.ContainsKey(goName))
        {
            Destroy(go);
            Debug.LogError("尝试回收对象池对象 (" + go.name + ") 但是对象池并未添加到管理器！");
        }
        else
        {
            pools[goName].Recycle(go);
            go.transform.parent = transform;
        }
    }

    /// <summary>
    /// 销毁对象池
    /// </summary>
    /// <param name="objectPool"></param>
    public void RemovePool(ObjectPool objectPool)
    {
        var poolName = objectPool.prefab.name;

        if (pools.ContainsKey(poolName))
        {
            pools.Remove(poolName);
            objectPoolCollection.Remove(objectPool);
            objectPool.Release();
        }

    }

    /// <summary>
    /// 获取对象池
    /// </summary>
    /// <param name="gameObjectName"></param>
    /// <returns></returns>
    public ObjectPool GetObjectPoolByName(string gameObjectName)
    {
        if (pools.ContainsKey(gameObjectName))
        {
            return pools[gameObjectName];
        }
        return null;
    }
    #endregion Public

}
