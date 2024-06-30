using Data;
using MagicianSpellUpgrade;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_SpellDescOnDefense : UI_Base
{
    Vector3 _hidedPos;
    Vector3 _showdPos;
    bool _isShowing;
    
    RectTransform _rectTf;
    Transform _upgradeTextsTf;
    List<TextMeshProUGUI> _upgradeTexts = new();

    enum Texts
    {
        Text_SpellName,
        Text_SpellUpgradeCount,
        Text_SpellDamageAndCdt,
    }

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        _rectTf = GetComponent<RectTransform>();
        _showdPos = new Vector3(0, -395f, 0);
        _hidedPos = new Vector3(0, -760f, 0);
        _isShowing = false;
        _rectTf.localPosition = _hidedPos;

        _upgradeTextsTf = Util.FindChild<Transform>(gameObject, "Content_UpgradeTexts",true);
        for (int i = 0; i < _upgradeTextsTf.childCount; ++i)
        {
            if (_upgradeTextsTf.GetChild(i).TryGetComponent<TextMeshProUGUI>(out var text))
            {
                _upgradeTexts.Add(text);
                text.gameObject.SetActive(false); // 초기에는 비활성화
            }
        }
        Util.FindChild(gameObject, "Button_Hide").AddUIEvent(HideSpellDesc);
    }

    public void SetSpellDesc(int spellId)
    {
        if (!_isShowing)
            ShowSpellDesc();

        // 업그레이드 텍스트 초기화
        foreach (var text in _upgradeTexts)
        {
            text.gameObject.SetActive(false); // 모든 텍스트 요소를 비활성화
        }

        

        MagicianSpell spell = Managers.Status.PlayerSpells.SpellDict[spellId];
        SpellDataByPlayerOwnedSpell spellData = Managers.Status.PlayerSpells.SpellDataDict[spellId];
        GetText((int)Texts.Text_SpellName).text = Language.GetLanguage(spellData.SpellName);
        GetText((int)Texts.Text_SpellName).color = ConstantData.TextColorsByElementTypes[(int)spellData.ElementType];
        List<ISpellUpgrade> upgrades = spell.Upgrades;

        float damage = spell.SpellDamage;
        float coolDownTime = spell.SpellDelay;

        float increaseDamageValue = 0f;
        float decreaseDelayValue = 0f;

        if (upgrades.Count > 0)
        {
            int upgradeIndex = 0;

            GetText((int)Texts.Text_SpellUpgradeCount).text = $"Lv{spellData.spellLevel}[<color=#00FF00>{upgrades.Count}</color>]";
            foreach(var upgrade in upgrades)
            {
                if (upgradeIndex >= _upgradeTexts.Count)
                    AddUpgradeTextObject();
                _upgradeTexts[upgradeIndex].text = GenerateUpgradeText(upgrade);

                if(upgrade.UpgradeType == SpellUpgradeType.IncreaseDamage)
                {
                    increaseDamageValue += upgrade.UpgradeValue;
                }
                if (upgrade.UpgradeType == SpellUpgradeType.DecreaseSpellDelay)
                {
                    decreaseDelayValue += upgrade.UpgradeValue;
                }

                _upgradeTexts[upgradeIndex].gameObject.SetActive(true);
                upgradeIndex++;
            }
            GetText((int)Texts.Text_SpellDamageAndCdt).text = GenerateATKandCDTText(damage, coolDownTime,increaseDamageValue,decreaseDelayValue);
        }
        else
            GetText((int)Texts.Text_SpellUpgradeCount).text = $"Lv{spellData.spellLevel}";

        GetText((int)Texts.Text_SpellDamageAndCdt).text = GenerateATKandCDTText(damage,coolDownTime,increaseDamageValue,decreaseDelayValue);
    }

    string GenerateATKandCDTText(float damage, float coolDownTime, float increaseDamageValue, float decreaseDelayValue)
    {
        string atkcdtText = "";

        atkcdtText += $"ATK : {Mathf.RoundToInt(damage)}";
        if (increaseDamageValue > 0)
            atkcdtText += $" <color=#00FF00>(+{Mathf.RoundToInt(increaseDamageValue * 100)}%)</color>";

        atkcdtText += $" CDT : {coolDownTime.ToString("0.0")}s";

        if (decreaseDelayValue > 0)
            atkcdtText += $" <color=#00FF00>(-{Mathf.RoundToInt(decreaseDelayValue * 100)}%)</color>";

        return atkcdtText;
    }

    string GenerateUpgradeText(ISpellUpgrade spellUpgrade)
    {
        switch (spellUpgrade.UpgradeType)
        {
            case SpellUpgradeType.IncreaseDamage:
            case SpellUpgradeType.IncreaseSize:
                return $"{Language.GetLanguage(spellUpgrade.UpgradeType.ToString())} <color=#00FF00>+{Mathf.RoundToInt(spellUpgrade.UpgradeValue * 100)}%</color>";
            case SpellUpgradeType.DecreaseSpellDelay:
                return $"{Language.GetLanguage(spellUpgrade.UpgradeType.ToString())} <color=#00FF00>-{Mathf.RoundToInt(spellUpgrade.UpgradeValue * 100)}%</color>";

            case SpellUpgradeType.IncreasePierce:
            case SpellUpgradeType.AddProjectile:
                return $"{Language.GetLanguage(spellUpgrade.UpgradeType.ToString())} <color=#00FF00>+{spellUpgrade.UpgradeValue}</color>";

            default:
                return "Invalid upgrade type";
        }
    }

    void AddUpgradeTextObject()
    {
        GameObject textObject = Instantiate(_upgradeTexts[0].gameObject, _upgradeTextsTf);
        if (textObject.TryGetComponent<TextMeshProUGUI>(out var text))
        {
            _upgradeTexts.Add(text);
            text.gameObject.SetActive(false); // 초기에는 비활성화
        }
    }

    void HideSpellDesc(PointerEventData data)
    {
        _isShowing = false;
        LeanTween.move(_rectTf, _hidedPos, 0.5f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true).setOnComplete(()
            => gameObject.SetActive(false));
        Managers.Sound.Play("ui_toggle");
    }

    void ShowSpellDesc()
    {
        _isShowing = true;
        LeanTween.move(_rectTf, _showdPos, 0.5f).setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);
    }
}
