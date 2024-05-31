using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellDataBase
{
    public Dictionary<int, MagicianSpell> SpellDict { get; private set; } = new();

    public void Init()
    {
        MakeMagicianSpell();
    }

    private void MakeMagicianSpell()
    {
        foreach(BaseSpellData spellData in Managers.Data.BaseSpellDataDict.Values)
        {
            SpellDict.Add(spellData.id, NewMagicianSpell(spellData));
        }
    }

    private MagicianSpell NewMagicianSpell(BaseSpellData data)
    {
        return data.spellType switch
        {
            SpellType.TargetedProjectile
            => new TargettedProjecttile(data),
            SpellType.StraightProjectile
            => new StraightProjectile(data),
            SpellType.TargetedAOE
            => new TargettedAOE(data),
            // SpellType.Summon
            // => new StraightProjectile(data),

            _ => throw new System.ArgumentException($"Unknown SpellType: {data.spellType}")
        };
    }
}