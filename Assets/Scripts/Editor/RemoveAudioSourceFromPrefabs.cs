using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR

public class RemoveAudioSourceFromPrefabs : EditorWindow
{
    [MenuItem("Tools/Remove AudioSource from Prefabs")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(RemoveAudioSourceFromPrefabs));
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Remove AudioSource from Prefabs"))
        {
            RemoveAudioSourceFromAllPrefabs();
        }
    }

    private static void RemoveAudioSourceFromAllPrefabs()
    {
        string[] prefabPaths = Directory.GetFiles("Assets/Resources/Prefabs", "*.prefab", SearchOption.AllDirectories);

        foreach (string prefabPath in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                bool hasChanges = false;
                AudioSource[] audioSources = prefab.GetComponentsInChildren<AudioSource>(true);
                foreach (AudioSource audioSource in audioSources)
                {
                    DestroyImmediate(audioSource, true);
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                    Debug.Log($"Removed AudioSource from prefab: {prefabPath}");
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Completed removing AudioSource from all prefabs.");
    }
}
#endif