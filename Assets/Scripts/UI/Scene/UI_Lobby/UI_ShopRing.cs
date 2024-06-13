using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ShopRing : UI_Base
{
    enum Buttons
    {
        Button_SummonCoin,
        Button_SummonDia
    }

    public Action<List<Item>> OnClickedSummon;
    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        GetButton((int)Buttons.Button_SummonCoin).gameObject.AddUIEvent(ClickedSummon, Buttons.Button_SummonCoin);
        GetButton((int)Buttons.Button_SummonDia).gameObject.AddUIEvent(ClickedSummon, Buttons.Button_SummonDia);
    }

    void ClickedSummon(Buttons button, PointerEventData data)
    {
        // Buttons에 따라서 Player의 Coin, Dia 확인해서 이벤트 실행
        List<Item> summonItems = new List<Item>();
        for (int i = 0; i < 10; ++i)
        {
            Item newItem = Managers.Item.GenerateRandomItem(EquipmentType.Ring);
            summonItems.Add(newItem);
        }
        Managers.Player.AddItems(summonItems);
        OnClickedSummon(summonItems);
    }
}
