using Define;
using Interfaces;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Diagnostics;

public interface IData
{
    public int Id { get; }
}

[Serializable]
public enum ElementType
{
                // 상성이 아닌 속성에는 75%의 데미지
                // 동일 속성 적(Light,Dark 제외)에게는 50%의 데미지.
                // Light,Dark 속성은 다른 속성에게 50%의 데미지만 받음
    Energy,     // 모든 속성의 적(어둠,빛 제외) 100% 데미지
    Fire,       // Wind 속성의 적에게 200% 데미지, Water 속성 적에게 50% 데미지
    Water,      // Fire 속성 적에게 200% 데미지, Lightning 속성 적에게 50% 데미지
    Lightning,  // Water 속성 적에게 200% 데미지, Earth 속성 적에게 50% 데미지
    Earth,      // Lightning 속성 적에게 200% 데미지, Wind 속성 적에게 50% 데미지
    Wind,       // Earth 속성 적에게 200% 데미지, Fire 속성 적에게 50% 데미지
    // Light,      // Dark속성 적에게 300% 데미지, Light 속성 적에게 0% 데미지
    // Dark,       // Light속성 적에게 300% 데미지, Dark 속성 적에게 0% 데미지
}

[Serializable]
public enum SpellBehaviorType
{
    TargetedProjectile,
    TargetedProjectileOfExplosion,
    StraightProjectile,
    TargetedAOE,
    Summon,
}

[Serializable]
public enum SpellRarity
{
    Normal,
    Rare,
    Epic,
    Legend,
}

[Serializable]
public enum SpellUpgradeType
{
    IncreaseDamage,
    IncreaseSize,
    IncreasePierce,
    DecreaseSpellDelay,
    AddProjectile,
}

[Serializable]
public enum ItemType
{
    None,
    Equipment,
}

[Serializable]
public enum EquipmentType
{
    Weapon,
    Ring
}

[Serializable]
public enum SummonType
{
    Weapon,
    Ring,
    Spell,
}

[Serializable]
public enum EquipmentRarity
{
    Normal,
    Rare,
    Epic,
    Legend,
}

[Serializable]
public enum StatusType
{
    Spell,
    BaseDamage,
    IncreaseDamage,
    DecreaseSpellDelay,
    AddProjectile,
    IncreasePierce,
    IncreaseEnergySpellDamage,
    IncreaseFireSpellDamage,
    IncreaseWaterSpellDamage,
    IncreaseLightningSpellDamage,
    IncreaseEarthSpellDamage,
    IncreaseWindSpellDamage,
    // . . .
}

[Serializable]
public enum AchievementType
{
    Main,
    Weekly,
    Daily,
    Repeat,
}
[Serializable]
public enum RewardType
{
    RewardDia,
    RewardCoins,
    RewardStatus,
}
[Serializable]
public enum AchievementTargetType
{
    DefeatEnemies,
    StageClear,
    Summon,
}


namespace Data
{
    [Serializable]
    public class BaseEnemyData : IData
    {
        int IData.Id => id;

        public int id;
        public string prefabName;
        public ElementType elementType;
        public float baseHp;
        public float baseMoveSpeed;
        public float baseAttackDamage;
        public float baseAttackDelay;
        public float baseAttackRange;
        public bool isRange;
        public int projectileId;
    }

    public interface ISpellEffectData
    {

    }

    [Serializable]
    public class ProjectileData : IData, ISpellEffectData
    {
        int IData.Id => id;
        public int id;
        public float baseMoveSpeed;
        public string projectileName;
        public string explosionName;
    }

    [Serializable]
    public class AOEEffectData : IData, ISpellEffectData
    {
        int IData.Id => id;
        public int id;
        public string effectName;
        public string explosionName;
    }

    [Serializable]
    public class BaseSpellData : IData, ISpellData
    {
        int IData.Id => id;
        public int id;
        public int effectId;
        public SpellBehaviorType spellBehaviorType;
        public SpellRarity spellRarity;
        public MagicianAnim animType;
        public ElementType elementType;
        public string spellName;
        public int pierceCount;
        public float spellSpeed;
        public float spellRange;
        public float spellSize;

        #region ISpellData
        public int SpellId => id;
        public int EffectId => effectId;
        public SpellBehaviorType SpellBehaviorType => spellBehaviorType;
        public MagicianAnim AnimType => animType;
        public ElementType ElementType => elementType;
        public string SpellName => spellName;
        public int PierceCount => pierceCount;
        public float SpellSpeed => spellSpeed;
        public float SpellRange => spellRange;
        public float SpellSize => spellSize;
        public float SpellDamageCoefficient => spellDataByLevel[0].spellDamageCoefficient;
        public float SpellDelay => spellDataByLevel[0].spellDelay;
        public int IntegerParam1 => integerParam1;
        public int IntegerParam2 => integerParam2;
        public float FloatParam1 => floatParam1;
        public float FloatParam2 => floatParam2;
        #endregion

        public List<BaseSpellDataByLevel> spellDataByLevel;

        // Optional fields
        public int integerParam1;
        public int integerParam2;
        public float floatParam1;
        public float floatParam2;
    }

    [Serializable]
    public class BaseSpellDataByLevel
    {
        public int spellLevel;
        public int requireSpellCountToLevelup;
        public float spellDamageCoefficient;
        public float spellDelay;
    }

    [Serializable]
    public class SpellUpgradeData
    {
        public int spellId;
        public SpellUpgradeType spellUpgradeType;
        public int integerValue;
        public float floatValue;
    }

    [Serializable]
    public class SpellUpgradeDatas : IData
    {
        int IData.Id => id;
        public int id;
        public int spellId;
        public List<SpellUpgradeData> spellUpgradeDatas;
    }

    [Serializable]
    public class WaveRewardData
    {
        public RewardType type;
        public int value;
    }

    [Serializable]
    public class StageRewardData
    {
        public RewardType type;
        public int value;
    }

    [Serializable]
    public class WaveData
    {
        public List<int> waveEnemyIds;
        public WaveRewardData waveRewardData;
        public int spawnEnemyCount;
        public float damageCoefficient;
        public float hpCoefficient;
    }

    [Serializable]
    public class StageData : IData
    {
        int IData.Id => id;
        public int id;
        public StageRewardData firstClearRewardData;
        public List<int> stageEnemysId;
        public List<WaveData> waveDatas;
    }

    [Serializable]
    public class ItemData : IData
    {
        int IData.Id => id;
        public int id;
        public ItemType itemType;
        public string itemName;
        public string itemSpriteName;
    }

    [Serializable]
    public class EquipmentOptionData : IData ,IRandomWeighted
    {
        int IData.Id => id;
        public int Weight { get => weight; }
        public int id;
        public List<EquipmentType> capableOfEquipmentType;
        public EquipmentRarity requireRarity;
        public StatusType optionType;
        public string prefix;
        public int weight;
        public int intParam1;
        public int intParam2;
        public float floatParam1;
        public float floatParam2;
    }

    [Serializable]
    public class EquipmentData : ItemData
    {
        public EquipmentRarity rarity;
        public EquipmentType equipmentType;
        public List<EquipmentOptionData> equipmentOptions;
        public bool isEquip;
        public int equipSlotIndex;
    }
    
    [Serializable]
    public class AchievementData : IData
    {
        int IData.Id => id;
        public int id;
        public string achievementName;
        public AchievementType type;
        public AchievementTargetData target;
        public List<AchievementRewardData> rewards;
        public bool isCompleted;
    }

    [Serializable]
    public class AchievementTargetData
    {
        public AchievementTargetType type;
        public ElementType elementType;
        public SummonType summonType;
        public int targetValue;
        public int progressValue;
    }

    [Serializable]
    public class AchievementRewardData
    {
        public RewardType type;
        public StatusType statusType;
        public int integerParam;
        public float floatParam;
    }

    [Serializable]
    public class Datas<T> : ILoader<int, T> where T : IData
    {
        public List<T> datas = new();

        public Dictionary<int, T> MakeDict()
        {
            Dictionary<int, T> dict = new();
            foreach (T data in datas)
            {
                dict.Add(data.Id, data);
            }
            return dict;
        }
    }

    [Serializable]
    public class LanguageData
    {
        public string key;
        public string eng;
        public string kr;
    }

    [Serializable]
    public class LanguageDatas : ILoader<string, LanguageData>
    {
        public List<LanguageData> datas = new();

        public Dictionary<string, LanguageData> MakeDict()
        {
            Dictionary<string, LanguageData> dict = new();
            foreach (LanguageData data in datas)
            {
                dict.Add(data.key, data);
            }
            return dict;
        }
    }
}
