using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ShopWeapon : UI_ShopSummon
{
    public Action<List<Equipment>> OnClickedSummon;
    // public Action<List<Item>> OnClickedSummon;
    public override void Init()
    {
        base.Init();
        GetText((int)Texts.Text_CoinAmount).text = ConstantData.CoinCostForSummonWeapon.ToString();
        GetText((int)Texts.Text_DiaAmount).text = ConstantData.DiaCostForSummonWeapon.ToString();
    }

    public override void SetBlock()
    {
        _summonCoinBlocker.gameObject.SetActive(!Managers.PlayerData.HasEnoughCoins(ConstantData.CoinCostForSummonWeapon));
        _summonDiaBlocker.gameObject.SetActive(!Managers.PlayerData.HasEnoughDia(ConstantData.DiaCostForSummonWeapon));
    }

    protected override void ClickedSummon(Buttons button,PointerEventData data)
    {
        switch (button)
        {
            case Buttons.Button_SummonCoin:
                if (!Managers.PlayerData.HasEnoughCoins(ConstantData.CoinCostForSummonWeapon))
                    return;
                Managers.PlayerData.DecreaseCoins(ConstantData.CoinCostForSummonWeapon);
                break;
            case Buttons.Button_SummonDia:
                if (!Managers.PlayerData.HasEnoughDia(ConstantData.DiaCostForSummonWeapon))
                    return;
                Managers.PlayerData.DecreaseDia(ConstantData.DiaCostForSummonWeapon);
                break;
        }
        SetBlock();
        OnClickedSummon(Managers.Item.SummonItems(EquipmentType.Weapon));
        LobbySceneManager.Instance.SaveDataOnLobbyScene();
    }
}
