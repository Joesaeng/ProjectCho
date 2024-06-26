using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ShopRing : UI_ShopSummon
{
    public Action<List<Equipment>> OnClickedSummon;
    // public Action<List<Item>> OnClickedSummon;
    public override void Init()
    {
        base.Init();
        GetText((int)Texts.Text_CoinAmount).text = ConstantData.CoinCostForSummonRing.ToString();
        GetText((int)Texts.Text_DiaAmount).text = ConstantData.DiaCostForSummonRing.ToString();
    }

    public override void SetBlock()
    {
        _summonCoinBlocker.gameObject.SetActive(!Managers.PlayerData.HasEnoughCoins(ConstantData.CoinCostForSummonRing));
        _summonDiaBlocker.gameObject.SetActive(!Managers.PlayerData.HasEnoughDia(ConstantData.DiaCostForSummonRing));
    }

    protected override void ClickedSummon(Buttons button, PointerEventData data)
    {
        switch (button)
        {
            case Buttons.Button_SummonCoin:
                if (!Managers.PlayerData.HasEnoughCoins(ConstantData.CoinCostForSummonRing))
                    return;
                Managers.PlayerData.DecreaseCoins(ConstantData.CoinCostForSummonRing);
                break;
            case Buttons.Button_SummonDia:
                if (!Managers.PlayerData.HasEnoughDia(ConstantData.DiaCostForSummonRing))
                    return;
                Managers.PlayerData.DecreaseDia(ConstantData.DiaCostForSummonRing);
                break;
        }
        SetBlock();
        OnClickedSummon(Managers.Item.SummonItems(EquipmentType.Ring));
        LobbySceneManager.Instance.SaveDataOnLobbyScene();
    }
}
