using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LevelUpPopup : UI_Popup
{
    enum GameObjects
    {
        Panel_LevelUpOptions
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
    List<UI_LevelUpOptions> _levelUpOptions;
    public Action<LevelUpOptions> OnClickedLevelUpOption;
    public Action OnClickedReroll;

    public override void Init()
    {
        base.Init();
        Bind<GameObject>(typeof(GameObjects));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        GetText((int)Texts.Text_Reroll).text = Language.GetLanguage("Reroll");
        GetButton((int)Buttons.Button_Reroll).gameObject.SetActive(true);
        GetButton((int)Buttons.Button_Reroll).gameObject.AddUIEvent(ClickedReroll);

        _panelLevelupTf = GetObject((int)GameObjects.Panel_LevelUpOptions).transform;

        MakeLevelUpOptions();
        DefenseSceneManager.Instance.OnSetLevelUpPopup += LevelUpListner;
        DefenseSceneManager.Instance.OnRerollLevelUpPopup += SetLevelUpOptions;
    }

    void LevelUpListner(List<LevelUpOptions> levelUpOptions)
    {
        gameObject.SetActive(true);
        GetButton((int)Buttons.Button_Reroll).gameObject.SetActive(true);
        SetLevelUpOptions(levelUpOptions);
    }

    void MakeLevelUpOptions()
    {
        _levelUpOptions = new();
        for (int i = 0; i < ConstantData.LevelUpOptionsBasicCount; i++)
        {
            UI_LevelUpOptions option = Managers.UI.MakeSubItem<UI_LevelUpOptions>(_panelLevelupTf);
            _levelUpOptions.Add(option);
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
            GameObject obj = _levelUpOptions[i].gameObject;
            obj.gameObject.SetActive(false);
            _levelUpOptions[i].Set(levelUpOptions[i]);
            obj.gameObject.RemoveEvent();
            obj.gameObject.AddUIEvent(ClickedLevelUpOption, levelUpOptions[i]);
            obj.gameObject.SetActive(true);
        }
    }
}
