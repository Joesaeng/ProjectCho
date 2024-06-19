using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_DefenseScene : UI_Scene
{
    TextMeshProUGUI _playerHpText;
    Transform _usingSpellsTf;

    const int UsingSpellCount = 5;

    UI_UsingSpell[] _usingSpells = new UI_UsingSpell[UsingSpellCount];
    Dictionary<int, UI_UsingSpell> _usingSpellDict = new();

    Image _expGuage;
    enum Texts
    {
        Text_PlayerHp,
    }

    public override void Init()
    {
        base.Init();
        // UI 바인드
        Bind<TextMeshProUGUI>(typeof(Texts));

        // 캐싱
        _playerHpText = GetText((int)Texts.Text_PlayerHp);
        _usingSpellsTf = Util.FindChild<Transform>(gameObject, "Image_Bottom");
        _expGuage = Util.FindChild<Image>(gameObject, "Image_ExpGauge", true);
        for (int i = 0; i < UsingSpellCount; i++)
        {
            _usingSpells[i] = _usingSpellsTf.GetChild(i).GetComponent<UI_UsingSpell>();
            _usingSpells[i].gameObject.SetActive(false);
        }

        #region 이벤트 바인드
        DefenseSceneManager.Instance.OnUpdatePlayerHp -= UpdatePlayerHpListner;
        DefenseSceneManager.Instance.OnUpdatePlayerHp += UpdatePlayerHpListner;
        #endregion
    }

    public void SetUsingSpell(int spellId, int spellNum)
    {
        _usingSpells[spellNum].Init();
        _usingSpells[spellNum].SetUsingSpell(spellId);
        _usingSpellDict[spellId] = _usingSpells[spellNum];
    }

    public void LevelUpUsingSpell(int spellId)
    {
        _usingSpellDict[spellId].SpellTextLevelUp();
    }

    public void SetExpGauge(float fill)
    {
        _expGuage.fillAmount = fill;
    }

    private void UpdatePlayerHpListner(int curHp,int maxHp)
    {
        if(_playerHpText.gameObject.activeSelf == false)
        {
            _playerHpText.gameObject.SetActive(true);
        }
        _playerHpText.text = $"{curHp} / {maxHp}";
    }
}
