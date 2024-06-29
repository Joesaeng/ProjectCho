using Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_LevelUpOptions : UI_Base
{
    enum Texts
    {
        Text_SpellName,
        Text_UpgradeDesc,
        Text_UpgradeStatus,
        Text_NewSpell,
    }

    TextMeshProUGUI _spellName;
    TextMeshProUGUI _upgradeDesc;
    TextMeshProUGUI _upgradeStatus;
    TextMeshProUGUI _newSpell;

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        _spellName = GetText((int)Texts.Text_SpellName);
        _upgradeDesc = GetText((int)Texts.Text_UpgradeDesc);
        _upgradeStatus = GetText((int)Texts.Text_UpgradeStatus);
        _newSpell = GetText((int)Texts.Text_NewSpell);
    }
    string statusFormat =
@"{0}
{1}
{2}
{3}
";

    public void Set(LevelUpOptions options)
    {
        if (options == null)
        {
            Debug.LogError("LevelUpOptions is null!");
            return;
        }
        if (options.IsNewSpell)
        {
            SpellDataByPlayerOwnedSpell data = options.SpellData;
            _newSpell.gameObject.SetActive(true);
            _newSpell.text = Language.GetLanguage("NewSpell");
            _spellName.text = Language.GetLanguage($"{data.spellName}");
            _spellName.color = ConstantData.TextColorsByElementTypes[(int)data.elementType];

            _upgradeDesc.text = Language.GetLanguage($"{data.spellName}_Desc");

            string v = string.Format(statusFormat,
                    $"{Language.GetLanguage("ElementType")}: {Language.GetLanguage($"{data.elementType}")}",
                    $"{Language.GetLanguage("DamageCoefficient")}: {data.spellDamageCoefficient * 100}%",
                    $"{Language.GetLanguage("AttackDelay")}: {data.spellDelay}s",
                    $"{Language.GetLanguage("AttackRange")}: {data.spellRange}m");
            _upgradeStatus.text = v;
        }
        else
        {
            SpellDataByPlayerOwnedSpell data = Managers.Status.PlayerSpells.SpellDataDict[options.SpellId];
            SpellUpgradeData upgradeData = options.SpellUpgradeData;
            _newSpell.gameObject.SetActive(false);
            _spellName.text = Language.GetLanguage($"{data.spellName}");
            _spellName.color = ConstantData.TextColorsByElementTypes[(int)data.elementType];

            _upgradeDesc.text =
                GenerateTooltipText(Language.GetLanguage($"{upgradeData.spellUpgradeType}_Desc"), upgradeData);

            _upgradeStatus.text = "";
        }
    }

    private string GenerateTooltipText(string template, SpellUpgradeData upgradeData)
    {
        switch (upgradeData.spellUpgradeType)
        {
            case SpellUpgradeType.IncreaseDamage:
            case SpellUpgradeType.IncreaseSize:
            case SpellUpgradeType.DecreaseSpellDelay:
                return string.Format(template, upgradeData.floatValue * 100);

            case SpellUpgradeType.IncreasePierce:
            case SpellUpgradeType.AddProjectile:
                return string.Format(template, upgradeData.integerValue);

            default:
                return "Invalid upgrade type";
        }
    }
}
