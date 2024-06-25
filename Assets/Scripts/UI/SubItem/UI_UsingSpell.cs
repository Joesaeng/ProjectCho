using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_UsingSpell : UI_Base
{
    Image _spellIcon;
    TextMeshProUGUI _spellLevelText;
    int _spellUpgradeCount;
    public override void Init()
    {
        _spellIcon      = Util.FindChild<Image>(gameObject, "Image_SpellIcon");
        _spellLevelText = Util.FindChild<TextMeshProUGUI>(gameObject, "Text_SpellLevel");
        _spellIcon.enabled      = false;
        _spellLevelText.enabled = false;
        _spellUpgradeCount = 0;
    }

    public void SetUsingSpell(int spellId)
    {
        _spellIcon.sprite = Managers.Spell.SpellSpriteDict[spellId];
        _spellIcon.enabled = true;
    }

    public void SpellTextLevelUp()
    {
        _spellUpgradeCount++;
        _spellLevelText.enabled = true;
        _spellLevelText.text = $"+ {_spellUpgradeCount}";
    }
}
