using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    /// <summary>
    /// 对象池创建的预制体
    /// </summary>
    public GameObject prefab;
    /// <summary>
    /// 初始创建对象个数 
    /// </summary>
    public int instancesToPreallocate = 5;
    /// <summary>
    /// 是否限制池的容量
    /// </summary>
    public bool imposeHardLimit = false;
    /// <summary>
    /// 池的最大容量
    /// </summary>
    public int hardLimit = 10;
    /// <summary>
    /// 切换场景是否保留对象池
    /// </summary>
    public bool persistBetweenScenes = false;

    /// <summary>
    /// 对象池已经创建的对象数量
    /// </summary>
    private int instanceCount = 0;

    public Action<GameObject> onGernerateEvent;
    public Action<GameObject> onRecycleEvent;
    public Action<GameObject> onDestroyEvent;

    private Stack<GameObject> unUsedStack;

    public ObjectPool(GameObject prefab, int instancesToPreallocate=5, bool imposeHardLimit=false, int hardLimit=10, bool persistBetweenScenes = false)
    {
        this.prefab = prefab;
        this.instancesToPreallocate = instancesToPreallocate;
        this.imposeHardLimit = imposeHardLimit;
        this.hardLimit = hardLimit;
        this.persistBetweenScenes = persistBetweenScenes;
    }


    #region Private
    private void CreateameObjects(int count)
    {
        if (imposeHardLimit && unUsedStack.Count + count > hardLimit)
        {
            count = hardLimit - unUsedStack.Count;
        }
           
        for (int n = 0; n < count; n++)
        {
            GameObject go = GameObject.Instantiate(prefab);
            go.name = prefab.name;
            go.transform.parent = PoolManager.Instance.transform;

            go.SetActive(false);
            unUsedStack.Push(go);
        }
    }

    private GameObject Pop()
    {
        if (imposeHardLimit && instanceCount >= hardLimit)
            return null;

        if (unUsedStack.Count > 0)
        {
            instanceCount++;
            return unUsedStack.Pop();
        }

        CreateameObjects(3);
        return Pop();
    }
    #endregion

    #region Public
    /// <summary>
    /// 预设对象池
    /// </summary>
    public void Initialize()
    {
        unUsedStack = new Stack<GameObject>(instancesToPreallocate);
        CreateameObjects(instancesToPreallocate);
    }

    /// <summary>
    /// 返回一个游戏对象，当对象池容量达到上限时，返回空
    /// </summary>
    /// <returns></returns>

    public GameObject Generate()
    {
        GameObject go = Pop();
        onGernerateEvent?.Invoke(go);
        return go;
    }

    /// <summary>
    /// 回收游戏对象
    /// </summary>
    public void Recycle(GameObject go)
    {
        go.SetActive(false);

        instanceCount--;
        unUsedStack.Push(go);
        onRecycleEvent?.Invoke(go);
    }

    /// <summary>
    /// 清空对象池并销毁对象
    /// </summary>
    public void Release()
    {
        onRecycleEvent = null;
        onGernerateEvent = null;
        while (unUsedStack.Count > 0)
        {
            var go = unUsedStack.Pop();
            GameObject.Destroy(go);
            onDestroyEvent?.Invoke(go);
        }
        onDestroyEvent = null;
    }
    #endregion
}
