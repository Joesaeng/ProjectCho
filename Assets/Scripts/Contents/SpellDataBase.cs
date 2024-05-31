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

        SpellDict[0].AddUpgrade(new ChainProjectileUpgrade(4,0.5f));
        SpellDict[0].AddUpgrade(new IncreasePierceUpgrade(2));
        SpellDict[1].AddUpgrade(new IncreaseSizeUpgrade(1.5f));
        SpellDict[2].AddUpgrade(new AddExplosionOnImpactUpgrade(20, 2f, SpellDict[2].ElementType,
            Managers.Data.AOEEffectDataDict[1]));
        SpellDict[2].AddUpgrade(new IncreasePierceUpgrade(2));
        SpellDict[3].AddUpgrade(new AddProjectileUpgrade(1));
        SpellDict[4].AddUpgrade(new AddProjectileUpgrade(1));
        SpellDict[4].AddUpgrade(new DecreaseDelayUpgrade(0.5f));
        SpellDict[4].AddUpgrade(new IncreaseSizeUpgrade(2f));
    }

    private MagicianSpell NewMagicianSpell(BaseSpellData data)
    {
        return data.spellType switch
        {
            SpellType.TargetedProjectile => new TargetedProjecttile(data),
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