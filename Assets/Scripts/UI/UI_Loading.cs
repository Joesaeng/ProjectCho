using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Loading : UI_Popup
{
    RectTransform _backgroundRectUi;
    Image _backgroundImage;
    Color _originalColor;
    bool _isLoadingUI;
    public System.Action OnCompleteLoadingUI;
    public override void Init()
    {
        _backgroundRectUi = Util.FindChild<RectTransform>(gameObject,"Image_BackGround");
        _backgroundImage = Util.FindChild<Image>(gameObject, "Image_BackGround");
        _originalColor = _backgroundImage.color;
    }

    public void ShowLoadingUI()
    {
        _isLoadingUI = false;
        Color newColor = _originalColor;
        newColor.a = 0f;
        _backgroundImage.color = newColor;

        StartCoroutine(CoLoadingUI());
        _backgroundRectUi.LeanAlpha(1, 0.2f).setOnComplete(()=>_isLoadingUI = true);
    }

    public void HideLoadingUI()
    {
        _isLoadingUI = false;
        Color newColor = _originalColor;
        _backgroundImage.color = newColor;

        StartCoroutine(CoLoadingUI());
        _backgroundRectUi.LeanAlpha(0, 0.2f).setOnComplete(() => _isLoadingUI = true);
    }

    public IEnumerator CoLoadingUI()
    {
        while(!_isLoadingUI)
        {
            yield return null;
        }
        OnCompleteLoadingUI.Invoke();
        OnCompleteLoadingUI = null;
    }
}
