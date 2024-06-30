#define COMBINE_METALLIC // Undefine this if you don't want metallic combination to occur

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace SMC
{
    public static class SkinnedMeshCombiner
    {
        public static void Combine
        (
            SkinnedMeshCombinerComponent smc,
            CombineMode combineMode = CombineMode.CombineAndReplace
        )
        {
            if (!smc) { return; }

            GameObject targetGO = GetTargetGameObject(smc.gameObject, combineMode);
            smc = targetGO.GetComponent<SkinnedMeshCombinerComponent>();

            Combine
            (
                smc.GetRenderersToCombine(),
                targetGO,
                smc.Settings,
                smc.RootSkinnedMeshRenderer,
                combineMode
            );

            static GameObject GetTargetGameObject(GameObject go, CombineMode combineMode = CombineMode.CombineAndReplace)
            {
                if (combineMode is CombineMode.CombineToNew or CombineMode.CombineToPrefab)
                {
                    GameObject instance = UnityEngine.Object.Instantiate
                    (
                        go,
                        go.transform.position,
                        go.transform.rotation,
                        go.transform.parent
                    );
                    #if UNITY_EDITOR
                    Undo.RegisterCreatedObjectUndo(instance, $"Created {instance.name}");
                    #endif
                    instance.name = instance.name.Replace("(Clone)", " (Combined)");
                    instance.transform.SetSiblingIndex(go.transform.GetSiblingIndex() + 1);

                    return instance;
                }
                else
                {
                    return go;
                }
            }
        }
        public static void Combine
        (
            Renderer[] renderersToCombine,
            GameObject targetGO,
            SkinnedMeshCombinerSettings settings,
            SkinnedMeshRenderer rootSMR,
            CombineMode combineMode = CombineMode.CombineAndReplace
        )
        {
            /*
                Early return if collection has no root skinned mesh renderer
                Without a root skinned mesh renderer the combiner will be unable to find the data it needs.

                To combine regular mesh renderers, use Unity's method Mesh.CombineMeshes() instead
            */
            if (!rootSMR || rootSMR.sharedMesh == null)
            {
                Debug.LogError("SkinnedMesh Combiner: No valid root skinned mesh renderer provided!");

                return;
            }

            if (renderersToCombine?.Length == 0)
            {
                Debug.LogError("SkinnedMesh Combiner: No valid renderers to combine!");

                return;
            }

            // A collection of created virtual bones only kept to check before deleting transforms during cleanup
            HashSet<Transform> virtualBones = new HashSet<Transform>();
            // The target gameObject will differ depending on the combine mode
            SkinnedMeshRenderer targetSMR = GetTargetSkinnedMeshRenderer(targetGO);
            // Keeping track of old transform information to reset once combined
            Transform oldParent = targetGO.transform.parent;
            targetGO.transform.GetLocalPositionAndRotation(out Vector3 oldPos, out Quaternion oldRot);

            // Resetting gameObject's transform before performing combining, this is necessary for proper bindpose creation
            #if UNITY_EDITOR
            Undo.SetCurrentGroupName("SkinnedMeshCombiner Combine Operation");
            Undo.RegisterFullObjectHierarchyUndo(targetGO, "Hierarchy Undo");
            Undo.SetTransformParent(targetGO.transform, null, "Set Transform Parent");
            #else
            targetGO.transform.parent = null;
            #endif
            targetGO.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            Mesh combinedMesh = new Mesh();
            Mesh[] meshesToCombine = GetMeshesToCombine(renderersToCombine, settings.m_CreateVirtualBones);

            if (meshesToCombine?.Length == 0)
            {
                Debug.LogWarning("SkinnedMeshCombiner: No valid meshes to combine!");

                return;
            }

            if
            (
                !CombineMeshGeometry
                (
                    meshesToCombine,
                    combinedMesh,
                    settings.m_CombineVertexColours,
                    settings.m_CombineVertexNormals,
                    settings.m_CombineVertexTangents
                )
            )
            {
                return;
            }

            // Ensuring the combined mesh bounds takes into account all of its new parts
            combinedMesh.bounds = GetCombinedMeshBounds();

            if (settings.m_CombineBlendshapes)
                CombineMeshBlendshapes(meshesToCombine, combinedMesh);

            if (settings.m_CombineBones)
                CombineBones();

            if (settings.m_UVChannels != 0)
                CombineMaterialAndUVs();

            if (settings.m_CombineTextures)
            {
                // Main Texture Combining
                CombineMainTexture(true);

                #if COMBINE_METALLIC
                // Metallic Texture Combining
                if (CombineTexture(Shader.PropertyToID("_MetallicGlossMap")))
                {
                    // Required to update metallic shading if wasn't enabled before
                    targetSMR.sharedMaterial.EnableKeyword("_METALLICGLOSSMAP"); 
                }
                #endif
            }

            targetSMR.sharedMesh = combinedMesh; // Final assignment of combined mesh after all operations are complete

            CleanupRenderers();

            // Re-assigning previous transform
            #if UNITY_EDITOR
            Undo.SetTransformParent(targetGO.transform, oldParent, "Set Transform Parent");

            if (combineMode == CombineMode.CombineToPrefab)
                SavePrefab(targetSMR, settings);

            if (targetGO && targetGO.TryGetComponent(out SkinnedMeshCombinerComponent smc))
                Undo.DestroyObjectImmediate(smc);

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            #else
            targetGO.transform.parent = oldParent;

            if (targetGO && targetGO.TryGetComponent(out SkinnedMeshCombinerComponent smc))
                UnityEngine.Object.DestroyImmediate(smc);
            #endif

            if (targetGO)
                targetGO.transform.SetLocalPositionAndRotation(oldPos, oldRot);

            static Mesh[] GetMeshesToCombine(Renderer[] renderersToCombine, bool createVirtualBones = false)
            {
                if (renderersToCombine == null) { return null; }

                // Collection of all found and valid meshes to combine
                Dictionary<string, Mesh> meshTable = new Dictionary<string, Mesh>(renderersToCombine.Length);

                foreach (Renderer renderer in renderersToCombine)
                {
                    if (renderer == null) continue;

                    if (renderer is MeshRenderer mr)
                    {
                        AddMeshRendererMeshToTable(mr);
                    }
                    else if (renderer is SkinnedMeshRenderer smr)
                    {
                        AddSkinnedMeshRendererMeshToTable(smr);
                    }
                    else
                    {
                        Debug.LogWarning
                        (
                            $"SkinnedMeshCombiner: Ignoring Renderer {renderer.name}..." +
                            $"\n(Reason: {renderer.name} is neither a SkinnedMeshRenderer nor a MeshRenderer + MeshFilter)"
                        );
                    }
                }
                Mesh[] meshesToCombine = new Mesh[meshTable.Count];
                meshTable.Values.CopyTo(meshesToCombine, 0);

                return meshesToCombine;

                void AddMeshRendererMeshToTable(MeshRenderer mr)
                {
                    // Mesh renderers don't store a mesh, it is stored in the MeshFilter on the same GameObject
                    if (mr.TryGetComponent(out MeshFilter mf))
                    {
                        if (mf.sharedMesh != null)
                        {
                            AddMeshToTable
                            (
                                mf.sharedMesh,
                                Matrix4x4.TRS(mr.transform.position, mr.transform.rotation, mr.transform.localScale)
                            );
                        }
                        else
                        {
                            Debug.LogWarning
                            (
                                $"SkinnedMeshCombiner: Ignoring Renderer {mr.name}..." +
                                $"\n(Reason: {mr.name} has no mesh assigned to its accompanying MeshFilter)"
                            );
                        }
                    }
                    else
                    {
                        Debug.LogWarning
                        (
                            $"SkinnedMeshCombiner: Ignoring Renderer {mr.name}..." +
                            $"\n(Reason: {mr.name} has no accompanying MeshFilter)"
                        );
                    }
                }

                void AddMeshToTable(Mesh mesh, Matrix4x4 worldMatrix)
                {
                    // All meshesToCombine MUST have Read/Write access enabled or the data can't be read
                    if (mesh.isReadable)
                    {
                        string meshName = mesh.name.ToLower();
                        if (!meshTable.ContainsKey(meshName))
                        {
                            Mesh meshInstance = UnityEngine.Object.Instantiate(mesh);

                            // Baking all mesh vertices to its world position before combining
                            if (!createVirtualBones)
                            {
                                Vector3[] v = new Vector3[meshInstance.vertexCount];
                                for (int j = 0; j < meshInstance.vertexCount; j++)
                                {
                                    v[j] = worldMatrix.MultiplyPoint3x4(meshInstance.vertices[j]);
                                }
                                meshInstance.vertices = v;
                            }

                            meshTable.TryAdd(meshName, meshInstance);
                        }
                    }
                    else
                    {
                        Debug.LogWarning
                        (
                            $"SkinnedMeshCombiner: Ignoring Mesh {mesh.name}..." +
                            $"\n(Reason: Mesh {mesh.name} is not Read/Write enabled)"
                        );
                    }
                }

                void AddSkinnedMeshRendererMeshToTable(SkinnedMeshRenderer smr)
                {
                    if (smr.sharedMesh != null)
                    {
                        // All meshesToCombine MUST have Read/Write access enabled or the data can't be read
                        if (smr.sharedMesh.isReadable)
                        {
                            AddMeshToTable
                            (
                                smr.sharedMesh,
                                Matrix4x4.TRS(smr.transform.position, smr.transform.rotation, smr.transform.localScale)
                            );
                        }
                        else
                        {
                            Debug.LogWarning
                            (
                                $"SkinnedMeshCombiner: Ignoring Mesh {smr.sharedMesh.name}..." +
                                $"\n(Reason: Mesh {smr.sharedMesh.name} is not Read/Write enabled)"
                            );
                        }
                    }
                    else
                    {
                        Debug.LogWarning
                        (
                            $"SkinnedMeshCombiner: Ignoring Renderer {smr.name}..." +
                            $"\n(Reason: {smr.name} has no mesh assigned)"
                        );
                    }
                }
            }

            static SkinnedMeshRenderer GetTargetSkinnedMeshRenderer(GameObject go)
            {
                #if UNITY_EDITOR
                if (!go.TryGetComponent(out SkinnedMeshRenderer targetSMR))
                    targetSMR = (SkinnedMeshRenderer)Undo.AddComponent(go, typeof(SkinnedMeshRenderer));
                #else
                if (!go.TryGetComponent(out SkinnedMeshRenderer targetSMR))
                    targetSMR = go.AddComponent<SkinnedMeshRenderer>();
                #endif

                return targetSMR;
            }

            void CleanupRenderers()
            {
                Action<UnityEngine.Object> immediateDestroyAction;
                #if UNITY_EDITOR
                immediateDestroyAction = Undo.DestroyObjectImmediate;
                #else
                immediateDestroyAction = UnityEngine.Object.DestroyImmediate;
                #endif

                for (int i = 0; i < renderersToCombine.Length; i++)
                {
                    DestroyRenderer(renderersToCombine[i]);
                }

                void DestroyRenderer(Renderer r)
                {
                    if (!virtualBones.Contains(r.transform))
                    {
                        immediateDestroyAction?.Invoke(r.gameObject);
                    }
                    else
                    {
                        if (r.TryGetComponent(out MeshFilter mf))
                            immediateDestroyAction?.Invoke(mf);

                        immediateDestroyAction?.Invoke(r);
                    }
                }
            }

            void CombineBones()
            {
                List<Matrix4x4> bindposes = new List<Matrix4x4>(rootSMR.sharedMesh.bindposes);
                List<Transform> bones = new List<Transform>(rootSMR.bones);
                Transform rootBone = rootSMR.rootBone;

                if (!HasMatchingBoneWeights(out string offendingSMRName))
                {
                    Debug.LogWarning($"SkinnedMeshCombiner: Meshes being combined do not share a valid armature and virtual bones are not enabled! ('{offendingSMRName}' does not match target bone count: {bones.Count}) Aborting! If you have access to this model, please re-export the meshesToCombine weighted to the same armature.");

                    return;
                }

                // If no armature was found within renderers, can't combine
                if (bones.Count == 0 || rootBone == null)
                {
                    Debug.LogWarning("SkinnedMeshCombiner: No valid armature found in source SkinnedMeshRenderers!");
                }
                else
                {
                    if (rootBone.parent != null && rootBone.parent.localRotation != Quaternion.identity)
                        Debug.LogWarning("SkinnedMeshCombiner: Armature parent is rotated, mesh binding might not work properly!");

                    for (int i = 0; i < renderersToCombine.Length; i++)
                    {
                        if (renderersToCombine[i] == null) continue;

                        // Calculate boneweights for MeshRenderer which by nature of its type doesn't have any
                        if (renderersToCombine[i] is MeshRenderer mr)
                        {
                            Transform bone = GetParentBone(mr.transform);
                            if (bone != null)
                            {
                                BoneWeight[] boneWeights = new BoneWeight[meshesToCombine[i].vertexCount];
                                int boneIndex = bones.IndexOf(bone);
                                for (int j = 0; j < meshesToCombine[i].vertexCount; j++)
                                {
                                    boneWeights[j] = new BoneWeight
                                    {
                                        boneIndex0 = boneIndex,
                                        weight0 = 1
                                    };
                                }
                                meshesToCombine[i].boneWeights = boneWeights;
                            }
                        }
                        // Calculate boneweights for a SkinnedMeshRenderer which has none or is from a different armature
                        else if
                        (
                            renderersToCombine[i] is SkinnedMeshRenderer smr &&
                            (smr.bones.Length == 0 || smr.rootBone != rootBone)
                        )
                        {
                            Transform bone = GetParentBone(smr.transform);
                            if (bone != null)
                            {
                                BoneWeight[] boneWeights = new BoneWeight[meshesToCombine[i].vertexCount];
                                int boneIndex = bones.IndexOf(bone);
                                for (int j = 0; j < meshesToCombine[i].vertexCount; j++)
                                {
                                    boneWeights[j] = new BoneWeight
                                    {
                                        boneIndex0 = boneIndex,
                                        weight0 = 1
                                    };
                                }
                                meshesToCombine[i].boneWeights = boneWeights;
                            }
                        }
                    }

                    // Assigning armature related fields to combined mesh before combining
                    combinedMesh.bindposes = bindposes.ToArray();
                    targetSMR.bones = bones.ToArray();
                    targetSMR.rootBone = rootBone;

                    // Actually performing the boneweight combining operation
                    if (CombineMeshBones(meshesToCombine, combinedMesh))
                    {
                        Animator animator = targetGO.GetComponentInChildren<Animator>();
                        if (animator != null) animator.Rebind();
                    }
                }

                Transform GetParentBone(Transform source)
                {
                    if (settings.m_CreateVirtualBones)
                    {
                        return CreateVirtualBone();
                    }
                    else
                    {
                        Transform curr = source;
                        do
                        {
                            if (bones.Contains(curr))
                            {
                                return curr;
                            }
                            else
                            {
                                curr = curr.parent;
                            }
                        } while (curr.parent != null);

                        return CreateVirtualBone();
                    }

                    Transform CreateVirtualBone()
                    {
                        bindposes.Add(source.worldToLocalMatrix * source.localToWorldMatrix);
                        bones.Add(source);
                        virtualBones.Add(source);

                        return source;
                    }
                }

                bool HasMatchingBoneWeights(out string offendingSMRName)
                {
                    offendingSMRName = null;

                    if (!rootSMR) { return false; }

                    int rootBoneAmount = rootSMR.bones.Length;
                    foreach (Renderer renderer in renderersToCombine)
                    {
                        if (renderer && renderer is SkinnedMeshRenderer smr)
                        {
                            if (!settings.m_CreateVirtualBones && smr.bones.Length != rootBoneAmount)
                            {
                                offendingSMRName = smr.name;

                                return false;
                            }
                        }
                    }

                    return true;
                }
            }

            void CombineMaterialAndUVs()
            {
                if (CombineMeshUVs(meshesToCombine, combinedMesh, settings.m_UVChannels))
                {
                    targetSMR.sharedMaterial = new Material(rootSMR.sharedMaterial)
                    {
                        name = "Combined Material"
                    };
                }
            }

            Bounds GetCombinedMeshBounds()
            {
                Bounds combinedBounds = meshesToCombine[0].bounds;

                for (int i = 0; i < meshesToCombine.Length; i++)
                {
                    if (i > 0)
                        combinedBounds.Encapsulate(meshesToCombine[i].bounds);
                }

                return combinedBounds;
            }

            Dictionary<Mesh, Texture2D> GetMaterialMainTextureDictionary()
            {
                Dictionary<Mesh, Texture2D> textureMeshMap = new Dictionary<Mesh, Texture2D>();
                foreach (Renderer renderer in renderersToCombine)
                {
                    if (!renderer)
                        continue;

                    GetMeshAndTexture(renderer, out Mesh mesh, out Texture2D tex);

                    if (mesh)
                        textureMeshMap.TryAdd(mesh, tex);
                }

                return textureMeshMap;

                void GetMeshAndTexture(Renderer renderer, out Mesh mesh, out Texture2D tex)
                {
                    mesh = null;
                    tex = null;

                    if (renderer is SkinnedMeshRenderer smr)
                    {
                        mesh = smr.sharedMesh;

                        // Ensuring mesh, material and texture are not empty
                        if (smr.sharedMesh && smr.sharedMaterial)
                        {
                            mesh = smr.sharedMesh;

                            Texture t = smr.sharedMaterial.mainTexture;
                            if (t && t is Texture2D t2D)
                            {
                                // All textures MUST have Read/Write access enabled or the data can't be read
                                if (t2D.isReadable)
                                    tex = t2D;
                                else
                                    Debug.LogWarning($"SkinnedMeshCombiner: Texture {t2D.name} is not Read/Write enabled, unable to combine it!");
                            }
                        }
                    }
                    else if (renderer is MeshRenderer mr)
                    {
                        if (mr.TryGetComponent(out MeshFilter mf))
                        {
                            mesh = mf.sharedMesh;

                            if (mf.sharedMesh && mr.sharedMaterial)
                            {
                                Texture t = mr.sharedMaterial.mainTexture;
                                if (t && t is Texture2D t2D)
                                {
                                    // All textures MUST have Read/Write access enabled or the data can't be read
                                    if (t2D.isReadable)
                                        tex = t2D;
                                    else
                                        Debug.LogWarning($"SkinnedMeshCombiner: Texture {t2D.name} is not Read/Write enabled, unable to combine it!");
                                }
                            }
                        }
                    }
                }
            }

            Dictionary<Mesh, Texture2D> GetMaterialTextureDictionary(int propertyID)
            {
                Dictionary<Mesh, Texture2D> textureMeshMap = new Dictionary<Mesh, Texture2D>();
                foreach (Renderer renderer in renderersToCombine)
                {
                    if (!renderer)
                        continue;

                    GetMeshAndTexture(renderer, out Mesh mesh, out Texture2D tex);

                    if (mesh)
                        textureMeshMap.TryAdd(mesh, tex);
                }

                return textureMeshMap;

                void GetMeshAndTexture(Renderer renderer, out Mesh mesh, out Texture2D tex)
                {
                    mesh = null;
                    tex = null;

                    if (renderer is SkinnedMeshRenderer smr)
                    {
                        mesh = smr.sharedMesh;

                        // Ensuring mesh, material and texture are not empty
                        if (smr.sharedMesh && smr.sharedMaterial)
                        {
                            mesh = smr.sharedMesh;

                            Texture t = smr.sharedMaterial.GetTexture(propertyID);
                            if (t && t is Texture2D t2D)
                            {
                                // All textures MUST have Read/Write access enabled or the data can't be read
                                if (t2D.isReadable)
                                    tex = t2D;
                                else
                                    Debug.LogWarning($"SkinnedMeshCombiner: Texture {t2D.name} is not Read/Write enabled, unable to combine it!");
                            }
                        }
                    }
                    else if (renderer is MeshRenderer mr)
                    {
                        if (mr.TryGetComponent(out MeshFilter mf))
                        {
                            mesh = mf.sharedMesh;

                            if (mf.sharedMesh && mr.sharedMaterial)
                            {
                                Texture t = mr.sharedMaterial.GetTexture(propertyID);
                                if (t && t is Texture2D t2D)
                                {
                                    // All textures MUST have Read/Write access enabled or the data can't be read
                                    if (t2D.isReadable)
                                        tex = t2D;
                                    else
                                        Debug.LogWarning($"SkinnedMeshCombiner: Texture {t2D.name} is not Read/Write enabled, unable to combine it!");
                                }
                            }
                        }
                    }
                }
            }

            bool CombineMainTexture(bool offsetUVs = false)
            {
                Dictionary<Mesh, Texture2D> textureMeshMap = GetMaterialMainTextureDictionary();
                return
                    textureMeshMap != null &&
                    CombineMeshMainTextures
                    (
                        textureMeshMap,
                        combinedMesh,
                        targetSMR.sharedMaterial,
                        settings.m_UVChannels,
                        offsetUVs
                    );
            }

            bool CombineTexture(int propertyID, bool offsetUVs = false)
            {
                Dictionary<Mesh, Texture2D> textureMeshMap = GetMaterialTextureDictionary(propertyID);
                return
                    textureMeshMap != null &&
                    CombineMeshTextures
                    (
                        textureMeshMap,
                        combinedMesh,
                        targetSMR.sharedMaterial,
                        settings.m_UVChannels,
                        propertyID,
                        offsetUVs
                    );
            }
        }

        /// <summary>
        /// Combines blendshapes sourced from the given array into the combined mesh.
        /// </summary>
        /// <param name="meshesToCombine">Source mesh collection to read blendshapes from</param>
        /// <param name="combinedMesh">Reference to combined mesh blendshapes will be assigned to</param>
        /// <returns>bool Success of the operation</returns>
        public static bool CombineMeshBlendshapes(in Mesh[] meshesToCombine, in Mesh combinedMesh)
        {
            try
            {
                // List of blendshape data, to be used to create blendshapes in combined mesh
                List<BlendshapeData> blendshapes = new List<BlendshapeData>();
                int vIndex = 0;
                for (int i = 0; i < meshesToCombine.Length; i++)
                {
                    if (meshesToCombine[i] == null) { continue; }

                    Mesh meshToCombine = meshesToCombine[i];
                    for (int j = 0; j < meshToCombine.blendShapeCount; j++)
                    {
                        // Getting required data from blendshapes of this mesh
                        string name = $"{meshToCombine.GetBlendShapeName(j)}";
                        Vector3[] deltaVertices = new Vector3[meshToCombine.vertexCount];
                        Vector3[] deltaNormals = new Vector3[meshToCombine.vertexCount];
                        Vector3[] deltaTangents = new Vector3[meshToCombine.vertexCount];
                        meshToCombine.GetBlendShapeFrameVertices(j, 0, deltaVertices, deltaNormals, deltaTangents);
                        blendshapes.Add(new BlendshapeData(deltaVertices, deltaNormals, deltaTangents, name, vIndex));
                    }

                    vIndex += meshesToCombine[i].vertexCount;
                }

                foreach (BlendshapeData blendshape in blendshapes)
                {
                    // Constructing new arrays from collected blendshape
                    Vector3[] deltaVertices = new Vector3[combinedMesh.vertices.Length];
                    Vector3[] deltaNormals = new Vector3[combinedMesh.vertices.Length];
                    Vector3[] deltaTangents = new Vector3[combinedMesh.vertices.Length];

                    blendshape.deltaVertices.CopyTo(deltaVertices, blendshape.index);
                    blendshape.deltaNormals.CopyTo(deltaNormals, blendshape.index);
                    blendshape.deltaTangents.CopyTo(deltaTangents, blendshape.index);

                    // Actually adding the blendshape frame to the combined mesh using the given arrays
                    // Only supports a single blendshape frame (0-100 weight)
                    combinedMesh.AddBlendShapeFrame(blendshape.name, 100, deltaVertices, deltaNormals, deltaTangents);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SkinnedMeshCombiner: {e.Message}");

                return false;
            }
        }

        /// <summary>
        /// Combines boneweights sourced from the given array into the combined mesh.
        /// </summary>
        /// <param name="meshesToCombine">Source mesh collection to read boneweights from</param>
        /// <param name="combinedMesh">Reference to combined mesh that will be skinned</param>
        /// <returns>bool Success of the operation</returns>
        public static bool CombineMeshBones(in Mesh[] meshesToCombine, in Mesh combinedMesh)
        {
            try
            {
                // Boneweights must equal the vertex count, each boneweight corresponds to a specific vertex
                BoneWeight[] boneWeights = new BoneWeight[combinedMesh.vertexCount];

                // Iterating through all meshes and adding their boneweights to the proper index of the boneweights array
                int vIndex = 0;
                for (int i = 0; i < meshesToCombine.Length; i++)
                {
                    if (meshesToCombine[i] == null) { continue; }

                    Mesh meshToCombine = meshesToCombine[i];
                    for (int j = 0; j < meshesToCombine[i].boneWeights.Length; j++)
                    {
                        boneWeights[j + vIndex] = meshToCombine.boneWeights[j];
                    }
                    vIndex += meshesToCombine[i].vertexCount;
                }

                combinedMesh.boneWeights = boneWeights;

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SkinnedMeshCombiner: {e.Message} {e.StackTrace}");

                return false;
            }
        }

        /// <summary>
        /// Procedurally creates a combined mesh using all of the geometry from the source mesh collection.
        /// </summary>
        /// <param name="meshesToCombine">Source mesh collection to combine</param>
        /// <param name="combinedMesh">Reference to combined mesh that will be an amalgamation of all the source meshes</param>
        /// <returns>bool Success of the operation</returns>
        public static bool CombineMeshGeometry
        (
            in Mesh[] meshesToCombine,
            in Mesh combinedMesh,
            bool combineColours,
            bool combineNormals,
            bool combineTangents
        )
        {
            try
            {
                int vertexCount = 0;
                for (int i = 0; i < meshesToCombine.Length; i++)
                {
                    if (meshesToCombine[i]) { vertexCount += meshesToCombine[i].vertexCount; }
                }

                // Creating all required collections
                Vector3[] vertices = new Vector3[vertexCount];
                Color[] colours = new Color[vertexCount];
                Vector3[] normals = new Vector3[vertexCount];
                Vector4[] tangents = new Vector4[vertexCount];
                List<int> triangles = new List<int>();

                int vIndex = 0;
                for (int i = 0; i < meshesToCombine.Length; i++)
                {
                    if (meshesToCombine[i] == null) { continue; }

                    Mesh meshToCombine = meshesToCombine[i];

                    // For each vertex, there is a corresponding normal, tangent and vertex colour
                    for (int j = 0; j < meshToCombine.vertexCount; j++)
                    {
                        int index = j + vIndex;

                        vertices[index] = meshToCombine.vertices[j];

                        if (combineColours && j < meshToCombine.colors.Length)
                            colours[index] = meshToCombine.colors[j];
                        if (combineNormals && j < meshToCombine.normals.Length)
                            normals[index] = meshToCombine.normals[j];
                        if (combineTangents && j < meshToCombine.tangents.Length)
                            tangents[index] = meshToCombine.tangents[j];
                    }

                    // Creating triangles from new vertex index
                    for (int j = 0; j < meshesToCombine[i].triangles.Length; j++)
                    {
                        triangles.Add(meshToCombine.triangles[j] + vIndex);
                    }

                    vIndex += meshesToCombine[i].vertexCount;
                }

                // Assignment of all mesh geometry collections
                combinedMesh.vertices = vertices;
                combinedMesh.colors = colours;
                combinedMesh.normals = normals;
                combinedMesh.tangents = tangents;
                combinedMesh.triangles = triangles.ToArray();

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SkinnedMeshCombiner: {e.Message}");

                return false;
            }
        }

        /// <summary>
        /// Combines all defined UV channels of all meses into the combined mesh's UV channels.
        /// </summary>
        /// <param name="meshesToCombine">Source mesh collection to read UVs from</param>
        /// <param name="combinedMesh">Reference to combined mesh to assign UVs to</param>
        /// <param name="uvChannels">Flags representing which UV channels should be processed and which ignored</param>
        /// <returns>bool Success of the operation</returns>
        public static bool CombineMeshUVs(in Mesh[] meshesToCombine, in Mesh combinedMesh, UVChannels uvChannels = 0)
        {
            try
            {
                // If no channels need to be combined
                if (uvChannels == 0) { return true; }

                // All UV channels will be the same size as the vertex collection
                Vector2[] uvs = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs2 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs3 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs4 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs5 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs6 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs7 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs8 = new Vector2[combinedMesh.vertexCount];

                int vIndex = 0;
                for (int i = 0; i < meshesToCombine.Length; i++)
                {
                    if (meshesToCombine[i] == null) { continue; }

                    // Copying each UV channels to the new collection at the specified position if applicable
                    Mesh meshToCombine = meshesToCombine[i];
                    if ((uvChannels & UVChannels.UV0) != 0)
                        meshToCombine.uv.CopyTo(uvs, vIndex);
                    if ((uvChannels & UVChannels.UV1) != 0)
                        meshToCombine.uv2.CopyTo(uvs2, vIndex);
                    if ((uvChannels & UVChannels.UV2) != 0)
                        meshToCombine.uv3.CopyTo(uvs3, vIndex);
                    if ((uvChannels & UVChannels.UV3) != 0)
                        meshToCombine.uv4.CopyTo(uvs4, vIndex);
                    if ((uvChannels & UVChannels.UV4) != 0)
                        meshToCombine.uv5.CopyTo(uvs5, vIndex);
                    if ((uvChannels & UVChannels.UV5) != 0)
                        meshToCombine.uv6.CopyTo(uvs6, vIndex);
                    if ((uvChannels & UVChannels.UV6) != 0)
                        meshToCombine.uv7.CopyTo(uvs7, vIndex);
                    if ((uvChannels & UVChannels.UV7) != 0)
                        meshToCombine.uv8.CopyTo(uvs8, vIndex);

                    vIndex += meshToCombine.vertexCount;
                }

                // Assignment of all UV channels to the new values
                combinedMesh.uv = uvs;
                combinedMesh.uv2 = uvs2;
                combinedMesh.uv3 = uvs3;
                combinedMesh.uv4 = uvs4;
                combinedMesh.uv5 = uvs5;
                combinedMesh.uv6 = uvs6;
                combinedMesh.uv7 = uvs7;
                combinedMesh.uv8 = uvs8;

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SkinnedMeshCombiner: {e.Message}");

                return false;
            }
        }

        /// <summary>
        /// Combines all renderer material textures together and scales UVs to correspond to the newly created texture atlas
        /// </summary>
        /// <param name="textureMeshMap">A hash table allowing meshes to access their assigned texture</param>
        /// <param name="combinedMesh">Reference to combined mesh to apply texture to after combining</param>
        /// <param name="combinedMaterial">The material that will be given the newly combined texture</param>
        /// <param name="uvChannels">Flags representing which UV channels should be processed and scaled</param>
        /// <returns>bool Success of the operation</returns>
        public static bool CombineMeshMainTextures(Dictionary<Mesh, Texture2D> textureMeshMap, Mesh combinedMesh, in Material combinedMaterial, UVChannels uvChannels, bool offsetUVs = false)
        {
            try
            {
                if (textureMeshMap != null && textureMeshMap.Count > 1)
                {
                    int textureResolution = 0;
                    foreach (KeyValuePair<Mesh, Texture2D> item in textureMeshMap)
                    {
                        if (item.Value != null)
                        {
                            textureResolution = item.Value.width;

                            break;
                        }
                    }

                    if (textureResolution == 0) { return false; }

                    Color32[] emptyCols = new Color32[textureResolution * textureResolution];
                    Array.Fill(emptyCols, Color.clear);
                    Texture2D emptyTex = new Texture2D(textureResolution, textureResolution);
                    emptyTex.SetPixels32(emptyCols);
                    emptyTex.Apply();

                    List<Texture2D> textureList = new List<Texture2D>();
                    foreach (KeyValuePair<Mesh, Texture2D> item in textureMeshMap)
                    {
                        if (item.Value != null && item.Value.width == textureResolution)
                            textureList.Add(item.Value);
                        else
                            textureList.Add(emptyTex);
                    }

                    if
                    (
                        TextureCombiner.CombineTextures
                        (
                            textureList.ToArray(),
                            out Texture2D combinedTexture,
                            out Dictionary<Texture2D, Vector2Int> textureCoordinates,
                            out int atlasResolution
                        ) &&
                        offsetUVs
                    )
                    {
                        // If successful, adjust combined mesh UV coordinates accordingly
                        OffsetCombinedMeshUVs(textureCoordinates, combinedTexture, atlasResolution);
                    }

                    // Applying newly created combined texture to combined material
                    combinedMaterial.mainTexture = combinedTexture;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                return false;
            }

            void OffsetCombinedMeshUVs(in Dictionary<Texture2D, Vector2Int> textureCoordinates, in Texture2D combinedTexture, int atlasResolution)
            {
                if (textureCoordinates == null || uvChannels == 0) return;

                Vector2[] uvs = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs2 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs3 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs4 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs5 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs6 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs7 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs8 = new Vector2[combinedMesh.vertexCount];

                // Reverse-engineering the texture slot size from the atlas
                int textureWidth = combinedTexture.width / atlasResolution;
                int vIndex = 0;

                foreach (KeyValuePair<Mesh, Texture2D> item in textureMeshMap)
                {
                    if (!item.Key || !item.Value || !textureCoordinates.TryGetValue(item.Value, out Vector2Int coords))
                        continue;

                    // Calculating where the UV should be offset to match up to the texture on this new atlas
                    Vector2 offset = new Vector2
                    (
                        coords.x * textureWidth / (float)combinedTexture.width,
                        coords.y * textureWidth / (float)combinedTexture.width
                    );

                    // Performing scaling and offsetting of UVs to ensure they are moved to their new texture location
                    // This is done for each UV channel specified
                    Mesh mesh = item.Key;
                    if ((uvChannels & UVChannels.UV0) != 0)
                    {
                        for (int j = 0; j < mesh.uv.Length; j++)
                            uvs[j + vIndex] = (mesh.uv[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV1) != 0)
                    {
                        for (int j = 0; j < mesh.uv2.Length; j++)
                            uvs2[j + vIndex] = (mesh.uv2[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV2) != 0)
                    {
                        for (int j = 0; j < mesh.uv3.Length; j++)
                            uvs3[j + vIndex] = (mesh.uv3[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV3) != 0)
                    {
                        for (int j = 0; j < mesh.uv4.Length; j++)
                            uvs4[j + vIndex] = (mesh.uv4[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV4) != 0)
                    {
                        for (int j = 0; j < mesh.uv5.Length; j++)
                            uvs5[j + vIndex] = (mesh.uv5[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV5) != 0)
                    {
                        for (int j = 0; j < mesh.uv6.Length; j++)
                            uvs6[j + vIndex] = (mesh.uv6[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV6) != 0)
                    {
                        for (int j = 0; j < mesh.uv7.Length; j++)
                            uvs7[j + vIndex] = (mesh.uv7[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV7) != 0)
                    {
                        for (int j = 0; j < mesh.uv8.Length; j++)
                            uvs8[j + vIndex] = (mesh.uv8[j] / atlasResolution) + offset;
                    }

                    vIndex += mesh.vertexCount;
                }

                // Re-assigning all UV channels to combined mesh
                combinedMesh.uv = uvs;
                combinedMesh.uv2 = uvs2;
                combinedMesh.uv3 = uvs3;
                combinedMesh.uv4 = uvs4;
                combinedMesh.uv5 = uvs5;
                combinedMesh.uv6 = uvs6;
                combinedMesh.uv7 = uvs7;
                combinedMesh.uv8 = uvs8;
            }
        }

        public static bool CombineMeshTextures(Dictionary<Mesh, Texture2D> textureMeshMap, Mesh combinedMesh, in Material combinedMaterial, UVChannels uvChannels, int propertyID, bool offsetUVs = false)
        {
            try
            {
                if (textureMeshMap != null && textureMeshMap.Count > 1)
                {
                    int textureResolution = 0;
                    foreach (KeyValuePair<Mesh, Texture2D> item in textureMeshMap)
                    {
                        if (item.Value != null)
                        {
                            textureResolution = item.Value.width;

                            break;
                        }
                    }

                    if (textureResolution == 0) { return false; }

                    Color32[] emptyCols = new Color32[textureResolution * textureResolution];
                    Array.Fill(emptyCols, Color.clear);
                    Texture2D emptyTex = new Texture2D(textureResolution, textureResolution);
                    emptyTex.SetPixels32(emptyCols);
                    emptyTex.Apply();

                    List<Texture2D> textureList = new List<Texture2D>();
                    foreach (KeyValuePair<Mesh, Texture2D> item in textureMeshMap)
                    {
                        if (item.Value != null && item.Value.width == textureResolution)
                            textureList.Add(item.Value);
                        else
                            textureList.Add(emptyTex);
                    }

                    if
                    (
                        TextureCombiner.CombineTextures
                        (
                            textureList.ToArray(),
                            out Texture2D combinedTexture,
                            out Dictionary<Texture2D, Vector2Int> textureCoordinates,
                            out int atlasResolution
                        ) &&
                        offsetUVs
                    )
                    {
                        // If successful, adjust combined mesh UV coordinates accordingly
                        OffsetCombinedMeshUVs(textureCoordinates, combinedTexture, atlasResolution);
                    }

                    // Applying newly created combined texture to combined material
                    combinedMaterial.SetTexture(propertyID, combinedTexture);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                return false;
            }

            void OffsetCombinedMeshUVs(in Dictionary<Texture2D, Vector2Int> textureCoordinates, in Texture2D combinedTexture, int atlasResolution)
            {
                if (textureCoordinates == null || uvChannels == 0) return;

                Vector2[] uvs = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs2 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs3 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs4 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs5 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs6 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs7 = new Vector2[combinedMesh.vertexCount];
                Vector2[] uvs8 = new Vector2[combinedMesh.vertexCount];

                // Reverse-engineering the texture slot size from the atlas
                int textureWidth = combinedTexture.width / atlasResolution;
                int vIndex = 0;

                foreach (KeyValuePair<Mesh, Texture2D> item in textureMeshMap)
                {
                    if (!item.Key || !item.Value || !textureCoordinates.TryGetValue(item.Value, out Vector2Int coords))
                        continue;

                    // Calculating where the UV should be offset to match up to the texture on this new atlas
                    Vector2 offset = new Vector2
                    (
                        coords.x * textureWidth / (float)combinedTexture.width,
                        coords.y * textureWidth / (float)combinedTexture.width
                    );

                    // Performing scaling and offsetting of UVs to ensure they are moved to their new texture location
                    // This is done for each UV channel specified
                    Mesh mesh = item.Key;
                    if ((uvChannels & UVChannels.UV0) != 0)
                    {
                        for (int j = 0; j < mesh.uv.Length; j++)
                            uvs[j + vIndex] = (mesh.uv[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV1) != 0)
                    {
                        for (int j = 0; j < mesh.uv2.Length; j++)
                            uvs2[j + vIndex] = (mesh.uv2[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV2) != 0)
                    {
                        for (int j = 0; j < mesh.uv3.Length; j++)
                            uvs3[j + vIndex] = (mesh.uv3[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV3) != 0)
                    {
                        for (int j = 0; j < mesh.uv4.Length; j++)
                            uvs4[j + vIndex] = (mesh.uv4[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV4) != 0)
                    {
                        for (int j = 0; j < mesh.uv5.Length; j++)
                            uvs5[j + vIndex] = (mesh.uv5[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV5) != 0)
                    {
                        for (int j = 0; j < mesh.uv6.Length; j++)
                            uvs6[j + vIndex] = (mesh.uv6[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV6) != 0)
                    {
                        for (int j = 0; j < mesh.uv7.Length; j++)
                            uvs7[j + vIndex] = (mesh.uv7[j] / atlasResolution) + offset;
                    }
                    if ((uvChannels & UVChannels.UV7) != 0)
                    {
                        for (int j = 0; j < mesh.uv8.Length; j++)
                            uvs8[j + vIndex] = (mesh.uv8[j] / atlasResolution) + offset;
                    }

                    vIndex += mesh.vertexCount;
                }

                // Re-assigning all UV channels to combined mesh
                combinedMesh.uv = uvs;
                combinedMesh.uv2 = uvs2;
                combinedMesh.uv3 = uvs3;
                combinedMesh.uv4 = uvs4;
                combinedMesh.uv5 = uvs5;
                combinedMesh.uv6 = uvs6;
                combinedMesh.uv7 = uvs7;
                combinedMesh.uv8 = uvs8;
            }
        }

        private static void SavePrefab(SkinnedMeshRenderer targetSMR, SkinnedMeshCombinerSettings settings)
        {
            #if UNITY_EDITOR
            try
            {
                string name = targetSMR.gameObject.name;
                string folderGuid = AssetDatabase.CreateFolder("Assets", name);
                string folderPath = AssetDatabase.GUIDToAssetPath(folderGuid);

                if
                (
                    settings.m_CombineTextures &&
                    targetSMR.sharedMaterial != null &&
                    targetSMR.sharedMaterial.mainTexture != null
                )
                {
                    Texture2D tempTex = (Texture2D)targetSMR.sharedMaterial.mainTexture;
                    byte[] bytes = ((Texture2D)targetSMR.sharedMaterial.mainTexture).EncodeToPNG();
                    if (bytes != null)
                    {
                        string
                            textureFileName = $"{name} Texture.png",
                            absoluteTexturePath = $"{Application.dataPath}/{name}/{textureFileName}",
                            relativeTexturePath = $"{folderPath}/{textureFileName}";
                        File.WriteAllBytes(absoluteTexturePath, bytes);
                        AssetDatabase.ImportAsset(relativeTexturePath);
                        Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(relativeTexturePath, typeof(Texture2D));

                        Material matInstance = new Material(targetSMR.sharedMaterial)
                        {
                            mainTexture = tex
                        };
                        AssetDatabase.CreateAsset(targetSMR.sharedMaterial, $"{folderPath}/{name} Material.mat");
                    }

                    targetSMR.sharedMaterial.mainTexture = tempTex;
                }

                // create asset from mesh snapshot
                AssetDatabase.CreateAsset(targetSMR.sharedMesh, $"{folderPath}/{name} Mesh.asset");

                // save gameobject clone as prefab in assets folder
                string finalPath = $"{folderPath}/{targetSMR.gameObject.name}.prefab";
                PrefabUtility.SaveAsPrefabAsset(targetSMR.gameObject, finalPath);
                AssetDatabase.Refresh();

                Debug.Log($"Created and saved prefab to {finalPath}");

                Undo.DestroyObjectImmediate(targetSMR.gameObject);
            }
            catch (Exception e)
            {
                Debug.LogError($"SkinnedMeshCombiner: {e.Message}");
            }
            #endif
        }
    }
}
