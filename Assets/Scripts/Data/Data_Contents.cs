using Define;
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
public enum SpellType
{
    TargetedProjectile,
    TargetedProjectileOfExplosion,
    StraightProjectile,
    TargetedAOE,
    Summon,
}

[Serializable]
public enum ElementType
{
                // 상성이 아닌 속성에는 75%의 데미지
                // 동일 속성 적(Light,Dark 제외)에게는 50%의 데미지.
                // Light,Dark 속성은 다른 속성에게 50%의 데미지만 받음
    Energy,     // 모든 속성의 적(어둠,빛 제외) 100% 데미지
    Fire,       // Air 속성의 적에게 200% 데미지, Water 속성 적에게 50% 데미지
    Water,      // Fire 속성 적에게 200% 데미지, Lightning 속성 적에게 50% 데미지
    Lightning,  // Water 속성 적에게 200% 데미지, Earth 속성 적에게 50% 데미지
    Earth,      // Lightning 속성 적에게 200% 데미지, Air 속성 적에게 50% 데미지
    Air,        // Earth 속성 적에게 200% 데미지, Fire 속성 적에게 50% 데미지
    Light,      // Dark속성 적에게 300% 데미지, Light 속성 적에게 0% 데미지
    Dark,       // Light속성 적에게 300% 데미지, Dark 속성 적에게 0% 데미지
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
    IncreaseAirSpellDamage,
    IncreaseLightSpellDamage,
    IncreaseDarkSpellDamage,

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
public enum AchievementRewardType
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
    public class InventoryData
    {
        public List<ItemData> inventoryItemsDatas;
        public List<ItemData> equipmentDatas;

    }
    [Serializable]
    public class PlayerData 
    {
        public bool beginner = true;
        public int gameLanguage = (int)Define.GameLanguage.English;
        public float bgmVolume = 1f;
        public float sfxVolume = 1f;
        public bool bgmOn = true;
        public bool sfxOn = true;
        public InventoryData inventoryData;
        public List<int> stageClearList;
        public List<AchievementData> achievementDatas;
    }

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
    public class BaseSpellData : IData
    {
        int IData.Id => id;
        public int id;
        public int effectId;
        public SpellType spellType;
        public MagicianAnim animType;
        public ElementType elementType;
        public string spellName;
        public float spellDamage;
        public float spellDelay;
        public float spellRange;
        public float spellSpeed;
        public float spellDuration;
        public float spellSize;
        public int pierceCount;

        // Optional fields
        public float? explosionDamage;
        public float? explosionRange;
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
    public class WaveData
    {
        public List<int> waveEnemyIds;
        public int spawnEnemyCount;
        public float damageCoefficient;
        public float hpCoefficient;
    }

    [Serializable]
    public class StageData : IData
    {
        int IData.Id => id;
        public int id;
        public List<int> stageEnemysId;
        public List<WaveData> stageDatas;
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
    public class EquipmentOptionData : IData
    {
        int IData.Id => id;
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
        public EquipmentType summonType;
        public int targetValue;
        public int progressValue;
    }
    [Serializable]
    public class AchievementRewardData
    {
        public AchievementRewardType type;
        public StatusType statusType;
        public int integerParam;
        public float floatParam;
    }


    [Serializable]
    public class Datas<T> : ILoader<int, T> where T : IData
    {
        public List<T> datas = new List<T>();

        public Dictionary<int, T> MakeDict()
        {
            Dictionary<int, T> dict = new Dictionary<int, T>();
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
        public List<LanguageData> datas = new List<LanguageData>();

        public Dictionary<string, LanguageData> MakeDict()
        {
            Dictionary<string, LanguageData> dict = new Dictionary<string, LanguageData>();
            foreach (LanguageData data in datas)
            {
                dict.Add(data.key, data);
            }
            return dict;
        }
    }
}
