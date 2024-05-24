using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    #region Pool
    class Pool
    {
        public GameObject Original { get; private set; }

        Stack<Poolable> _poolStack = new Stack<Poolable>();

        public void Init(GameObject original, int count = 5)
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

            poolable.gameObject.SetActive(false);
            poolable.isUsing = false;

            _poolStack.Push(poolable);
        }

        public Poolable Pop()
        {
            Poolable poolable;

            if (_poolStack.Count > 0)
                poolable = _poolStack.Pop();
            else
                poolable = Create();

            poolable.gameObject.SetActive(true);

            poolable.isUsing = true;

            return poolable;
        }
    }
    #endregion

    Dictionary<string, Pool> _pool = new Dictionary<string, Pool>();
    Transform _root;

    // 풀을 만들고 데이터에 저장합니다.
    public void CreatePool(GameObject original, int count = 5)
    {
        Pool pool = new Pool();
        pool.Init(original, count);

        _pool.Add(original.name, pool);
    }

    /// <summary>
    /// 풀에 풀링오브젝트를 넣는다
    /// </summary>
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

    /// <summary>
    /// 풀에서 꺼내온다
    /// </summary>
    public Poolable Pop(GameObject original, Transform parent = null)
    {
        if (_pool.ContainsKey(original.name) == false)
        {
            CreatePool(original);
        }
        return _pool[original.name].Pop();
    }

    public GameObject GetOriginal(string name)
    {
        if (_pool.ContainsKey(name) == false)
            return null;

        return _pool[name].Original;
    }

    public void Clear()
    {
        foreach (Transform child in _root)
            GameObject.Destroy(child.gameObject);

        _pool.Clear();
    }
}
