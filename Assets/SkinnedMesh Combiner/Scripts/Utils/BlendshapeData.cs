using UnityEngine;

namespace SMC
{
    /// <summary>
    /// Utility data structure containing blendshape related fields including the blendshape deltas, name and index
    /// </summary>
    internal readonly struct BlendshapeData
    {
        public readonly Vector3[] deltaVertices, deltaNormals, deltaTangents;
        public readonly string name;
        public readonly int index;

        /// <summary>
        /// Construct new BlendshapeData
        /// </summary>
        /// <param name="_dv">DeltaVertices, representing the change to apply to each vertex </param>
        /// <param name="_dn">DeltaNormals, representing the change to apply to each normal</param>
        /// <param name="_dt" DeltaTangents, representing the change to apply to each tangent></param>
        /// <param name="_n">Name of this Blendshape</param>
        /// <param name="_i">Index of this Blendshape within the parent SkinnedMeshRenderer</param>
        public BlendshapeData(Vector3[] _dv, Vector3[] _dn, Vector3[] _dt, string _n, int _i)
        {
            deltaVertices = _dv;
            deltaNormals = _dn;
            deltaTangents = _dt;
            name = _n;
            index = _i;
        }
    }
}
