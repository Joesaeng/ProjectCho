using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 스펠 소환 시 랜덤한 스펠을 생성해주고 스펠의 아이콘을 저장

public class SpellRarityWeight : IRandomWeighted
{
    public SpellRarity rarity;
    public int weight;
    public int Weight => weight;
}
public class SpellManager
{
    private Dictionary<int,Sprite> _spellSpriteDict;
    public Dictionary<int, Sprite> SpellSpriteDict { get => _spellSpriteDict; set => _spellSpriteDict = value; }

    private Dictionary<SpellRarity,List<int>> _spellDictByRarity;

    private List<SpellRarityWeight> rarityWeights = new()
    {
        new SpellRarityWeight { rarity = SpellRarity.Normal, weight = 650 },
        new SpellRarityWeight { rarity = SpellRarity.Rare, weight = 250 },
        new SpellRarityWeight { rarity = SpellRarity.Epic, weight = 68 },
        new SpellRarityWeight { rarity = SpellRarity.Legend, weight = 32 }
    };

    public void Init()
    {
        SetSpriteDict();
        SetSpellDictByRarity();
    }

    void SetSpellDictByRarity()
    {
        List<BaseSpellData> spellDatas = Managers.Data.BaseSpellDataDict.Values.ToList();
        _spellDictByRarity = new();
        for(int i = 0; i < spellDatas.Count; ++i)
        {
            if (!_spellDictByRarity.ContainsKey(spellDatas[i].spellRarity))
                _spellDictByRarity[spellDatas[i].spellRarity] = new();
            _spellDictByRarity[spellDatas[i].spellRarity].Add(spellDatas[i].id);
        }
    }

    void SetSpriteDict()
    {
        _spellSpriteDict = new();
        var list = Managers.Data.BaseSpellDataDict.Values.ToList();
        for(int i = 0; i < list.Count; i++)
        {
            _spellSpriteDict[list[i].id] = Managers.Resource.Load<Sprite>($"UI/SpellIcons/{list[i].spellName}");
        }
    }

    public Dictionary<int,int> SummonSpells(int count = 100)
    {
        // Player의 Coin, Dia 보유량 확인
        Dictionary<int,int> retSpellDict = new();
        for(int i = 0; i < count; ++i)
        {
            int randomSpellId = SummonRandomSpell();
            if (retSpellDict.ContainsKey(randomSpellId))
                retSpellDict[randomSpellId]++;
            else
                retSpellDict[randomSpellId] = 1;
        }

        foreach(var spellKvp in retSpellDict)
        {
            Managers.Player.AddSpells(spellKvp.Key,spellKvp.Value);
        }

        return retSpellDict;
    }

    int SummonRandomSpell()
    {
        // SpellRarity rarity = Util.GetRandomWeightedSelect(rarityWeights).rarity;
        SpellRarity rarity = SpellRarity.Normal;

        List<int> spellsByRarity = _spellDictByRarity[rarity].ToList();
        int randomInt = Random.Range(0,spellsByRarity.Count);

        return spellsByRarity[randomInt];
    }
}
