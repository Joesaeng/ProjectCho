using System;

namespace SMC
{
    /// <summary>
    /// Defines different combine mode states to direct mesh combination behaviour
    /// </summary>
    [Serializable]
    public enum CombineMode : byte
    {
        CombineAndReplace = 0,
        CombineToNew = 1,
        CombineToPrefab = 2
    }
}
