using UnityEngine;
using static UnityEngine.UI.Image;

public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0)
                name = name.Substring(index + 1);

            GameObject go = Managers.Pool.GetOriginal(name);
            if (go != null)
                return go as T;
        }
        return Resources.Load<T>(path);
    }

    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        if (original.TryGetComponent<Poolable>(out Poolable poolable))
            return Managers.Pool.Pop(original, parent).gameObject;

        GameObject go;
        go = Object.Instantiate(original, parent);

        go.name = original.name;

        return go;
    }

    public GameObject Instantiate(string path, Vector3 pos)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        GameObject go;

        if (original.TryGetComponent<Poolable>(out Poolable poolable))
        {
            go = Managers.Pool.Pop(original).gameObject;
            go.transform.SetPositionAndRotation(pos, Quaternion.identity);
            return go;
        }
        go = Object.Instantiate(original, pos, Quaternion.identity);

        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        if (go.TryGetComponent<Poolable>(out Poolable poolable))
        {
            Managers.Pool.Push(poolable);
            return;
        }

        Object.Destroy(go);
    }
}
