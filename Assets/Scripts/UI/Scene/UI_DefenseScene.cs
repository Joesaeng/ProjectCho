using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_DefenseScene : UI_Scene
{
    enum Texts
    {
        Text_PlayerHp,
        Text_LastWave,
        Text_WaveInfo,
        Text_KillCount,
        Text_KillCountValue,
    }

    enum Buttons
    {
        Button_Pause,
        Button_Fast,
    }

    TextMeshProUGUI _playerHpText;
    TextMeshProUGUI _waveInfo;
    TextMeshProUGUI _killCountValue;
    string _waveInfoFormat;

    UI_UsingSpell[] _usingSpells = new UI_UsingSpell[ConstantData.UsingSpellCount];
    Dictionary<int, UI_UsingSpell> _usingSpellDict = new();

    UI_SpellDescOnDefense _spellDesc;

    Image _expGuage;

    [SerializeField] Sprite[] _speedSprite;

    public Action OnClickedPause;
    public Action OnClickedFast;

    public override void Init()
    {
        // base.Init();
        // UI 바인드
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        GetText((int)Texts.Text_LastWave).gameObject.SetActive(false);

        _spellDesc = Util.FindChild<UI_SpellDescOnDefense>(gameObject);
        _spellDesc.Init();

        GetButton((int)Buttons.Button_Pause).gameObject.AddUIEvent(ClickedPause);
        GetButton((int)Buttons.Button_Fast).gameObject.AddUIEvent(ClickedFast);
        GetButton((int)Buttons.Button_Fast).GetComponent<Image>().sprite =
            _speedSprite[Managers.Time.CurTimeScale - 1];

        // 캐싱
        _playerHpText = GetText((int)Texts.Text_PlayerHp);
        Transform usingSpellsTf = Util.FindChild<Transform>(gameObject, "Tf_UsingSpells");
        _expGuage = Util.FindChild<Image>(gameObject, "Image_ExpGauge", true);

        for (int i = 0; i < _usingSpells.Length; i++)
        {
            _usingSpells[i] = usingSpellsTf.GetChild(i).GetComponent<UI_UsingSpell>();
            _usingSpells[i].Init();
        }

        _waveInfo = GetText((int)Texts.Text_WaveInfo);
        _waveInfoFormat = Language.GetLanguage("WaveInfoFormat");
        _killCountValue = GetText((int)Texts.Text_KillCountValue);
        GetText((int)Texts.Text_KillCount).text = Language.GetLanguage("KillCount");

        #region 이벤트 바인드
        DefenseSceneManager.Instance.OnUpdatePlayerHp -= UpdatePlayerHpListner;
        DefenseSceneManager.Instance.OnUpdatePlayerHp += UpdatePlayerHpListner;
        #endregion
        SetExpGauge(0);
    }

    public void ClickedPause(PointerEventData data)
    {
        OnClickedPause.Invoke();
    }

    public void ClickedFast(PointerEventData data)
    {
        OnClickedFast.Invoke();
        GetButton((int)Buttons.Button_Fast).GetComponent<Image>().sprite =
            _speedSprite[Managers.Time.CurTimeScale - 1];
    }

    public void SetUsingSpell(int spellId, int spellNum)
    {
        _usingSpells[spellNum].SetUsingSpell(spellId);
        _usingSpellDict[spellId] = _usingSpells[spellNum];
        _usingSpells[spellNum].gameObject.AddUIEvent(ClickedUsingSpell, spellId);
    }

    void ClickedUsingSpell(int spellId, PointerEventData data)
    {
        _spellDesc.gameObject.SetActive(true);
        _spellDesc.SetSpellDesc(spellId);
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

    public void ShowLastWaveText()
    {
        GetText((int)Texts.Text_LastWave).gameObject.SetActive(true);
    }

    public void SetWaveInfoText(int curWave, int monsterCount)
    {
        _waveInfo.text = string.Format(_waveInfoFormat, curWave, monsterCount);
    }

    public void SetKillCount(int count)
    {
        _killCountValue.text = $"<color=#FF0000>{count}</color>";
    }
}
