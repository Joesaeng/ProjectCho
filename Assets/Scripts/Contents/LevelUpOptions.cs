using Data;
using MagicianSpellUpgrade;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpOptions
{
    public int SpellId { get; private set; }
    public bool IsNewSpell { get; private set; }
    public BaseSpellData BaseSpellData {  get; private set; }
    public SpellUpgradeData SpellUpgradeData { get; private set; }

    public LevelUpOptions(bool newSpell, BaseSpellData spellData = null, SpellUpgradeData upgradeData = null)
    {
        IsNewSpell = newSpell;
        if(IsNewSpell)
        {
            if(spellData == null)
            {
                Debug.Log("Is new spell level up option but spellData is null");
                return;
            }
            BaseSpellData = spellData;
            SpellId = spellData.id;
        }
        else
        {
            if(upgradeData == null)
            {
                Debug.Log("Is spell upgrade but spellData is null");
                return;
            }
            SpellUpgradeData = upgradeData;
            BaseSpellData = Managers.Data.BaseSpellDataDict[upgradeData.spellId];
            SpellId = upgradeData.spellId;
        }
    }
}
