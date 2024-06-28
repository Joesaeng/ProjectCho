using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

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
            string fileName = Path.GetFileNameWithoutExtension(spritePath) + "_" + sprite.name + ".png";
            string destPath = Path.Combine(targetPath, fileName);

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            // Extract and save the individual sprite as PNG
            SaveSpriteAsPNG(sprite, destPath);
        }

        AssetDatabase.Refresh();
    }

    private void SaveSpriteAsPNG(Sprite sprite, string path)
    {
        Texture2D sourceTexture = sprite.texture;

        // Check if texture is readable
        if (!sourceTexture.isReadable)
        {
            Debug.LogError($"Texture {sourceTexture.name} is not readable. Please enable 'Read/Write Enabled' in the import settings.");
            return;
        }

        Rect rect = sprite.rect;
        int width = (int)rect.width;
        int height = (int)rect.height;
        Texture2D newTex = new Texture2D(width, height, TextureFormat.ARGB32, false);

        // Ensure we are not reading outside the bounds
        Color[] pixels = sourceTexture.GetPixels((int)rect.x, (int)rect.y, width, height);
        newTex.SetPixels(pixels);
        newTex.Apply();

        byte[] bytes = newTex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Object.DestroyImmediate(newTex);
    }
}
