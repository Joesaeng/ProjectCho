using Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_LevelUpOptions : UI_Base
{
    Color[] _colorsByElementTypes = new Color[]
    {
        new Color(1f,0.28f,0.72f),  // Energy
        new Color(1f,0,0),          // Fire
        new Color(0f,0.3f,1f),      // Water
        new Color(0.4f,0,1f),       // Lightning
        new Color(0,0.2f,0),        // Earth
        new Color(0.25f,1,0.55f),   // Air
        new Color(1f,0.76f,0.33f),  // Light
        new Color(0,0,0.22f),       // Dark
    };
    enum Texts
    {
        Text_SpellName,
        Text_UpgradeDesc,
        Text_UpgradeStatus,
        Text_NewSpell
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
        Bind<TextMeshProUGUI>(typeof(Texts));
        if (options == null)
        {
            Debug.LogError("LevelUpOptions is null!");
            return;
        }
        if(options.IsNewSpell)
        {
            BaseSpellData data = options.BaseSpellData;
            GetText((int)Texts.Text_NewSpell).gameObject.SetActive(true);
            GetText((int)Texts.Text_NewSpell).text = Language.GetLanguage("NewSpell");
            GetText((int)Texts.Text_SpellName).text = Language.GetLanguage($"{data.spellName}");
            GetText((int)Texts.Text_SpellName).color = _colorsByElementTypes[(int)data.elementType];

            GetText((int)Texts.Text_UpgradeDesc).text = Language.GetLanguage($"{data.spellName}_Desc");

            string v = string.Format(statusFormat,
                    string.Format(Language.GetLanguage("ElementType"), Language.GetLanguage($"{data.elementType}")),
                    string.Format(Language.GetLanguage("DamageCoefficient"), data.spellDamage),
                    string.Format(Language.GetLanguage("AttackDelay"), data.spellDelay),
                    string.Format(Language.GetLanguage("AttackRange"), data.spellRange));
            GetText((int)Texts.Text_UpgradeStatus).text = v;
        }
        else
        {
            BaseSpellData baseData = options.BaseSpellData;
            SpellUpgradeData data = options.SpellUpgradeData;
            GetText((int)Texts.Text_NewSpell).gameObject.SetActive(false);
            GetText((int)Texts.Text_SpellName).text = Language.GetLanguage($"{baseData.spellName}");
            GetText((int)Texts.Text_SpellName).color = _colorsByElementTypes[(int)baseData.elementType];

            GetText((int)Texts.Text_UpgradeDesc).text =
                GenerateTooltipText(Language.GetLanguage($"{data.spellUpgradeType}"), data);

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

            case SpellUpgradeType.AddExplosionOnImpact:
                return string.Format(template, upgradeData.floatValue * 100);

            case SpellUpgradeType.IncreasePierce:
            case SpellUpgradeType.AddProjectile:
                return string.Format(template, upgradeData.integerValue);

            default:
                return "Invalid upgrade type";
        }
    }
}
