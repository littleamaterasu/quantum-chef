using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform poolRoot;
    private readonly Queue<T> pool = new();

    public ObjectPool(T prefab, Transform poolRoot)
    {
        this.prefab = prefab;
        this.poolRoot = poolRoot;
    }

    public T Get(Transform parent)
    {
        T obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Object.Instantiate(prefab, poolRoot);
        }

        obj.transform.SetParent(parent, false);
        obj.gameObject.SetActive(true);

        return obj;
    }

    public void Release(T obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(poolRoot, false);
        pool.Enqueue(obj);
    }

    public void Clear()
    {
        while (pool.Count > 0)
        {
            Object.Destroy(pool.Dequeue().gameObject);
        }
    }
}