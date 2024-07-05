using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemOptionFilter : UI_Base
{
    enum Buttons
    {
        Button_Apply,
        Button_AllToggleOn,
        Button_AllToggleOff,
        Button_Quit,
    }

    enum Texts
    {
        Text_OptionFilter,
        Text_Apply,
        Text_AllToggleOn,
        Text_AllToggleOff,
        Text_Quit,
    }
    public Dictionary<StatusType,bool> _filterDict = new();
    public Dictionary<StatusType,UI_ToggleOption> _toggleOptionsDict = new();

    public Action<Dictionary<StatusType,bool>> OnApplyItemOptionFilter;
    public Action OnResetFilter;

    Transform _filtersTf;
    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));

        SetTexts();
        SetButtons();

        _filtersTf = Util.FindChild<Transform>(gameObject, "Content_Filters", true);
        InitToggles();
    }

    void SetTexts()
    {
        GetText((int)Texts.Text_OptionFilter).text = Language.GetLanguage("ItemOptionFilter");
        GetText((int)Texts.Text_Apply).text = Language.GetLanguage("Apply");
        GetText((int)Texts.Text_AllToggleOn).text = Language.GetLanguage("AllToggleOn");
        GetText((int)Texts.Text_AllToggleOff).text = Language.GetLanguage("AllToggleOff");
        GetText((int)Texts.Text_Quit).text = Language.GetLanguage("Quit");

    }

    void SetButtons()
    {
        GetButton((int)Buttons.Button_Apply).gameObject.AddUIEvent(ClickedApplyButton);
        GetButton((int)Buttons.Button_AllToggleOn).gameObject.AddUIEvent(ClickedOnAllButton);
        GetButton((int)Buttons.Button_AllToggleOff).gameObject.AddUIEvent(ClickedOffAllButton);
        GetButton((int)Buttons.Button_Quit).gameObject.AddUIEvent(ClickedQuitButton);
    }

    void InitToggles()
    {
        int toggleIndex = 0;
        for (int i = 0; i < Enum.GetValues(typeof(StatusType)).Length; i++)
        {
            StatusType type = (StatusType)i;
            if (type == StatusType.Spell || type == StatusType.BaseDamage)
                continue;

            UI_ToggleOption toggle;
            if (!_filtersTf.TryGetChild<UI_ToggleOption>(toggleIndex, out toggle))
            {
                toggle = Managers.UI.MakeSubItem<UI_ToggleOption>(_filtersTf);
            }

            toggle.transform.localScale = Vector3.one;
            toggle.transform.localPosition = Vector3.zero;
            toggle.Init();
            toggle.SetType(type);
            toggle.OnToggleChanged += ChangedToggleListner;
            toggle.SetToggleOn(true);
            _filterDict[type] = true;
            _toggleOptionsDict[type] = toggle;
            toggleIndex++;
        }
    }

    void ClickedOnAllButton(PointerEventData data)
    {
        ResetToggleOptions(true);
    }
    void ClickedOffAllButton(PointerEventData data)
    {
        ResetToggleOptions(false);
    }

    void ClickedApplyButton(PointerEventData data)
    {
        Managers.Sound.Play("ui_click3");
        ApplyItemOptionFilter();
    }

    void ClickedQuitButton(PointerEventData data)
    {
        Managers.Sound.Play("ui_click3");
        gameObject.SetActive(false);
    }

    void ResetToggleOptions(bool onoff)
    {
        Managers.Sound.Play("ui_click3");
        foreach (var type in _filterDict.Keys.ToList())
        {
            _filterDict[type] = onoff;
            if (_toggleOptionsDict.ContainsKey(type))
            {
                _toggleOptionsDict[type].SetToggleOn(onoff);
            }
        }
        OnResetFilter.Invoke();
    }

    void ApplyItemOptionFilter()
    {
        OnApplyItemOptionFilter.Invoke(_filterDict);
    }

    void ChangedToggleListner(StatusType statusType, bool value)
    {
        if(_filterDict.ContainsKey(statusType))
            _filterDict[statusType] = value;
    }
}
