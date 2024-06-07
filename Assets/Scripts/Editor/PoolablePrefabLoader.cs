using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PoolablePrefabLoader : Editor
{
    [MenuItem("Editor/Save Poolable Prefabs")]
    public static void SavePoolablePrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Prefabs" });
        List<string> poolablePrefabPaths = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null && prefab.GetComponent<Poolable>() != null)
            {
                string resourcePath = GetResourcePath(path);
                poolablePrefabPaths.Add(resourcePath);
                Debug.Log("Found Poolable Prefab: " + resourcePath);
            }
        }

        // JSON으로 경로 저장
        string json = JsonUtility.ToJson(new PoolablePrefabList(poolablePrefabPaths));
        File.WriteAllText(Application.dataPath + "/Resources/Data/PoolablePrefabs.json", json);
        AssetDatabase.Refresh();

        Debug.Log("Poolable Prefab paths saved.");
    }

    private static string GetResourcePath(string assetPath)
    {
        // "Assets/Resources/" 이후의 경로만 가져오고, ".prefab" 확장자를 제거
        int resourcesIndex = assetPath.IndexOf("Resources/");
        if (resourcesIndex != -1)
        {
            string relativePath = assetPath.Substring(resourcesIndex + "Resources/".Length);
            return Path.ChangeExtension(relativePath, null); // ".prefab" 확장자 제거
        }
        return null;
    }

    [System.Serializable]
    public class PoolablePrefabList
    {
        public List<string> paths;
        public PoolablePrefabList(List<string> paths)
        {
            this.paths = paths;
        }
    }
}
