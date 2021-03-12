using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : SingletonBehaviour<PoolManager>
{

    private readonly Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();
    [HideInInspector]
    public List<ObjectPool> objectPoolCollection = new List<ObjectPool>();
    private Dictionary<GameObject, DelayRecycleEvent> delayRecycleEvents = new Dictionary<GameObject, DelayRecycleEvent>();
    private Dictionary<ObjectPool, List<GameObject>> delayObjectPools = new Dictionary<ObjectPool, List<GameObject>>();

    class DelayRecycleEvent
    {
        public Action<GameObject, DelayRecycleEvent> delayRecycle;
        public float delayTime;
        public GameObject go;

        public DelayRecycleEvent(GameObject go, float delayTime)
        {
            this.delayTime = delayTime;
            this.go = go;
        }

        public void InvokeEvent()
        {
            delayRecycle?.Invoke(go, this);
        }

        public void Update()
        {
            delayTime -= Time.deltaTime;
            if (delayTime <= 0)
            {
                InvokeEvent();
            }
        }

        public void Release()
        {
            delayRecycle = null;
            go = null;
        }

    }

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(Instance);

        InitializePrefabPools();
        SceneManager.activeSceneChanged += activeSceneChanged;
    }

    private void Update()
    {
        List<DelayRecycleEvent> events = delayRecycleEvents.Values.ToList();
        for (int i = 0; i < events.Count; i++)
        {
            events[i].Update();
        }
    }

    private void DespawnAfterDelay(GameObject go, DelayRecycleEvent eventDelay)
    {
        Recycle(go);
    }

    #region Private
    private void activeSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene == null)
            return;

        for (var i = objectPoolCollection.Count - 1; i >= 0; i--)
        {
            if (!objectPoolCollection[i].persistBetweenScenes)
                RemovePool(objectPoolCollection[i]);
        }

        List<ObjectPool> codeAdd = pools.Values.ToList();
        for (var i = codeAdd.Count - 1; i >= 0; i--)
        {
            if (!codeAdd[i].persistBetweenScenes)
                RemovePool(codeAdd[i]);
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

    private void RemoveEvent(GameObject go)
    {
        if (delayRecycleEvents.ContainsKey(go))
        {
            DelayRecycleEvent eventDelay = delayRecycleEvents[go];
            eventDelay.Release();
            delayRecycleEvents.Remove(go);
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
    public GameObject Generate(GameObject go, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent=null) 
    {
        if(pools.ContainsKey(go.name))
        {
            ObjectPool pool = pools[go.name];
            GameObject ge = pool.Generate();
            if (ge != null)
            {
                if (parent != null) ge.transform.parent = parent;
                ge.transform.position = position;
                ge.transform.rotation = rotation;
                ge.SetActive(true);
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

        RemoveEvent(go);

        string goName = go.name;
        if (!pools.ContainsKey(goName))
        {
            Destroy(go);
            Debug.LogError("尝试回收对象池对象 (" + go.name + ") 但是对象池并未添加到管理器！");
        }
        else
        {
            go.transform.parent = transform;
            pools[goName].Recycle(go);
        }
    }

    /// <summary>
    /// 延迟回收
    /// </summary>
    /// <param name="go"></param>
    /// <param name="delaySeconds"></param>
    public void RecycleAfterDelay(GameObject go, float delaySeconds)
    {
        if (go == null)
            return;
        if (delaySeconds > 0 && pools.ContainsKey(go.name))
        {
            if (!delayRecycleEvents.ContainsKey(go))
            {
                DelayRecycleEvent delayRecycleEvent = new DelayRecycleEvent(go, delaySeconds);
                delayRecycleEvent.delayRecycle += DespawnAfterDelay;
                delayRecycleEvents.Add(go, delayRecycleEvent);
            }
            else
            {
                delayRecycleEvents[go].Release();
                delayRecycleEvents[go] = null;

                DelayRecycleEvent delayRecycleEvent = new DelayRecycleEvent(go, delaySeconds);
                delayRecycleEvent.delayRecycle += DespawnAfterDelay;
                delayRecycleEvents[go] = delayRecycleEvent;
            }

            ObjectPool pool = GetObjectPoolByName(go);
            if (delayObjectPools.ContainsKey(pool))
            {
                delayObjectPools[pool].Add(go);
            }
            else
            {
                delayObjectPools.Add(pool, new List<GameObject>() { go });
            }
        }
        else
        {
            Recycle(go);
        }

    }


    /// <summary>
    /// 销毁对象池
    /// </summary>
    /// <param name="objectPool"></param>
    public void RemovePool(ObjectPool objectPool)
    {
        var poolName = objectPool.prefab.name;

        if (delayObjectPools.ContainsKey(objectPool)) // 先处理延迟回收的对象
        {
            List<GameObject> objects = delayObjectPools[objectPool];
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject obj = objects[i];
                if (delayRecycleEvents.ContainsKey(obj))
                {
                    RemoveEvent(obj);
                    Destroy(obj);
                }
            }
            delayObjectPools.Remove(objectPool);
        }

        if (pools.ContainsKey(poolName))
        {
            pools.Remove(poolName);
            objectPoolCollection.Remove(objectPool);
            objectPool.Release(); //摧毁已经回收的对象
        }
    }

    /// <summary>
    /// 获取对象池
    /// </summary>
    /// <param name="gameObjectName"></param>
    /// <returns></returns>
    public ObjectPool GetObjectPoolByName(GameObject gameObject)
    {
        string gameObjectName = gameObject.name;
        if (pools.ContainsKey(gameObjectName))
        {
            return pools[gameObjectName];
        }
        return null;
    }
    #endregion Public

}
