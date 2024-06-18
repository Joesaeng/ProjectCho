using Data;
using Define;
using MagicianSpellUpgrade;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public interface ISpellData
{
    int SpellId { get; }
    int EffectId { get; }
    SpellBehaviorType SpellBehaviorType { get; }
    MagicianAnim AnimType { get; }
    ElementType ElementType { get; }
    string SpellName { get; }
    int PierceCount { get; }
    float SpellSpeed { get; }
    float SpellRange { get; }
    float SpellSize { get; }

    float SpellDamageCoefficient { get; }
    float SpellDelay { get; }

    int IntegerParam1 { get; }
    int IntegerParam2 { get; }
    float FloatParam1 { get; }
    float FloatParam2 { get; }
}

public class SpellDataByPlayerOwnedSpell : ISpellData
{
    public int id;
    public int spellLevel;
    public int effectId;
    public SpellBehaviorType spellBehaviorType;
    public MagicianAnim animType;
    public ElementType elementType;
    public string spellName;
    public int pierceCount;
    public float spellSpeed;
    public float spellRange;
    public float spellSize;

    public float spellDamageCoefficient;
    public float spellDelay;

    public int integerParam1;
    public int integerParam2;
    public float floatParam1;
    public float floatParam2;

    public int ownedSpellCount;
    public int requireSpellCountToLevelup;

    public bool isEquip;

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
    public float SpellDamageCoefficient => spellDamageCoefficient;
    public float SpellDelay => spellDelay;
    public int IntegerParam1 => integerParam1;
    public int IntegerParam2 => integerParam2;
    public float FloatParam1 => floatParam1;
    public float FloatParam2 => floatParam2;
    #endregion

    public SpellDataByPlayerOwnedSpell(BaseSpellData baseSpellData, int spellLevel)
    {
        id = baseSpellData.id;
        this.spellLevel = spellLevel;
        effectId = baseSpellData.effectId;
        spellBehaviorType = baseSpellData.spellBehaviorType;
        animType = baseSpellData.animType;
        elementType = baseSpellData.elementType;
        spellName = baseSpellData.spellName;
        pierceCount = baseSpellData.pierceCount;
        spellSpeed = baseSpellData.spellSpeed;
        spellRange = baseSpellData.spellRange;
        spellSize = baseSpellData.spellSize;
        integerParam1 = baseSpellData.integerParam1;
        integerParam2 = baseSpellData.integerParam2;
        floatParam1 = baseSpellData.floatParam1;
        floatParam2 = baseSpellData.floatParam2;

        spellDamageCoefficient = baseSpellData.spellDataByLevel[spellLevel].spellDamageCoefficient;
        spellDelay = baseSpellData.spellDataByLevel[spellLevel].spellDelay;
        requireSpellCountToLevelup = baseSpellData.spellDataByLevel[spellLevel].requireSpellCountToLevelup;
    }

    public PlayerOwnedSpellData ToData()
    {
        PlayerOwnedSpellData toData = new()
        {
            spellId = id,
            spellLevel = spellLevel,
            ownCount = ownedSpellCount,
            isEquip = isEquip,
        };
        return toData;
    }
}

public class PlayerSpells
{
    public Dictionary<int, MagicianSpell> SpellDict { get; private set; } = new();
    public Dictionary<int, SpellDataByPlayerOwnedSpell> SpellDataDict { get; private set; } = new();

    public Action OnChangedSpellData;

    public void Init()
    {
        foreach (var ownedSpellData in Managers.PlayerData.Data.ownedSpellDatas)
        {
            SpellDataByPlayerOwnedSpell data = new(Managers.Data.BaseSpellDataDict[ownedSpellData.spellId], ownedSpellData.spellLevel)
            {
                ownedSpellCount = ownedSpellData.ownCount,
                isEquip = ownedSpellData.isEquip,
            };
            SpellDataDict[data.id] = data;
        }
    }

    public void BuildSpellDict()
    {
        var builder = new DataBuilder<int, SpellDataByPlayerOwnedSpell, MagicianSpell>(NewMagicianSpell);
        List<SpellDataByPlayerOwnedSpell> list = new();

        foreach (var spellData in SpellDataDict.Values)
        {
            if(spellData.isEquip)
                builder.AddData(spellData.id, spellData);
        }
        
        SpellDict = builder.Build();
    }

    public void ClearSpellDict()
    {
        SpellDict.Clear();
    }

    private MagicianSpell NewMagicianSpell(SpellDataByPlayerOwnedSpell data)
    {
        return data.spellBehaviorType switch
        {
            SpellBehaviorType.TargetedProjectile => new TargetedProjectile(data),
            SpellBehaviorType.TargetedProjectileOfExplosion => new TargetedProjectileOfExplosion(data),
            SpellBehaviorType.StraightProjectile => new StraightProjectile(data),
            SpellBehaviorType.TargetedAOE => new TargetedAOE(data),
            // SpellType.Summon => new Summon(data), // 필요 시 추가
            _ => throw new System.ArgumentException($"Unknown SpellType: {data.spellBehaviorType}")
        };
    }

    public void UpgradeSkill(int id, ISpellUpgrade upgrade)
    {
        SpellDict[id].AddUpgrade(upgrade);
    }

    public bool AvailableLevelUp(int spellId)
    {
        if (SpellDataDict[spellId].requireSpellCountToLevelup == 0) return false;
        return SpellDataDict[spellId].ownedSpellCount >= SpellDataDict[spellId].requireSpellCountToLevelup;
    }

    public void SpellLevelUp(int spellId)
    {
        if (!AvailableLevelUp(spellId))
            return;
        int remainCount = SpellDataDict[spellId].ownedSpellCount - SpellDataDict[spellId].requireSpellCountToLevelup;
        int upLevel = SpellDataDict[spellId].spellLevel + 1;
        ModifiySpellCount(spellId, -SpellDataDict[spellId].requireSpellCountToLevelup);
        NewOwnedSpellData(spellId, upLevel, remainCount);
    }

    public void AddSpell(int spellId,int count)
    {
        if (SpellDataDict.ContainsKey(spellId))
            ModifiySpellCount(spellId, count);
        else
            NewOwnedSpellData(spellId,0, count);
    }

    void NewOwnedSpellData(int spellId, int spellLevel, int spellcount)
    {
        bool isEquip = false;
        if (SpellDataDict.ContainsKey(spellId))
            isEquip = SpellDataDict[spellId].isEquip;
        SpellDataDict.Remove(spellId);
        SpellDataDict[spellId] = new SpellDataByPlayerOwnedSpell(
                    Managers.Data.BaseSpellDataDict[spellId], spellLevel)
        {
            ownedSpellCount = spellcount,
            isEquip = isEquip
        };
    }

    void ModifiySpellCount(int spellId, int count)
    {
        SpellDataDict[spellId].ownedSpellCount += count;
        OnChangedSpellData?.Invoke();
    }

    public List<PlayerOwnedSpellData> SpellDataDictToData()
    {
        return SpellDataDict.Select(spellData => spellData.Value.ToData()).ToList();
    }
}