using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConstantData
{
    #region 상수 데이터
    public const int    PopupUISortOrder = 10;
    public const int    SceneUISortOrder = 9;
    public const int    WorldSpaceUISortOrder = 8;

    public const float  OneWaveTime = 10f;

    public const int    LevelUpOptionsBasicCount = 3;
    public const int    MaxRingSlots = 4;

    public const int    UsingSpellCount = 5;

    public const int    CoinCostForSummonWeapon = 1000;
    public const int    CoinCostForSummonRing = 500;
    public const int    CoinCostForSummonSpell = 500;

    public const int    DiaCostForSummonWeapon = 100;
    public const int    DiaCostForSummonRing = 50;
    public const int    DiaCostForSummonSpell = 50;
    #endregion

    public static readonly Color[] TextColorsByElementTypes = new Color[]
    {
        new Color(1f,0.28f,0.72f),  // Energy
        new Color(1f,0,0),          // Fire
        new Color(0f,0.3f,1f),      // Water
        new Color(0.4f,0,1f),       // Lightning
        new Color(0,0.2f,0),        // Earth
        new Color(0.25f,1,0.55f),   // Wind
        new Color(1f,0.76f,0.33f),  // Light
        new Color(0,0,0.22f),       // Dark
    };

    public static readonly Color[] TextColorsByRarity = new Color[]
    {
        new Color(0.33f,1f,0),      // Normal
        new Color(0.64f,0.27f,0),   // Rare
        new Color(0.70f,0f,1f),     // Epic
        new Color(1f,0.87f,0f),     // Legend
    };
}
