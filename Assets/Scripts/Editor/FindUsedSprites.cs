using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
public class FindAndCopyUsedSprites : EditorWindow
{
    private List<string> usedSprites = new List<string>();
    private HashSet<Sprite> uniqueSprites = new HashSet<Sprite>();
    private Dictionary<Sprite, string> spritePathsBySprite = new Dictionary<Sprite, string>();
    private Dictionary<Sprite, Vector4> spriteBorders = new Dictionary<Sprite, Vector4>();
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
            UpdateSpriteReferences();
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
                            spriteBorders[image.sprite] = image.sprite.border;
                            usedSprites.Add($"{scenePath} - {obj.name} : {AssetDatabase.GetAssetPath(image.sprite)}");
                        }
                    }
                }
            }

            EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath(scenePath), true);
        }

        string folderPath = "Assets/Resources/Prefabs/UI";
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        for (int i = 0; i < prefabGUIDs.Length; i++)
        {
            string prefabGUID = prefabGUIDs[i];
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
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
                            spriteBorders[image.sprite] = image.sprite.border;
                            usedSprites.Add($"Prefab: {prefabPath} - {prefab.name} : {AssetDatabase.GetAssetPath(image.sprite)}");
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
            string fileName = Path.GetFileNameWithoutExtension(sprite.name) + ".png";
            string destPath = Path.Combine(targetPath, fileName);

            Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(destPath);

            if (sprite == newSprite)
            {
                Debug.Log($"{sprite.name} : 복사 필요 없음");
                continue;
            }

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            // Extract and save the individual sprite as PNG
            if (SaveSpriteAsPNG(sprite, destPath))
                spritePathsBySprite[sprite] = destPath;

        }

        AssetDatabase.Refresh();
    }

    private bool SaveSpriteAsPNG(Sprite sprite, string path)
    {
        Texture2D sourceTexture = sprite.texture;

        // Check if texture is readable
        if (!sourceTexture.isReadable)
        {
            Debug.LogError($"Texture {sourceTexture.name} is not readable. Please enable 'Read/Write Enabled' in the import settings.");
            return false;
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

        // Set texture type to sprite after saving
        SetTextureImporterSettings(sprite, path);
        return true;
    }

    private void SetTextureImporterSettings(Sprite originalSprite, string path)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        if (textureImporter != null)
        {
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteBorder = spriteBorders[originalSprite];
            textureImporter.SaveAndReimport();
        }
    }

    private void UpdateSpriteReferences()
    {
        // Update sprite references in all scenes
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
                        if (spritePathsBySprite.ContainsKey(image.sprite))
                        {

                            string newSpritePath = spritePathsBySprite[image.sprite];
                            Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(newSpritePath);
                            if (image.sprite == newSprite)
                            {
                                Debug.Log($"{image.sprite.name} : 업데이트 필요 없음");
                                continue;
                            }
                            image.sprite = newSprite;
                            EditorUtility.SetDirty(image);
                        }
                    }
                }
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetSceneByPath(scenePath));
            EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath(scenePath), true);
        }

        // Update sprite references in all prefabs
        string folderPath = "Assets/Resources/Prefabs/UI";
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        for (int i = 0; i < prefabGUIDs.Length; i++)
        {
            string prefabGUID = prefabGUIDs[i];
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                Image[] images = prefab.GetComponentsInChildren<Image>(true);
                foreach (Image image in images)
                {
                    if (image.sprite != null)
                    {
                        if (spritePathsBySprite.ContainsKey(image.sprite))
                        {
                            string newSpritePath = spritePathsBySprite[image.sprite];
                            Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(newSpritePath);
                            if (image.sprite == newSprite)
                            {
                                Debug.Log($"{image.sprite.name} : 업데이트 필요 없음");
                                continue;
                            }
                            image.sprite = newSprite;
                            EditorUtility.SetDirty(image);
                        }
                    }
                }

            }
        }

        AssetDatabase.SaveAssets();
    }
}
#endif