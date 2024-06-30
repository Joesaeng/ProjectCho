using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ToggleOption : UI_Base
{
    StatusType _statusType;
    Toggle _toggle;

    public Action<StatusType, bool> OnToggleChanged;
    public override void Init()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(ValueChanged);
    }

    public void SetType(StatusType statusType)
    {
        _statusType = statusType;
        Util.FindChild<TextMeshProUGUI>(gameObject,"Text_OptionName").text 
            = Language.GetLanguage(_statusType.ToString());
    }

    public void SetToggleOn(bool value)
    {
        _toggle.isOn = value;
    }

    void ValueChanged(bool value)
    {
        Managers.Sound.Play("ui_toggle");
        OnToggleChanged.Invoke(_statusType,value);
    }
}
