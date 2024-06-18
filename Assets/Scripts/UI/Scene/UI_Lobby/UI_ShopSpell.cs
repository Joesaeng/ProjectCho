using System;
using System.Collections;
using System.Collections.Generic;
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
    }

    void ClickedSummon(Buttons button, PointerEventData data)
    {
        OnClickedSummon(SpellManager.Instance.SummonSpells());
    }
}
