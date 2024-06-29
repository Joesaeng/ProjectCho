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
    GameObject _blockerOptionWait;

    public override void Init()
    {
        base.Init();
        Bind<GameObject>(typeof(Objects));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        GetText((int)Texts.Text_Reroll).text = Language.GetLanguage("Reroll");
        _rerollButtonObj = GetButton((int)Buttons.Button_Reroll).gameObject;
        _rerollButtonObj.SetActive(true);
        _rerollButtonObj.AddUIEvent(ClickedReroll);

        _panelLevelupTf = GetObject((int)Objects.Panel_LevelUpOptions).transform;
        _blockerOptionWait = GetObject((int)Objects.Blocker_ShowOptionWait);
        _blockerOptionWait.SetActive(true);

        MakeLevelUpOptions();
        DefenseSceneManager.Instance.OnSetLevelUpPopup += LevelUpListner;
        DefenseSceneManager.Instance.OnRerollLevelUpPopup += SetLevelUpOptions;
    }

    void LevelUpListner(List<LevelUpOptions> levelUpOptions)
    {
        gameObject.SetActive(true);
        _blockerOptionWait.SetActive(true);
        _rerollButtonObj.SetActive(true);
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
        gameObject.SetActive(false);
    }

    public void ClickedReroll(PointerEventData data)
    {
        GetButton((int)Buttons.Button_Reroll).gameObject.SetActive(false);
        OnClickedReroll.Invoke();
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
