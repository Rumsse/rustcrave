using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    private Dictionary<Component, IObjectPool<Component>> _pools = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public T Get<T>(T obj, int defaultCapacity = 20, int maxSize = 50) where T : Component
    {
        if (!_pools.TryGetValue(obj, out var pool))
        {
            pool = CreateNewPool(obj, defaultCapacity, maxSize);
            _pools[obj] = pool;
        }

        return (T)_pools[obj].Get();
    }

    public void Release<T>(T instance, T obj) where T : Component
    {
        if (_pools.ContainsKey(obj)) _pools[obj].Release(instance);
        else Destroy(instance.gameObject);
    }

    private IObjectPool<Component> CreateNewPool<T>(T obj, int defaultCapacity, int maxSize) where T : Component
    {
        return new ObjectPool<Component>(
            createFunc: () => Instantiate(obj),
            actionOnGet: (proj) => proj.gameObject.SetActive(true),
            actionOnRelease: (proj) => proj.gameObject.SetActive(false),
            actionOnDestroy: (proj) => Destroy(proj.gameObject),
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }
}