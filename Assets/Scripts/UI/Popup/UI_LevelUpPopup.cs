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
    public Action<LevelUpOptions> OnClickedLevelUpOption;

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
    }

    void MakeLevelUpOptions()
    {
        for (int i = 0; i < Managers.Game.SetLevelUpOptions.Count; i++)
        {
            UI_LevelUpOptions option = Managers.UI.MakeSubItem<UI_LevelUpOptions>(_panelLevelupTf);
            // option.Init();
            option.Set(Managers.Game.SetLevelUpOptions[i]);
            option.gameObject.AddUIEvent(ClickedLevelUpOption, Managers.Game.SetLevelUpOptions[i]);
        }
    }

    public void ClickedLevelUpOption(LevelUpOptions option, PointerEventData data)
    {
        OnClickedLevelUpOption.Invoke(option);
        OnClickedLevelUpOption = null;
        Managers.UI.ClosePopupUI(this);
    }

    public void ClickedReroll(PointerEventData data)
    {
        Managers.Game.CreateLevelUpOptions();
        GetButton((int)Buttons.Button_Reroll).gameObject.SetActive(false);
        for (int i = 0; i < _panelLevelupTf.childCount; i++)
        {
            Managers.Resource.Destroy(_panelLevelupTf.GetChild(i).gameObject);
        }
        MakeLevelUpOptions();
    }
}
