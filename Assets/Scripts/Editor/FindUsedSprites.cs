using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;

public class FindAndCopyUsedSprites : EditorWindow
{
    private List<string> usedSprites = new List<string>();
    private HashSet<Sprite> uniqueSprites = new HashSet<Sprite>();
    private Vector2 scrollPos;
    private string targetPath = "Assets/Resources/UI/Useds";

    [MenuItem("Tools/Find and Copy Used Sprites")]
    public static void ShowWindow()
    {
        GetWindow<FindAndCopyUsedSprites>("Find and Copy Used Sprites");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Find and Copy Used Sprites"))
        {
            FindAllUsedSprites();
            CopyUsedSprites();
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var sprite in usedSprites)
        {
            EditorGUILayout.LabelField(sprite);
        }

        EditorGUILayout.EndScrollView();
    }

    private void FindAllUsedSprites()
    {
        usedSprites.Clear();
        uniqueSprites.Clear();
        string[] allAssets = AssetDatabase.GetAllAssetPaths();

        // Find used sprites in all scenes
        string[] scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
        foreach (string scenePath in scenes)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                Image[] images = obj.GetComponentsInChildren<Image>(true);
                foreach (Image image in images)
                {
                    if (image.sprite != null)
                    {
                        if (IsValidSpritePath(image.sprite) && !uniqueSprites.Contains(image.sprite))
                        {
                            uniqueSprites.Add(image.sprite);
                            usedSprites.Add($"{scenePath} - {obj.name} : {AssetDatabase.GetAssetPath(image.sprite)}");
                        }
                    }
                }
            }

            EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath(scenePath), true);
        }

        // Find used sprites in all prefabs
        foreach (string assetPath in allAssets)
        {
            if (assetPath.EndsWith(".prefab"))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null)
                {
                    Image[] images = prefab.GetComponentsInChildren<Image>(true);
                    foreach (Image image in images)
                    {
                        if (image.sprite != null)
                        {
                            if (IsValidSpritePath(image.sprite) && !uniqueSprites.Contains(image.sprite))
                            {
                                uniqueSprites.Add(image.sprite);
                                usedSprites.Add($"Prefab: {assetPath} - {prefab.name} : {AssetDatabase.GetAssetPath(image.sprite)}");
                            }
                        }
                    }
                }
            }
        }
    }

    private bool IsValidSpritePath(Sprite sprite)
    {
        string path = AssetDatabase.GetAssetPath(sprite);
        // Exclude package paths and unity_builtin_extra
        if (path.Contains("Packages/") || path.Contains("unity_builtin_extra"))
            return false;

        return true;
    }

    private void CopyUsedSprites()
    {
        foreach (var sprite in uniqueSprites)
        {
            string spritePath = AssetDatabase.GetAssetPath(sprite);
            string fileName = Path.GetFileName(spritePath);
            string destPath = Path.Combine(targetPath, fileName);

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            // Copy each individual sprite
            if (sprite != null)
            {
                string spriteName = sprite.name;
                string spriteFileName = $"{spriteName}.png";
                string spriteDestPath = Path.Combine(targetPath, spriteFileName);

                if (!File.Exists(spriteDestPath))
                {
                    File.Copy(spritePath, spriteDestPath);
                }
            }
        }

        AssetDatabase.Refresh();
    }
}
