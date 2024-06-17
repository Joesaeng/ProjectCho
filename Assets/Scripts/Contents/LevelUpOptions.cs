using Data;
using MagicianSpellUpgrade;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpOptions
{
    public int SpellId { get; private set; }
    public bool IsNewSpell { get; private set; }
    public SpellDataByPlayerOwnedSpell SpellData {  get; private set; }
    public SpellUpgradeData SpellUpgradeData { get; private set; }

    public LevelUpOptions(bool newSpell, SpellDataByPlayerOwnedSpell spellData = null, SpellUpgradeData upgradeData = null)
    {
        IsNewSpell = newSpell;
        if(IsNewSpell)
        {
            if(spellData.Equals(null))
            {
                Debug.Log("Is new spell level up option but spellData is null");
                return;
            }
            SpellData = spellData;
            SpellId = spellData.id;
        }
        else
        {
            if(upgradeData.Equals(null))
            {
                Debug.Log("Is spell upgrade but spellData is null");
                return;
            }
            SpellUpgradeData = upgradeData;
            SpellId = upgradeData.spellId;
        }
    }
}
