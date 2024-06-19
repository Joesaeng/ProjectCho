using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_UsingSpell : UI_Base
{
    Image _spellIcon;
    TextMeshProUGUI _spellLevelText;
    int _spellLevel;
    public override void Init()
    {
        _spellIcon = Util.FindChild<Image>(gameObject, "Image_SpellIcon");
        _spellLevelText = Util.FindChild<TextMeshProUGUI>(gameObject, "Text_SpellLevel");
        _spellLevel = 1;
    }

    public void SetUsingSpell(int spellId)
    {
        gameObject.SetActive(true);
        _spellIcon.sprite = Managers.Spell.SpellSpriteDict[spellId];
        _spellLevelText.text = $"Lv {_spellLevel}";
    }

    public void SpellTextLevelUp()
    {
        _spellLevel++;
        _spellLevelText.text = $"Lv {_spellLevel}";
    }
}
