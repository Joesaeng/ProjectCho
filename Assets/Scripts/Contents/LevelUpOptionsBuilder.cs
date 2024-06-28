using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LevelUpOptionsBuilder
{
    public static List<LevelUpOptions> CreateLevelUpOptions(List<ISpellUseable> spells)
    {
        var ownSpells = spells.Select(m => m.Spell.id).ToHashSet();
        var levelupOptions = new List<LevelUpOptions>();

        if(ownSpells.Count < 5)
        {
            levelupOptions.AddRange(
            Managers.Status.PlayerSpells.SpellDataDict.Values
                .Where(data => !ownSpells.Contains(data.id))
                .Select(data => new LevelUpOptions(true, data)));
        }
        
        foreach(var data in DefenseSceneManager.Instance.SpellUpgradeDatas)
        {
            if (ownSpells.Contains(data.spellId))
                levelupOptions.Add(new LevelUpOptions(false, upgradeData: data));
        }

        return GetRandomOptions(levelupOptions, 3);
    }

    private static List<LevelUpOptions> GetRandomOptions(List<LevelUpOptions> options, int count)
    {
        return options.OrderBy(_ => UnityEngine.Random.value).Take(count).ToList();
    }
}
