using Data;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

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
        Text_AvailableUpgrades,
        Text_LevelUp
    }
    enum Buttons
    {
        Button_LevelUp
    }

    Dictionary<int, UI_SpellIcon> _spellIconDict = new();

    Dictionary<Texts, TextMeshProUGUI> _textDict = new();

    Sprite[] _spellEdgeSprites = new Sprite[2];

    GameObject _spellDesc;

    Transform _iconsTf;
    int _selectedSpellId;

    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        _spellDesc = Util.FindChild(gameObject, "Image_SpellDesc");
        _spellDesc.SetActive(false);

        foreach (Texts text in Enum.GetValues(typeof(Texts)))
        {
            _textDict.Add(text, GetText((int)text));
        }

        _spellEdgeSprites[0] = Resources.Load<Sprite>("UI/Useds/UnSelected");
        _spellEdgeSprites[1] = Resources.Load<Sprite>("UI/Useds/Selected");

        _iconsTf = GetObject((int)Objects.Panel_SpellIcons).transform;
        GetButton((int)Buttons.Button_LevelUp).gameObject.AddUIEvent(ClickedSpellLevelUp);

        Managers.Status.PlayerSpells.OnChangedSpellData -= SetSpellIcons;
        Managers.Status.PlayerSpells.OnChangedSpellData += SetSpellIcons;

        GetText((int)Texts.Text_LevelUp).text = Language.GetLanguage("LevelUp");

        SetSpellIcons();
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
            if (!_spellIconDict.ContainsKey(spellId))
            {
                _spellIconDict.Add(spellId, _iconsTf.GetChild(spellId).GetComponent<UI_SpellIcon>());
                _spellIconDict[spellId].Init();
                _spellIconDict[spellId].SetId(spellId);
                _spellIconDict[spellId].SetImages(spellId, _spellEdgeSprites[0]);
                _spellIconDict[spellId].OnClickedSkillIcon += ClickedSpellIcon;
            }
            else
                _spellIconDict[spellId].SetOwnedCount(spellId);
        }

        foreach (KeyValuePair<int, UI_SpellIcon> kvp in _spellIconDict)
        {
            if (Managers.Status.PlayerSpells.SpellDataDict.TryGetValue(kvp.Key, out SpellDataByPlayerOwnedSpell spellData))
            {
                kvp.Value.SetUnlock();
                kvp.Value.SetOwnedCount(spellData.id);
            }
            else
            {
                kvp.Value.SetLock();
                kvp.Value.SetOwnedCount();
            }
        }
    }

    void ClickedSpellIcon(int spellId)
    {
        foreach (UI_SpellIcon icon in _spellIconDict.Values)
        {
            icon.SetEdge(_spellEdgeSprites[0]);
        }

        SetSpellDesc(spellId, _spellIconDict[spellId]._isLock);
    }

    void ClickedSpellLevelUp(PointerEventData data)
    {
        if (Managers.Status.PlayerSpells.SpellLevelUp(_selectedSpellId))
        {
            var spellData = Managers.Status.PlayerSpells.SpellDataDict[_selectedSpellId];
            _spellIconDict[_selectedSpellId].SetOwnedCount(spellData.id);
            SetSpellDesc(_selectedSpellId, false);
            LobbySceneManager.Instance.SaveDataOnLobbyScene();
        }
    }

    void SetSpellDesc(int spellId, bool isLock)
    {
        _selectedSpellId = spellId;
        _spellDesc.SetActive(true);
        ISpellData spellData;
        if (isLock)
        {
            spellData = Managers.Data.BaseSpellDataDict[spellId];
            GetButton((int)Buttons.Button_LevelUp).gameObject.SetActive(false);
        }
        else
        {
            spellData = Managers.Status.PlayerSpells.SpellDataDict[spellId];
            GetButton((int)Buttons.Button_LevelUp).gameObject.SetActive(Managers.Status.PlayerSpells.AvailableLevelUp(spellId));
        }

        _spellIconDict[spellId].SetEdge(_spellEdgeSprites[1]);
        _textDict[Texts.Text_Name].text = Language.GetLanguage(spellData.SpellName);
        _textDict[Texts.Text_Name].color = ConstantData.TextColorsByElementTypes[(int)spellData.ElementType];

        _textDict[Texts.Text_Attributes].text =
        $"{Language.GetLanguage("ElementType")}\n" +
        $"{Language.GetLanguage("DamageCoefficient")}\n" +
        $"{Language.GetLanguage("AttackDelay")}\n" +
        $"{Language.GetLanguage("AttackRange")}";

        _textDict[Texts.Text_AttributesValue].text =
        $"{Language.GetLanguage($"{spellData.ElementType}")}\n" +
        $"{spellData.SpellDamageCoefficient * 100}%\n" +
        $"{spellData.SpellDelay}s\n" +
        $"{spellData.SpellRange}m";

        SetTextForSpecialOptionSpell(spellData);

        _textDict[Texts.Text_Mechanics].text = Language.GetLanguage($"{spellData.SpellName}_Desc");
        _textDict[Texts.Text_AvailableUpgradesTitle].text = Language.GetLanguage("AvailableUpgrades");

        HashSet<SpellUpgradeType> upgrades = new();
        foreach (SpellUpgradeData upgradeData in Managers.Data.UpgradeDataDict[spellId].spellUpgradeDatas)
        {
            upgrades.Add(upgradeData.spellUpgradeType);
        }
        _textDict[Texts.Text_AvailableUpgrades].text = "";
        foreach (SpellUpgradeType upgradeType in upgrades)
        {
            _textDict[Texts.Text_AvailableUpgrades].text += Language.GetLanguage($"{upgradeType}") + ", ";
        }
    }

    void SetTextForSpecialOptionSpell(ISpellData spellData)
    {
        switch (spellData.SpellId)
        {
            case 2: // ExplosionRange
                _textDict[Texts.Text_Attributes].text +=
                    $"\n{Language.GetLanguage("ExplosionRange")}";
                _textDict[Texts.Text_AttributesValue].text +=
                    $"\n{spellData.FloatParam2}m";

                break;
            case 5: // SpellDuration
                _textDict[Texts.Text_Attributes].text +=
                    $"\n{Language.GetLanguage("SpellDuration")}";
                _textDict[Texts.Text_AttributesValue].text +=
                    $"\n{spellData.FloatParam1}s";
                break;
        }
    }
}
