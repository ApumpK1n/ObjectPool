using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T:MonoBehaviour
{
    private GameObject prefab;
    private Stack<T> unUsedStack;
    private int maxSize;

    public ObjectPool(string prefabPath, int maxSize = -1, int originSize = 0):
        this(Resources.Load<GameObject>(prefabPath), originSize)
    {
    }

    public ObjectPool(GameObject obj, int maxSize = -1, int originSize = 0)
    {
        prefab = obj;
        unUsedStack = new Stack<T>();

        for (int i = 0; i < originSize; i++)
        {
            unUsedStack.Push(CreateItem());
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
    
    private T CreateItem()
    {
        GameObject obj = UnityEngine.Object.Instantiate<GameObject>(prefab);
        T component = obj.transform.GetComponent<T>();

        return component;
    }
    
    public T Allocate()
    {
        if (unUsedStack.Count > 0)
        {
            return unUsedStack.Pop();
        }
        else
        {
            return CreateItem();
        }
    }

    public void Recycle(T item)
    {
        item.transform.parent = null;
        if (maxSize < 0)
        {
            unUsedStack.Push(item);
        }
        else
        {
            if (GetPoolSize() < maxSize)
            {
                unUsedStack.Push(item);
            }
            else
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }
        }
    }

    public int GetPoolSize()
    {
        return unUsedStack.Count;
    }

    public void Release()
    {
        T[] list = unUsedStack.ToArray();
        for (int i = 0; i < list.Length; i++)
        {
            UnityEngine.Object.Destroy(list[i].gameObject);
        }
        unUsedStack.Clear();
    }
}
