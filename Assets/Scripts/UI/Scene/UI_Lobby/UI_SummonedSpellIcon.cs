using Coffee.UIExtensions;
using Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SummonedSpellIcon : UI_Base
{
    Image _spellIcon;
    TextMeshProUGUI _spellCountText;
    UIParticle _particle;
    string _effectPathFormat = "Effects/ChargeUseSpell/{0}Enchant";
    public override void Init()
    {
        _spellIcon = GetComponent<Image>();
        _spellCountText = GetComponentInChildren<TextMeshProUGUI>();
        _particle = GetComponentInChildren<UIParticle>();
    }

    public void SetSummonSpellIcon(int spellId, int count)
    {
        for(int i = 0; i < _particle.transform.childCount; i++)
        {
            Managers.Resource.Destroy(_particle.transform.GetChild(i).gameObject);
        }
        BaseSpellData spellData = Managers.Data.BaseSpellDataDict[spellId];
        _spellIcon.sprite = SpellManager.Instance.SpellSpriteDict[spellId];
        _spellCountText.text = $"x {count}";
        GameObject effect = Managers.Resource.Instantiate(string.Format(_effectPathFormat,spellData.ElementType), _particle.transform);
        effect.transform.SetLocalPositionAndRotation(Vector3.zero,Quaternion.identity);
        effect.transform.localScale = Vector3.one;

        _particle.RefreshParticles();
    }
}
