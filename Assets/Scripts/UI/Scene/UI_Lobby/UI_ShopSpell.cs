using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ShopSpell : UI_Base
{
    enum Buttons
    {
        Button_SummonCoin,
        Button_SummonDia
    }

    public Action<Dictionary<int,int>> OnClickedSummon;
    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.Button_SummonCoin).gameObject.AddUIEvent(ClickedSummon, Buttons.Button_SummonCoin);
        GetButton((int)Buttons.Button_SummonDia).gameObject.AddUIEvent(ClickedSummon, Buttons.Button_SummonDia);

        Util.FindChild<TextMeshProUGUI>(gameObject, "Text_Summonx10ByCoin", true).text
            = string.Format(Language.GetLanguage("Summon"), "x 10");
        Util.FindChild<TextMeshProUGUI>(gameObject, "Text_Summonx10ByDia", true).text
            = string.Format(Language.GetLanguage("Summon"), "x 10");
    }

    void ClickedSummon(Buttons button, PointerEventData data)
    {
        OnClickedSummon(Managers.Spell.SummonSpells());
    }
}
