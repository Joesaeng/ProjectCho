using Define;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Diagnostics;

public interface IData
{
    public int id { get; }
}

[Serializable]
public enum SpellType
{
    TargetedProjectile,
    StraightProjectile,
    TargetedAOE,
    Summon,
}

[Serializable]
public enum ElementType
{
                // 동일 속성 적(Light,Dark 제외)에게는 75%의 데미지를 가함.
    Energy,     // 모든 속성의 적(어둠,빛 제외) 100% 데미지
    Fire,       // Air 속성의 적에게 200% 데미지, Water 속성 적에게 50% 데미지
    Water,      // Fire 속성 적에게 200% 데미지, Lightning 속성 적에게 50% 데미지
    Lightning,  // Water 속성 적에게 200% 데미지, Earth 속성 적에게 50% 데미지
    Earth,      // Lightning 속성 적에게 200% 데미지, Air 속성 적에게 50% 데미지
    Air,        // Earth 속성 적에게 200% 데미지, Fire 속성 적에게 50% 데미지
    Light,      // Dark속성 적에게 300% 데미지, Light 속성 적에게 0% 데미지
    Dark,       // Light속성 적에게 300% 데미지, Dark 속성 적에게 0% 데미지
}

namespace Data
{
    [Serializable]
    public class PlayerData 
    {
        public bool beginner = true;
        public int gameLanguage = (int)Define.GameLanguage.English;
        public float bgmVolume = 1f;
        public float sfxVolume = 1f;
        public bool bgmOn = true;
        public bool sfxOn = true;
    }

    public class PlayerWallData : IData
    {
        int IData.id => id;
        public int id;
        public float maxHp;
    }

    [Serializable]
    public class BaseEnemyData : IData
    {
        int IData.id => id;
        public int id;
        public ElementType elementType;
        public float baseHp;
        public float baseMoveSpeed;
        public float baseAttackDamage;
        public float baseAttackDelay;
        public float baseAttackRange;
        public bool isRange;
        public int projectileId;
    }

    [Serializable]
    public class ProjectileData : IData
    {
        int IData.id => id;
        public int id;
        public float baseMoveSpeed;
        public string projectileName;
        public string explosionName;
    }

    [Serializable]
    public class AOEEffectData : IData
    {
        int IData.id => id;
        public int id;
        public string effectName;
    }

    [Serializable]
    public class BaseSpellData : IData
    {
        int IData.id => id;
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
                dict.Add(data.id, data);
            }
            return dict;
        }
    }
}
