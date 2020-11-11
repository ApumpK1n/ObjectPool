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
    public int hardLimit = 5;


    private Stack<GameObject> unUsedStack;
    private int maxSize;



    /// <summary>
    /// 预设对象池
    /// </summary>
    public void Initialize()
    {
        unUsedStack = new Stack<GameObject>(instancesToPreallocate);
        CreateameObjects(instancesToPreallocate);
    }

    private void CreateameObjects(int count)
    {
        if (imposeHardLimit && unUsedStack.Count + count > hardLimit)
        {
            count = hardLimit - unUsedStack.Count;
        }
           
        for (int n = 0; n < count; n++)
        {
            GameObject go = GameObject.Instantiate(prefab.gameObject) as GameObject;
            go.name = prefab.name;

            go.SetActive(false);
            unUsedStack.Push(go);
        }
    }

    public GameObject Pop()
    {
        if (unUsedStack.Count > 0)
        {
            return unUsedStack.Pop();
        }

        return null;
    }

    public ObjectPool(GameObject obj, int maxSize = -1, int originSize = 0)
    {
        //prefab = obj;
        //unUsedStack = new Stack<T>();

        for (int i = 0; i < originSize; i++)
        {
          //  unUsedStack.Push(CreateItem());
        }

        if (maxSize < 0)
        {
            this.maxSize = maxSize;
        }
        else
        {
            this.maxSize = Math.Max(originSize, maxSize);
        }
    }
    
 //   private T CreateItem()
 //   {
 //       GameObject obj = UnityEngine.Object.Instantiate<GameObject>(prefab);
 //       T component = obj.transform.GetComponent<T>();

 //       return component;
 //   }
    
  //  public T Allocate()
   // {
   //     if (unUsedStack.Count > 0)
    //    {
    //        return unUsedStack.Pop();
   //     }
    ////    else
   //     {
   //         return CreateItem();
   //     }
   // }

   // public void Recycle(T item)
    //{
   //     item.transform.parent = null;
   //     if (maxSize < 0)
   //     {
    //        unUsedStack.Push(item);
     //   }
    //    else
     //   {
     //       if (GetPoolSize() < maxSize)
     //       {
     //           unUsedStack.Push(item);
      ///      }
     //       else
      //      {
     //           UnityEngine.Object.Destroy(item.gameObject);
     //       }
     //   }
    //}

    public int GetPoolSize()
    {
        return unUsedStack.Count;
    }

    public void Release()
    {
        //T[] list = unUsedStack.ToArray();
        //for (int i = 0; i < list.Length; i++)
        //{
        //    UnityEngine.Object.Destroy(list[i].gameObject);
        //}
       // unUsedStack.Clear();
    }

}
