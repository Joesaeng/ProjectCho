using Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_LevelUpOptions : UI_Base
{
    bool isBind = false;

    enum Texts
    {
        Text_SpellName,
        Text_UpgradeDesc,
        Text_UpgradeStatus,
        Text_NewSpell,
    }

    public override void Init()
    {

    }
    string statusFormat =
@"{0}
{1}
{2}
{3}
";

    public void Set(LevelUpOptions options)
    {
        if(isBind == false)
        {
            Bind<TextMeshProUGUI>(typeof(Texts));
            isBind = true;
        }

        if (options == null)
        {
            Debug.LogError("LevelUpOptions is null!");
            return;
        }
        if (options.IsNewSpell)
        {
            BaseSpellData data = options.BaseSpellData;
            GetText((int)Texts.Text_NewSpell).gameObject.SetActive(true);
            GetText((int)Texts.Text_NewSpell).text = Language.GetLanguage("NewSpell");
            GetText((int)Texts.Text_SpellName).text = Language.GetLanguage($"{data.spellName}");
            GetText((int)Texts.Text_SpellName).color = ConstantData.TextColorsByElementTypes[(int)data.elementType];

            GetText((int)Texts.Text_UpgradeDesc).text = Language.GetLanguage($"{data.spellName}_Desc");

            // string v = string.Format(statusFormat,
            //         string.Format(Language.GetLanguage("ElementType"), Language.GetLanguage($"{data.elementType}")),
            //         string.Format(Language.GetLanguage("DamageCoefficient"), data.spellDamage * 100),
            //         string.Format(Language.GetLanguage("AttackDelay"), data.spellDelay),
            //         string.Format(Language.GetLanguage("AttackRange"), data.spellRange));
            string v = string.Format(statusFormat,
                    $"{Language.GetLanguage("ElementType")}: {Language.GetLanguage($"{data.elementType}")}",
                    $"{Language.GetLanguage("DamageCoefficient")}: {data.spellDamageCoefficient * 100}%",
                    $"{Language.GetLanguage("AttackDelay")}: {data.spellDelay}s",
                    $"{Language.GetLanguage("AttackRange")}: {data.spellRange}m");
            GetText((int)Texts.Text_UpgradeStatus).text = v;
        }
        else
        {
            BaseSpellData baseData = options.BaseSpellData;
            SpellUpgradeData data = options.SpellUpgradeData;
            GetText((int)Texts.Text_NewSpell).gameObject.SetActive(false);
            GetText((int)Texts.Text_SpellName).text = Language.GetLanguage($"{baseData.spellName}");
            GetText((int)Texts.Text_SpellName).color = ConstantData.TextColorsByElementTypes[(int)baseData.elementType];

            GetText((int)Texts.Text_UpgradeDesc).text =
                GenerateTooltipText(Language.GetLanguage($"{data.spellUpgradeType}_Desc"), data);

            GetText((int)Texts.Text_UpgradeStatus).text = "";
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
