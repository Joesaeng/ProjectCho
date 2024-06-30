using System;

namespace SMC
{
    /// <summary>
    /// Simple data structure for holding related settings used by the SkinnedMeshCombiner.
    /// </summary>
    [Serializable]
    public struct SkinnedMeshCombinerSettings
    {
        public UVChannels m_UVChannels;

        public bool
            m_CombineBlendshapes,
            m_CombineBones,
            m_CombineTextures,
            m_CombineVertexColours,
            m_CombineVertexNormals,
            m_CombineVertexTangents,
            m_CreateVirtualBones;
    }
}
