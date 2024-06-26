using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ShopSpell : UI_ShopSummon
{
    public Action<Dictionary<int,int>> OnClickedSummon;
    public override void Init()
    {
        base.Init();
        GetText((int)Texts.Text_CoinAmount).text = ConstantData.CoinCostForSummonSpell.ToString();
        GetText((int)Texts.Text_DiaAmount).text = ConstantData.DiaCostForSummonSpell.ToString();
    }

    public override void SetBlock()
    {
        _summonCoinBlocker.gameObject.SetActive(!Managers.PlayerData.HasEnoughCoins(ConstantData.CoinCostForSummonSpell));
        _summonDiaBlocker.gameObject.SetActive(!Managers.PlayerData.HasEnoughDia(ConstantData.DiaCostForSummonSpell));
    }

    protected override void ClickedSummon(Buttons button, PointerEventData data)
    {
        switch (button)
        {
            case Buttons.Button_SummonCoin:
                if (!Managers.PlayerData.HasEnoughCoins(ConstantData.CoinCostForSummonSpell))
                    return;
                Managers.PlayerData.DecreaseCoins(ConstantData.CoinCostForSummonSpell);
                break;
            case Buttons.Button_SummonDia:
                if (!Managers.PlayerData.HasEnoughDia(ConstantData.DiaCostForSummonSpell))
                    return;
                Managers.PlayerData.DecreaseDia(ConstantData.DiaCostForSummonSpell);
                break;
        }
        SetBlock();
        OnClickedSummon(Managers.Spell.SummonSpells());
        LobbySceneManager.Instance.SaveDataOnLobbyScene();
    }
}
