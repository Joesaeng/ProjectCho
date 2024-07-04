using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ParticleCombiner : EditorWindow
{
    [MenuItem("Tools/Particle Prefab Processor")]
    public static void ShowWindow()
    {
        GetWindow<ParticleCombiner>("Particle Prefab Processor");
    }

    private string prefabFolderPath = "Assets/YourPrefabFolder"; // 원본 프리팹 폴더 경로
    private string outputFolderPath = "Assets/YourOutputFolder"; // 병합 텍스쳐 저장 폴더 경로
    private Dictionary<Material, List<Texture>> materialTextureMap = new Dictionary<Material, List<Texture>>();
    private Dictionary<Vector2Int, Dictionary<Texture2D, string>> textureToNameMap = new Dictionary<Vector2Int, Dictionary<Texture2D, string>>(); // 텍스쳐 이름을 저장할 딕셔너리
    private Dictionary<Vector2Int,Dictionary<int,Material>> newMaterialDict = new();

    private void OnGUI()
    {
        GUILayout.Label("Particle Prefab Processor", EditorStyles.boldLabel);
        prefabFolderPath = EditorGUILayout.TextField("Prefab Folder Path", prefabFolderPath);
        outputFolderPath = EditorGUILayout.TextField("Output Folder Path", outputFolderPath);

        if (GUILayout.Button("Process Prefabs"))
        {
            ProcessPrefabs();
        }

        if (GUILayout.Button("Merge Textures"))
        {
            MergeTextures();
        }

        if (materialTextureMap.Count > 0)
        {
            GUILayout.Label("Found Materials and Textures:", EditorStyles.boldLabel);

            foreach (var kvp in materialTextureMap)
            {
                GUILayout.Label("Material: " + kvp.Key.name);

                foreach (var texture in kvp.Value)
                {
                    GUILayout.Label(" - Texture: " + texture.name);
                }
            }
        }
    }

    private void ProcessPrefabs()
    {
        materialTextureMap.Clear();
        textureToNameMap.Clear();
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolderPath });

        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab != null)
            {
                ParticleSystem[] particleSystems = prefab.GetComponentsInChildren<ParticleSystem>();

                foreach (var ps in particleSystems)
                {
                    if (ps.textureSheetAnimation.enabled)
                        continue;
                    var renderer = ps.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null)
                    {
                        foreach (var mat in renderer.sharedMaterials)
                        {
                            if (mat != null)
                            {
                                if (mat.GetFloat("_Surface") == 0f) // Opaque 타입의 머티리얼 건너뛰기
                                    continue;
                                if (!materialTextureMap.ContainsKey(mat))
                                {
                                    materialTextureMap[mat] = new List<Texture>();
                                }

                                foreach (var textureProperty in mat.GetTexturePropertyNames())
                                {
                                    var texture = mat.GetTexture(textureProperty);
                                    if (texture != null && !materialTextureMap[mat].Contains(texture))
                                    {
                                        materialTextureMap[mat].Add(texture);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        AssetDatabase.Refresh();
    }

    private void MergeTextures()
    {
        Dictionary<Vector2Int, List<Texture2D>> textureGroups = new Dictionary<Vector2Int, List<Texture2D>>();

        // 텍스쳐를 크기별로 그룹화
        foreach (var kvp in materialTextureMap)
        {
            foreach (var texture in kvp.Value)
            {
                Texture2D texture2D = texture as Texture2D;
                if (texture2D != null)
                {
                    Vector2Int size = new Vector2Int(texture2D.width, texture2D.height);
                    if (!textureGroups.ContainsKey(size))
                    {
                        textureGroups[size] = new List<Texture2D>();
                        textureToNameMap[size] = new Dictionary<Texture2D, string>();
                    }
                    if (!textureToNameMap[size].ContainsKey(texture2D))
                    {
                        textureToNameMap[size][texture2D] = texture2D.name;
                        textureGroups[size].Add(texture2D);
                    }
                }
            }
        }

        // 각 그룹별로 텍스쳐 병합 및 복사 진행
        foreach (var group in textureGroups)
        {
            Vector2Int size = group.Key;
            List<Texture2D> textures = group.Value;

            // 병합된 텍스쳐의 크기를 결정
            int atlasWidth = 4096;
            int atlasHeight = 4096;
            int currentX = 0;
            int currentY = 0;
            int maxX = 0;
            int maxY = 0;

            Texture2D atlas = new Texture2D(atlasWidth, atlasHeight);
            List<Texture2D> currentTextures = new List<Texture2D>();
            Color[] atlasPixels = new Color[atlasWidth * atlasHeight];

            for (int i = 0; i < textures.Count; i++)
            {
                Texture2D texture = textures[i];
                Color[] texturePixels = texture.GetPixels();

                if (currentX + size.x > atlasWidth)
                {
                    currentX = 0;
                    currentY += size.y;
                }

                if (currentY + size.y > atlasHeight)
                {
                    SaveAtlas(atlas, currentTextures, atlasPixels, size, maxX, maxY);
                    atlas = new Texture2D(atlasWidth, atlasHeight);
                    atlasPixels = new Color[atlasWidth * atlasHeight];
                    currentTextures.Clear();
                    currentX = 0;
                    currentY = 0;
                    maxX = 0;
                    maxY = 0;
                }

                for (int y = 0; y < size.y; y++)
                {
                    for (int x = 0; x < size.x; x++)
                    {
                        int atlasIndex = ((currentY + y) * atlasWidth) + (currentX + x);
                        int textureIndex = (y * size.x) + x;

                        if (atlasIndex < atlasPixels.Length && textureIndex < texturePixels.Length)
                        {
                            atlasPixels[atlasIndex] = texturePixels[textureIndex];
                        }
                    }
                }

                currentTextures.Add(texture);
                maxX = Mathf.Max(maxX, currentX + size.x);
                maxY = Mathf.Max(maxY, currentY + size.y);
                currentX += size.x;
            }

            if (currentTextures.Count > 0)
            {
                SaveAtlas(atlas, currentTextures, atlasPixels, size, maxX, maxY);
            }
        }

        // 프리팹에서 머티리얼 업데이트
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolderPath }))
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab != null)
            {
                bool prefabModified = false;
                ParticleSystem[] particleSystems = prefab.GetComponentsInChildren<ParticleSystem>();

                foreach (var ps in particleSystems)
                {
                    if (ps.textureSheetAnimation.enabled)
                        continue;
                    var renderer = ps.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null)
                    {
                        Material material = renderer.sharedMaterial;

                        if (material != null)
                        {
                            if (material.GetFloat("_Surface") == 0f) // Opaque 타입의 머티리얼 건너뛰기
                                continue;
                            if (material.mainTexture == null)
                                continue;
                            Vector2Int size = new Vector2Int(material.mainTexture.width, material.mainTexture.height);
                            string atlasPath;
                            Sprite[] sprites;
                            string spriteName;
                            Sprite targetSprite = null;
                            Dictionary<int,Material> newMaterials = newMaterialDict[size];
                            foreach (var mat in newMaterials.Values)
                            {
                                atlasPath = AssetDatabase.GetAssetPath(mat.mainTexture);
                                sprites = AssetDatabase.LoadAllAssetsAtPath(atlasPath).OfType<Sprite>().ToArray();
                                spriteName = textureToNameMap[size][material.mainTexture as Texture2D];

                                targetSprite = sprites.FirstOrDefault(s => s.name == spriteName);
                                if (targetSprite != null)
                                {
                                    renderer.sharedMaterial = mat;
                                    prefabModified = true;
                                    break;
                                }
                                continue;
                            }
                            if (!prefabModified)
                            {
                                Debug.LogError("뭔가 잘못됨");
                                return;
                            }

                            var textureSheetAnimation = ps.textureSheetAnimation;
                            textureSheetAnimation.enabled = true;
                            textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites;
                            textureSheetAnimation.rowMode = ParticleSystemAnimationRowMode.Custom;

                            if (targetSprite != null)
                                textureSheetAnimation.SetSprite(0, targetSprite);
                        }
                    }
                }

                if (prefabModified)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
                }
            }
        }

        AssetDatabase.Refresh();
    }

    private void SaveAtlas(Texture2D atlas, List<Texture2D> currentTextures, Color[] atlasPixels, Vector2Int size, int maxX, int maxY)
    {
        Texture2D trimmedAtlas = new Texture2D(maxX, maxY);
        Dictionary<Vector2Int, Material> newMaterials = new();
        Color[] trimmedPixels = new Color[maxX * maxY];
        int atlasNum = 0;

        for (int y = 0; y < maxY; y++)
        {
            for (int x = 0; x < maxX; x++)
            {
                trimmedPixels[(y * maxX) + x] = atlasPixels[(y * 4096) + x];
            }
        }

        trimmedAtlas.SetPixels(trimmedPixels);
        trimmedAtlas.Apply();

        string atlasPath = Path.Combine(outputFolderPath, "Atlas_"+ atlasNum + "_" + size.x + "x" + size.y + ".png");
        while (AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath) != null)
        {
            atlasNum++;
            atlasPath = Path.Combine(outputFolderPath, "Atlas_" + atlasNum + "_" + size.x + "x" + size.y + ".png");
        }
        File.WriteAllBytes(atlasPath, trimmedAtlas.EncodeToPNG());
        AssetDatabase.ImportAsset(atlasPath);
        Texture2D newAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);

        // TextureImporter를 사용하여 TextureType을 Sprite로 변경하고, Sprite Mode를 Multiple로 설정
        string assetPath = AssetDatabase.GetAssetPath(newAtlas);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            // Slice를 Grid by Cell Size로 설정
            importer.spritePixelsPerUnit = size.x;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            // 스프라이트 슬라이싱 설정
            List<SpriteMetaData> spriteMetaData = new List<SpriteMetaData>();
            int currentX = 0;
            int currentY = 0;

            foreach (var texture in currentTextures)
            {
                if (currentX + size.x > 4096)
                {
                    currentX = 0;
                    currentY += size.y;
                }

                spriteMetaData.Add(new SpriteMetaData
                {
                    alignment = (int)SpriteAlignment.Center,
                    border = new Vector4(0, 0, 0, 0),
                    name = texture.name,
                    rect = new Rect(currentX, currentY, size.x, size.y)
                });

                currentX += size.x;
            }

            importer.spritesheet = spriteMetaData.ToArray();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        // 새로운 텍스쳐로 머티리얼 업데이트 및 저장
        Material newMaterial = null;
        foreach (var kvp in materialTextureMap)
        {
            var mat = kvp.Key;
            if (newMaterials.ContainsKey(size))
            {
                newMaterial = newMaterials[size];
            }
            else
            {
                newMaterial = new Material(mat); // Create a copy of the original material
                newMaterials[size] = newMaterial;

                foreach (var textureProperty in mat.GetTexturePropertyNames())
                {
                    foreach (var texture in kvp.Value)
                    {
                        if (mat.GetTexture(textureProperty) == texture)
                        {
                            newMaterial.SetTexture(textureProperty, newAtlas);
                        }
                    }
                }
                if (newMaterialDict.ContainsKey(size))
                    newMaterialDict[size][atlasNum] = newMaterial;
                else
                {
                    Dictionary<int,Material> newMatDict = new();
                    newMatDict[atlasNum] = newMaterial;
                    newMaterialDict[size] = newMatDict;
                }

                string newMaterialPath = Path.Combine(outputFolderPath, "Material_"+ atlasNum + "_" + size.x + "x" + size.y + ".mat");
                AssetDatabase.CreateAsset(newMaterial, newMaterialPath);
            }
        }
    }


}
