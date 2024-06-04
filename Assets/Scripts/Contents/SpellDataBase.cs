using Data;
using MagicianSpellUpgrade;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellDataBase
{
    public Dictionary<int, MagicianSpell> SpellDict { get; private set; } = new();

    public void Init()
    {
        var builder = new DataBuilder<int, BaseSpellData, MagicianSpell>(NewMagicianSpell);
        foreach (BaseSpellData spellData in Managers.Data.BaseSpellDataDict.Values)
        {
            builder.AddData(spellData.id, spellData);
        }
        SpellDict = builder.Build();
    }

    private MagicianSpell NewMagicianSpell(BaseSpellData data)
    {
        return data.spellType switch
        {
            SpellType.TargetedProjectile => new TargetedProjectile(data),
            SpellType.TargetedProjectileOfExplosion => new TargetedProjectileOfExplosion(data),
            SpellType.StraightProjectile => new StraightProjectile(data),
            SpellType.TargetedAOE => new TargetedAOE(data),
            // SpellType.Summon => new Summon(data), // 필요 시 추가
            _ => throw new System.ArgumentException($"Unknown SpellType: {data.spellType}")
        };
    }

    public void UpgradeSkill(int id, ISpellUpgrade upgrade)
    {
        SpellDict[id].AddUpgrade(upgrade);
    }
}