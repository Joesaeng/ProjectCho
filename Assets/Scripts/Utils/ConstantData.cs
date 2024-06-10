using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConstantData
{
    #region 상수 데이터
    public const int    PopupUISortOrder = 10;
    public const int    SceneUISortOrder = 9;
    public const int    WorldSpaceUISortOrder = 8;

    public const int    LevelUpOptionsBasicCount = 3;
    #endregion

    public static readonly Color[] TextColorsByElementTypes = new Color[]
    {
        new Color(1f,0.28f,0.72f),  // Energy
        new Color(1f,0,0),          // Fire
        new Color(0f,0.3f,1f),      // Water
        new Color(0.4f,0,1f),       // Lightning
        new Color(0,0.2f,0),        // Earth
        new Color(0.25f,1,0.55f),   // Air
        new Color(1f,0.76f,0.33f),  // Light
        new Color(0,0,0.22f),       // Dark
    };
}
