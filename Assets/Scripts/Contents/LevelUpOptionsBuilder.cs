using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LevelUpOptionsBuilder
{
    public static List<LevelUpOptions> CreateLevelUpOptions(List<Magician> magicians)
    {
        var ownSpells = magicians.Select(m => m.Spell.id).ToHashSet();
        var tempOptions = new List<LevelUpOptions>();

        if(ownSpells.Count < 5)
        {
            tempOptions.AddRange(
            Managers.Data.BaseSpellDataDict.Values
                .Where(data => !ownSpells.Contains(data.id))
                .Select(data => new LevelUpOptions(true, data)));
        }
        
        foreach(var data in DefenseSceneManager.Instance._SpellUpgradeDatas)
        {
            if (ownSpells.Contains(data.spellId))
                tempOptions.Add(new LevelUpOptions(false, upgradeData: data));
        }

        return GetRandomOptions(tempOptions, 3);
    }

    private static List<LevelUpOptions> GetRandomOptions(List<LevelUpOptions> options, int count)
    {
        return options.OrderBy(_ => UnityEngine.Random.value).Take(count).ToList();
    }
}
