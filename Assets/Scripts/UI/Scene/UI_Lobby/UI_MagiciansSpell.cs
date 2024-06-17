using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class UI_MagiciansSpell : UI_Base
{
    enum Objects
    {
        Panel_SpellIcons
    }
    enum Texts
    {
        Text_Name,
        Text_Attributes,
        Text_AttributesValue,
        Text_Mechanics,
        Text_AvailableUpgradesTitle,
        Text_AvailableUpgrades
    }

    Dictionary<int,UI_SpellIcon> _spellIconDict = new();

    Dictionary<int,Sprite> _spellIconSpriteDict = new();
    Dictionary<Texts,TextMeshProUGUI> _textDict = new();

    Sprite[] _spellEdgeSprites = new Sprite[2];

    Transform _iconsTf;
    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));
        Bind<TextMeshProUGUI>(typeof(Texts));

        foreach (Texts text in Enum.GetValues(typeof(Texts)))
        {
            _textDict.Add(text, GetText((int)text));
        }

        foreach (BaseSpellData data in Managers.Data.BaseSpellDataDict.Values)
        {
            _spellIconSpriteDict.Add(data.id, Managers.Resource.Load<Sprite>($"UI/SpellIcons/{data.elementType}"));
        }

        Sprite[] sprites = Resources.LoadAll<Sprite>("UI/UI_atlas");
        foreach (Sprite sprite in sprites)
        {
            if (sprite.name == "UnSelected")
                _spellEdgeSprites[0] = sprite;
            if (sprite.name == "Selected")
                _spellEdgeSprites[1] = sprite;
        }

        _iconsTf = GetObject((int)Objects.Panel_SpellIcons).transform;

        SetSpellIcons();
        SetSpellDesc(0);
    }

    void SetSpellIcons()
    {
        while (_iconsTf.childCount < Managers.Data.BaseSpellDataDict.Count)
        {
            GameObject newObj = Managers.UI.MakeSubItem<UI_SpellIcon>(_iconsTf).gameObject;
            newObj.transform.localScale = Vector3.one;
            newObj.transform.localPosition = Vector3.zero;
        }

        for (int spellId = 0; spellId < Managers.Data.BaseSpellDataDict.Count; ++spellId)
        {
            _spellIconDict.Add(spellId, _iconsTf.GetChild(spellId).GetComponent<UI_SpellIcon>());
            _spellIconDict[spellId].Init();
            _spellIconDict[spellId].SetImages(_spellIconSpriteDict[spellId], _spellEdgeSprites[0]);
            _spellIconDict[spellId].gameObject.AddUIEvent(ClickedSpellIcon, spellId);
        }

        foreach(KeyValuePair<int,UI_SpellIcon> kvp in _spellIconDict)
        {
            if (Managers.Player.SpellDataBase.SpellDataDict.ContainsKey(kvp.Key))
                kvp.Value.SetUnlock();
            else
                kvp.Value.SetLock();
        }
    }

    void ClickedSpellIcon(int spellId, PointerEventData data)
    {
        foreach (UI_SpellIcon icon in _spellIconDict.Values)
        {
            icon.SetEdge(_spellEdgeSprites[0]);
        }
        
        SetSpellDesc(spellId);
    }

    void SetSpellDesc(int spellId)
    {
        _spellIconDict[spellId].SetEdge(_spellEdgeSprites[1]);
        SpellDataByPlayerOwnedSpell spellData = Managers.Player.SpellDataBase.SpellDataDict[spellId];
        _textDict[Texts.Text_Name].text = spellData.spellName;
        _textDict[Texts.Text_Name].color = ConstantData.TextColorsByElementTypes[(int)spellData.elementType];

        _textDict[Texts.Text_Attributes].text =
        $"{Language.GetLanguage("ElementType")}\n" +
        $"{Language.GetLanguage("DamageCoefficient")}\n" +
        $"{Language.GetLanguage("AttackDelay")}\n" +
        $"{Language.GetLanguage("AttackRange")}";

        _textDict[Texts.Text_AttributesValue].text =
        $"{Language.GetLanguage($"{spellData.elementType}")}\n" +
        $"{spellData.spellDamageCoefficient * 100}%\n" +
        $"{spellData.spellDelay}s\n" +
        $"{spellData.spellRange}m";

        SetTextForSpecialOptionSpell(spellData);

        _textDict[Texts.Text_Mechanics].text = Language.GetLanguage($"{spellData.spellName}_Desc");
        _textDict[Texts.Text_AvailableUpgradesTitle].text = Language.GetLanguage("AvailableUpgrades");

        HashSet<SpellUpgradeType> upgrades = new();
        foreach (SpellUpgradeData upgradeData in Managers.Data.UpgradeDataDict[spellId].spellUpgradeDatas)
        {
            upgrades.Add(upgradeData.spellUpgradeType);
        }
        _textDict[Texts.Text_AvailableUpgrades].text = "";
        foreach (SpellUpgradeType upgradeType in upgrades)
        {
            _textDict[Texts.Text_AvailableUpgrades].text += Language.GetLanguage($"{upgradeType}")+", ";
        }
    }

    void SetTextForSpecialOptionSpell(SpellDataByPlayerOwnedSpell spellData)
    {
        switch (spellData.id)
        {
            case 2: // ExplosionRange
                _textDict[Texts.Text_Attributes].text +=
                    $"\n{Language.GetLanguage("ExplosionRange")}";
                _textDict[Texts.Text_AttributesValue].text +=
                    $"\n{spellData.floatParam2}";
                    
                break;
            case 5: // SpellDuration
                _textDict[Texts.Text_Attributes].text +=
                    $"\n{Language.GetLanguage("SpellDuration")}";
                _textDict[Texts.Text_AttributesValue].text +=
                    $"\n{spellData.floatParam1}";
                break;
        }
    }
}
