using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UI_ShopSummon : UI_Base
{
    protected enum Buttons
    {
        Button_SummonCoin,
        Button_SummonDia
    }
    protected enum Texts
    {
        Text_Summonx10ByCoin,
        Text_Summonx10ByDia,
        Text_CoinAmount,
        Text_DiaAmount,
    }

    protected enum Images
    {
        Image_SummonCoinBlock,
        Image_SummonDiaBlock,
    }

    protected Image _summonCoinBlocker;
    protected Image _summonDiaBlocker;

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));

        GetButton((int)Buttons.Button_SummonCoin).gameObject.AddUIEvent(ClickedSummon, Buttons.Button_SummonCoin);
        GetButton((int)Buttons.Button_SummonDia).gameObject.AddUIEvent(ClickedSummon, Buttons.Button_SummonDia);

        GetText((int)Texts.Text_Summonx10ByCoin).text = string.Format(Language.GetLanguage("Summon"), "x 10");
        GetText((int)Texts.Text_Summonx10ByDia).text = string.Format(Language.GetLanguage("Summon"), "x 10");

        _summonCoinBlocker = GetImage((int)Images.Image_SummonCoinBlock);
        _summonDiaBlocker = GetImage((int)Images.Image_SummonDiaBlock);
    }

    public abstract void SetBlock();

    public void ShowImage()
    {
        Image shopImage = Util.FindChild<Image>(gameObject,"Image_ShopImage");
        shopImage.color = Color.black;
        RectTransform rect = shopImage.rectTransform;
        LeanTween.color(rect, Color.white, 0.5f);
    }

    protected abstract void ClickedSummon(Buttons button, PointerEventData data);
    
}
