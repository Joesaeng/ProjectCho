using System.Collections.Generic;
using UnityEngine;

namespace SMC
{
    /// <summary>
    /// Main point-of-entry for accessing the SkinnedMeshCombiner through an easy to use MonoBehaviour component
    /// </summary>
    public sealed class SkinnedMeshCombinerComponent : MonoBehaviour
    {
        public SkinnedMeshCombinerSettings Settings
        {
            get => m_Settings;
            set => m_Settings = value;
        }
        public SkinnedMeshRenderer RootSkinnedMeshRenderer
        {
            get => m_RootSkinnedMeshRenderer;
            set => m_RootSkinnedMeshRenderer = value;
        }

        [SerializeField] private List<Renderer> m_RenderersToCombine = new List<Renderer>();
        [SerializeField] private SkinnedMeshCombinerSettings m_Settings = new SkinnedMeshCombinerSettings
        {
            m_UVChannels = (UVChannels)255,
        };
        [SerializeField] private SkinnedMeshRenderer m_RootSkinnedMeshRenderer;
        [SerializeField] private bool m_CombineOnStart = true;

        private static int GetTextureWidth(Renderer renderer, int materialPropID)
        {
            if (renderer is SkinnedMeshRenderer smr)
            {
                if (smr.sharedMaterial)
                {
                    Texture t = smr.sharedMaterial.GetTexture(materialPropID);
                    if (t)
                        return t.width;
                }
            }
            else if (renderer is MeshRenderer mr)
            {
                if (mr.sharedMaterial)
                {
                    Texture t = mr.sharedMaterial.GetTexture(materialPropID);
                    if (t)
                    {
                        return t.width;
                    }
                }
            }

            return 0;
        }

        public void Combine(CombineMode combineMode = CombineMode.CombineAndReplace)
        {
            SkinnedMeshCombiner.Combine(this, combineMode);
        }

        public void SetRenderersToCombine(in List<Renderer> renderersToCombine)
        {
            m_RenderersToCombine = renderersToCombine;
        }

        public Renderer[] GetRenderersToCombine()
        {
            List<Renderer> renderers = new List<Renderer>(m_RenderersToCombine.Count);

            if (m_RootSkinnedMeshRenderer)
            {
                if (m_RootSkinnedMeshRenderer)
                {
                    AddRendererToList(m_RootSkinnedMeshRenderer);
                }
            }

            foreach (Renderer renderer in m_RenderersToCombine)
            {
                AddRendererToList(renderer);
            }

            return renderers.ToArray();

            void AddRendererToList(Renderer r)
            {
                if (r && !renderers.Contains(r))
                {
                    renderers.Add(r);
                }
            }
        }

        #if UNITY_EDITOR
        public int CanCombine(out string problemString)
        {
            // 2: Error
            // 1: Warning
            // 0: No issue

            problemString = string.Empty;

            if (!m_RootSkinnedMeshRenderer)
            {
                problemString = "No root skinned mesh renderer assigned!";

                return 2;
            }

            if (m_RenderersToCombine == null || m_RenderersToCombine.Count <= 0)
            {
                problemString = "No renderers assigned to combine!";

                return 2;
            }

            if (m_Settings.m_CombineBones && !HasMatchingBoneWeights(out string offendingSMRName))
            {
                problemString = $"Bones are set to be combined, but {offendingSMRName} has a mismatched armature and virtual bones are not enabled. Bones will not be combined!";

                return 1;
            }

            int materialPropID = Shader.PropertyToID("_MainTex");
            if
            (
                m_Settings.m_CombineTextures &&
                !HasMatchingTextureDimensions(materialPropID, out Renderer offendingRenderer)
            )
            {
                int
                    offendingTextureWidth = GetTextureWidth(offendingRenderer, materialPropID),
                    rootTextureWidth = GetTextureWidth(m_RootSkinnedMeshRenderer, materialPropID);
                problemString = $"Textures are set to be combined, but {offendingRenderer.name} has a mismatched albedo texture dimension ({offendingTextureWidth}x vs {rootTextureWidth}x) Its texture will be ignored!";

                return 1;
            }

            return 0;
        }
        #endif

        void Start()
        {
            if (m_CombineOnStart) { Combine(); }
        }

        private bool HasMatchingBoneWeights(out string offendingSMRName)
        {
            offendingSMRName = null;

            if (!m_RootSkinnedMeshRenderer) { return false; }

            int rootBoneAmount = m_RootSkinnedMeshRenderer.bones.Length;
            foreach (Renderer renderer in m_RenderersToCombine)
            {
                if (renderer && renderer is SkinnedMeshRenderer smr)
                {
                    if (!m_Settings.m_CreateVirtualBones && smr.bones.Length != rootBoneAmount)
                    {
                        offendingSMRName = smr.name;

                        return false;
                    }
                }
            }

            return true;
        }

        private bool HasMatchingTextureDimensions(int materialPropID, out Renderer offendingRenderer)
        {
            offendingRenderer = null;

            if (!m_RootSkinnedMeshRenderer) { return false; }

            int rootTextureWidth = GetTextureWidth(m_RootSkinnedMeshRenderer, materialPropID);
            foreach (Renderer renderer in m_RenderersToCombine)
            {
                if (renderer)
                {
                    if (GetTextureWidth(renderer, materialPropID) < rootTextureWidth)
                    {
                        offendingRenderer = renderer;

                        return false;
                    }
                }
            }

            return true;
        }
    }
}
