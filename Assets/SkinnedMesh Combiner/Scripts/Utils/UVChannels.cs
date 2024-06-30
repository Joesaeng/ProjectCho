using System;

namespace SMC
{
    /// <summary>
    /// Enumeration representing the different UV channels ranging from 0-7 (all 8 channels). With the flags attribute, this
    /// enum can operate as a bitmask and be toggled individually with bitwise operations. Also works as a flags dropdown
    /// in the Unity Editor.
    /// </summary>
    [Flags][Serializable]
    public enum UVChannels
    {
        UV0 = 1 << 0,
        UV1 = 1 << 1,
        UV2 = 1 << 2,
        UV3 = 1 << 3,
        UV4 = 1 << 4,
        UV5 = 1 << 5,
        UV6 = 1 << 6,
        UV7 = 1 << 7,
    }
}
