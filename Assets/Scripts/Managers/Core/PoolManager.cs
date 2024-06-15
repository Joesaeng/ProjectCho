using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    #region Pool
    class Pool
    {
        public GameObject Original { get; private set; }

        Stack<Poolable> _poolStack = new Stack<Poolable>();

        public void Init(GameObject original, int count = 1)
        {
            Original = original;

            for (int i = 0; i < count; ++i)
            {
                Push(Create());
            }
        }

        Poolable Create()
        {
            GameObject go = Object.Instantiate<GameObject>(Original);
            go.name = Original.name;
            return go.GetOrAddComponent<Poolable>();
        }

        public void Push(Poolable poolable)
        {
            if (poolable == null)
                return;

            poolable.transform.SetParent(null);
            poolable.gameObject.SetActive(false);
            poolable.isUsing = false;

            _poolStack.Push(poolable);
        }

        public Poolable Pop(Transform parent = null)
        {
            Poolable poolable;

            if (_poolStack.Count > 0)
                poolable = _poolStack.Pop();
            else
                poolable = Create();

            poolable.transform.SetParent(parent);
            poolable.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            poolable.gameObject.SetActive(true);

            poolable.isUsing = true;

            return poolable;
        }
    }
    #endregion

    List<GameObject> poolablePrefabs;
    Dictionary<string, Pool> _pool = new Dictionary<string, Pool>();

    public void CreatePool(GameObject original, int count = 5)
    {
        Pool pool = new Pool();
        pool.Init(original, count);

        _pool.Add(original.name, pool);
    }

    public void Push(Poolable poolable)
    {
        string name = poolable.gameObject.name;
        if (_pool.ContainsKey(name) == false)
        {
            GameObject.Destroy(poolable.gameObject);
            return;
        }

        _pool[name].Push(poolable);
    }

    public Poolable Pop(GameObject original, Transform parent = null)
    {
        if (_pool.ContainsKey(original.name) == false)
        {
            CreatePool(original);
        }
        return _pool[original.name].Pop(parent);
    }

    public GameObject GetOriginal(string name)
    {
        if (_pool.ContainsKey(name) == false)
            return null;

        return _pool[name].Original;
    }

    public void Init()
    {
        LoadPoolablePrefabs();
        InitializeObjectPool();
    }

    void LoadPoolablePrefabs()
    {
        TextAsset jsonTextAsset = Resources.Load<TextAsset>("Prefabs/PoolablePrefabs");
        if (jsonTextAsset != null)
        {
            PoolablePrefabList prefabList = JsonUtility.FromJson<PoolablePrefabList>(jsonTextAsset.text);
            poolablePrefabs = new List<GameObject>();

            foreach (string path in prefabList.paths)
            {
                GameObject prefab = Resources.Load<GameObject>(path);
                if (prefab != null)
                {
                    poolablePrefabs.Add(prefab);
                }
                else
                {
                    Debug.LogWarning("Could not load prefab at path: " + path);
                }
            }
        }
        else
        {
            Debug.LogError("Could not find PoolablePrefabs.json in Resources folder.");
        }
    }

    void InitializeObjectPool()
    {
        foreach (GameObject prefab in poolablePrefabs)
        {
            CreatePool(prefab, 5);
        }
    }

    public void Clear()
    {
        _pool.Clear();
    }
}

[System.Serializable]
public class PoolablePrefabList
{
    public List<string> paths;
}
