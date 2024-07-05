using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_CantShowAds : UI_Popup
{
    TextMeshProUGUI _cantLoadedAdsText;
    TextMeshProUGUI _countText;
    int _count;
    public void ShowCantShowAds(System.Action callback)
    {
        _cantLoadedAdsText = Util.FindChild<TextMeshProUGUI>(gameObject, "Text_CantShowAds",recursive:true);
        _countText = Util.FindChild<TextMeshProUGUI>(gameObject, "Text_Count", recursive: true);
        _cantLoadedAdsText.text = Language.GetLanguage("CantShowAds");
        _count = 5;
        _countText.text = _count.ToString();
        StartCoroutine(CoCount(callback));
    }
    IEnumerator CoCount(System.Action callback)
    {
        while(true)
        {
            yield return new WaitForSecondsRealtime(1);
            _count--;
            _countText.text = _count.ToString();
            if(_count <= 0)
            {
                callback?.Invoke();
                Managers.UI.ClosePopupUI(this);
                break;
            }
        }
    }
}
