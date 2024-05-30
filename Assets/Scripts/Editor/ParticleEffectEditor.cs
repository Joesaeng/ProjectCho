using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ParticleEffectEditor : Editor
{
    [MenuItem("Editor/ParticleEffect Optimization")]
    private static void ParticleEffectOptimization()
    {
        // Load ParticleEffect Prefabs
        string folderPath = "Assets/Resources/Prefabs/ParticleEffects";
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        for (int i = 0; i < prefabGUIDs.Length; i++)
        {
            string prefabGUID = prefabGUIDs[i];
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            // 파티클 이펙트에 Poolable컴포넌트룰 추가하고
            go.GetOrAddComponent<Poolable>();

            int maxParticles = 25;
            ParticleSystem[] particles = go.GetComponentsInChildren<ParticleSystem>();
            for (int j = 0; j < particles.Length; j++)
            {
                // MaxParticles 수 조정
                var main = particles[j].main;
                main.maxParticles = maxParticles;
                // 그림자 Off
                ParticleSystemRenderer renderer = particles[j].GetComponent<ParticleSystemRenderer>();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            }
        }

        // Save
        AssetDatabase.SaveAssets();

        Debug.Log("파티클이펙트 최적화!");
    }
}
