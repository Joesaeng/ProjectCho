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
public enum SpellDamageType
{

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

    [Serializable]
    public class BaseEnemyData : IData
    {
        int IData.id => id;
        public int id;
        public float baseHp;
        public float baseMoveSpeed;
        public float baseAttackDamage;
        public float baseAttackDelay;
        public float baseAttackRange;
        public bool isRange;
    }

    [Serializable]
    public class BaseSpellData : IData
    {
        int IData.id => id;
        public int id;
        public SpellDamageType spellDamageType;
        public float damage;
        public float damageDuration;
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
