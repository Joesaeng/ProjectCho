using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LevelUpPopup : UI_Popup
{
    enum Objects
    {
        Panel_LevelUpOptions,
        Blocker_ShowOptionWait
    }
    enum Buttons
    {
        Button_Reroll
    }
    enum Texts
    {
        Text_Reroll
    }

    Transform _panelLevelupTf;
    List<UI_LevelUpOptions> _levelUpOptionsUI;
    public Action<LevelUpOptions> OnClickedLevelUpOption;
    public Action OnClickedReroll;

    GameObject _rerollButtonObj;
    TextMeshProUGUI _rerollButonText;

    GameObject _blockerOptionWait;
    bool _rerollButtonIsSeeAds;

    public override void Init()
    {
        base.Init();
        Bind<GameObject>(typeof(Objects));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        _rerollButonText = GetText((int)Texts.Text_Reroll);
        _rerollButtonObj = GetButton((int)Buttons.Button_Reroll).gameObject;
        _rerollButtonObj.SetActive(true);
        _rerollButtonObj.AddUIEvent(ClickedReroll);

        SetRerollText();

        _panelLevelupTf = GetObject((int)Objects.Panel_LevelUpOptions).transform;
        _blockerOptionWait = GetObject((int)Objects.Blocker_ShowOptionWait);
        _blockerOptionWait.SetActive(true);

        MakeLevelUpOptions();
        DefenseSceneManager.Instance.OnSetLevelUpPopup += LevelUpListner;
        DefenseSceneManager.Instance.OnRerollLevelUpPopup += SetLevelUpOptions;
    }

    void SetRerollText()
    {
        int availableRerollCount = DefenseSceneManager.Instance.AvailableRerollCount;
        if (availableRerollCount > 0)
        {
            _rerollButtonIsSeeAds = false;
            _rerollButonText.text = $"{Language.GetLanguage("Reroll")}\n[{DefenseSceneManager.Instance.AvailableRerollCount}/{ConstantData.MaxLevelupOptionRerollCount}]";
        }
        else
        {
            _rerollButtonIsSeeAds = true;
            _rerollButonText.text = $"{Language.GetLanguage("RechargeReroll")}";
        }
    }

    void LevelUpListner(List<LevelUpOptions> levelUpOptions)
    {
        Managers.Sound.Play("ui_levelup");
        gameObject.SetActive(true);
        _blockerOptionWait.SetActive(true);
        _rerollButtonObj.SetActive(true);
        SetRerollText();
        SetLevelUpOptions(levelUpOptions);
    }

    void MakeLevelUpOptions()
    {
        _levelUpOptionsUI = new();
        for (int i = 0; i < ConstantData.LevelUpOptionsBasicCount; i++)
        {
            if (!_panelLevelupTf.TryGetChild(i, out UI_LevelUpOptions option))
                option = Managers.UI.MakeSubItem<UI_LevelUpOptions>(_panelLevelupTf);
            option.Init();
            _levelUpOptionsUI.Add(option);
        }
    }

    public void ClickedLevelUpOption(LevelUpOptions option, PointerEventData data)
    {
        OnClickedLevelUpOption.Invoke(option);
        Managers.Sound.Play("ui_spelllevelup");
        gameObject.SetActive(false);
    }

    public void ClickedReroll(PointerEventData data)
    {
        if (_rerollButtonIsSeeAds)
        {
            Managers.Ads.ShowRewardedAd(() =>
            {
                // Managers.Time.GamePause();
                DefenseSceneManager.Instance.AvailableRerollCount = ConstantData.MaxLevelupOptionRerollCount;
                SetRerollText();
            });
        }
        else
        {
            GetButton((int)Buttons.Button_Reroll).gameObject.SetActive(false);
            OnClickedReroll.Invoke();
        }
    }

    void SetLevelUpOptions(List<LevelUpOptions> levelUpOptions)
    {
        for (int i = 0; i < levelUpOptions.Count; i++)
        {
            _levelUpOptionsUI[i].Set(levelUpOptions[i]);
            GameObject obj = _levelUpOptionsUI[i].gameObject;
            obj.RemoveEvent();
            obj.AddUIEvent(ClickedLevelUpOption, levelUpOptions[i]);
            RectTransform rect = obj.GetComponent<RectTransform>();
            // rect.localScale = Vector3.one * 0.5f;
            rect.LeanScale(Vector3.one * 1.2f, 0.1f).setIgnoreTimeScale(true).setOnComplete(() =>
                rect.LeanScale(Vector3.one, 0.1f).setIgnoreTimeScale(true).setOnComplete(() =>
                _blockerOptionWait.SetActive(false)));
        }
    }
}
