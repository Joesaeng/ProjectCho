using Data;
using Define;
using MagicianSpellUpgrade;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellDataByPlayerOwnedSpell
{
    public int id;
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

    public SpellDataByPlayerOwnedSpell(BaseSpellData baseSpellData, int spellLevel)
    {
        id = baseSpellData.id;
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
    }
}

public class SpellDataBase
{
    public Dictionary<int, MagicianSpell> SpellDict { get; private set; } = new();
    public Dictionary<int, SpellDataByPlayerOwnedSpell> SpellDataDict { get; private set; } = new();

    public void Init()
    {
        foreach (var ownedSpellData in Managers.PlayerData.Data.ownedSpellDatas)
        {
            if (ownedSpellData.isEquip)
            {
                SpellDataByPlayerOwnedSpell data = new SpellDataByPlayerOwnedSpell(
                    Managers.Data.BaseSpellDataDict[ownedSpellData.spellId], ownedSpellData.spellLevel);
                SpellDataDict.Add(data.id, data);
            }
        }
    }

    public void BuildSpellDict()
    {
        var builder = new DataBuilder<int, SpellDataByPlayerOwnedSpell, MagicianSpell>(NewMagicianSpell);
        List<SpellDataByPlayerOwnedSpell> list = new List<SpellDataByPlayerOwnedSpell>();

        foreach (var ownedSpellData in Managers.PlayerData.Data.ownedSpellDatas)
        {
            if (ownedSpellData.isEquip)
            {
                SpellDataByPlayerOwnedSpell data = new SpellDataByPlayerOwnedSpell(
                    Managers.Data.BaseSpellDataDict[ownedSpellData.spellId], ownedSpellData.spellLevel);
                list.Add(data);
            }
        }

        foreach (var spellData in list)
        {
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
}