using Define;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Setting : UI_Base
{
    [SerializeField] Sprite[] _soundOnOff; // 0 : On, 1 : Off 
    enum Objects
    {
        UI_SoundSetting,
        UI_FrameSetting,
        UI_LanguageSetting,
        UI_AccountSetting,
        Image_Back,
        Button_Quit,

        Obj_LanguageSettingCheck,
        Text_LanguageSettingCheck,
        Button_Agree,
        Button_Cancel
    }
    UI_SoundSetting _sound;
    UI_FrameSetting _frame;
    UI_LanguageSetting _language;
    UI_AccountSetting _account;

    RectTransform _uiRect;

    public Action OnSetting;

    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));
        _uiRect = GetObject((int)Objects.Image_Back).GetComponent<RectTransform>();

        _sound = GetObject((int)Objects.UI_SoundSetting).GetOrAddComponent<UI_SoundSetting>();
        _sound.Init();
        _sound.SetSound(_soundOnOff);

        _frame = GetObject((int)Objects.UI_FrameSetting).GetOrAddComponent<UI_FrameSetting>();
        _frame.Init();

        _language = GetObject((int)Objects.UI_LanguageSetting).GetOrAddComponent<UI_LanguageSetting>();
        _language.Init();
        _language.OnClickedLanguageSetting += ShowLanguageSettingCheck;
        GetObject((int)Objects.Button_Cancel).AddUIEvent((PointerEventData data)
            => GetObject((int)Objects.Obj_LanguageSettingCheck).SetActive(false));

        _account = GetObject((int)Objects.UI_AccountSetting).GetOrAddComponent<UI_AccountSetting>();
        _account.Init();

        GetObject((int)Objects.Obj_LanguageSettingCheck).SetActive(false);
        GetObject((int)Objects.Button_Quit).AddUIEvent((PointerEventData data) => HideSetting());
    }

    public void ShowSetting()
    {
        gameObject.SetActive(true);
        _uiRect.localScale = Vector3.one * 0.5f;
        LeanTween.scale(_uiRect, Vector3.one, 0.1f);
        OnSetting?.Invoke();
    }

    public void HideSetting()
    {
        gameObject.SetActive(false);
        OnSetting?.Invoke();
    }

    void ShowLanguageSettingCheck(GameLanguage language)
    {
        GetObject((int)Objects.Obj_LanguageSettingCheck).SetActive(true);
        GetObject((int)Objects.Button_Agree).RemoveEvent();
        GetObject((int)Objects.Button_Agree).AddUIEvent((PointerEventData data) => 
            Managers.Game.SetGameLanguage(language));
        GetObject((int)Objects.Text_LanguageSettingCheck).GetComponent<TextMeshProUGUI>().text
            = Language.GetLanguage("LanguageSettingCheck");
    }
}

public class UI_SoundSetting : UI_Base
{
    enum Buttons
    {
        Button_BGM,
        Button_SFX
    }
    enum Texts
    {
        Text_BGM,
        Text_SFX,
    }
    enum Images
    {
        Image_BGMOff,
        Image_SFXOff,
        Image_BGMOnOff,
        Image_SFXOnOff,
    }

    Sprite[] _soundOnOff; // 0 : On, 1 : Off 
    Image _bgmOff;
    Image _sfxOff;
    Image _bgmOnOff;
    Image _sfxOnOff;

    RectTransform _bgmButtonRect;
    RectTransform _sfxButtonRect;
    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));

        _bgmOff = GetImage((int)Images.Image_BGMOff);
        _sfxOff = GetImage((int)Images.Image_SFXOff);
        _bgmOnOff = GetImage((int)Images.Image_BGMOnOff);
        _sfxOnOff = GetImage((int)Images.Image_SFXOnOff);

        _bgmButtonRect = GetButton((int)Buttons.Button_BGM).GetComponent<RectTransform>();
        _sfxButtonRect = GetButton((int)Buttons.Button_SFX).GetComponent<RectTransform>();

        GetButton((int)Buttons.Button_BGM).gameObject.AddUIEvent(ClickedButton, Buttons.Button_BGM);
        GetButton((int)Buttons.Button_SFX).gameObject.AddUIEvent(ClickedButton, Buttons.Button_SFX);
    }

    void ClickedButton(Buttons buttonType, PointerEventData data)
    {
        switch (buttonType)
        {
            case Buttons.Button_BGM:
                ButtonClickEffect(_bgmButtonRect, 1.2f, time: 0.1f);
                Managers.Game.SetBGMOnOff();
                break;
            case Buttons.Button_SFX:
                ButtonClickEffect(_sfxButtonRect, 1.2f, time: 0.1f);
                Managers.Game.SetSFXOnOff();
                break;
        }
        SetImages();
    }

    public void SetSound(Sprite[] soundOfOff)
    {
        _soundOnOff = soundOfOff;
        GetText((int)Texts.Text_BGM).text = Language.GetLanguage("BGM");
        GetText((int)Texts.Text_SFX).text = Language.GetLanguage("SFX");
        SetImages();
    }

    void SetImages()
    {
        if (Managers.PlayerData.BgmOn)
        {
            _bgmOff.gameObject.SetActive(false);
            _bgmOnOff.sprite = _soundOnOff[0];
        }
        else
        {
            _bgmOff.gameObject.SetActive(true);
            _bgmOnOff.sprite = _soundOnOff[1];
        }
        if (Managers.PlayerData.SfxOn)
        {
            _sfxOff.gameObject.SetActive(false);
            _sfxOnOff.sprite = _soundOnOff[0];
        }
        else
        {
            _sfxOff.gameObject.SetActive(true);
            _sfxOnOff.sprite = _soundOnOff[1];
        }
    }


}

public class UI_FrameSetting : UI_Base
{
    enum Objects
    {
        Text_FrameTitle,
        Image_Off30Fps,
        Image_Off60Fps,
    }
    enum Buttons
    {
        Button_30Fps,
        Button_60Fps,
    }
    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));
        Bind<Button>(typeof(Buttons));
        GetObject((int)Objects.Text_FrameTitle).GetComponent<TextMeshProUGUI>().text = 
            Language.GetLanguage("FrameSettingTitle");

        GetButton((int)Buttons.Button_60Fps).gameObject.AddUIEvent(ClickedFrameButton, Buttons.Button_60Fps);
        GetButton((int)Buttons.Button_30Fps).gameObject.AddUIEvent(ClickedFrameButton, Buttons.Button_30Fps);
        SetFrameImages();
    }

    void ClickedFrameButton(Buttons buttonType,PointerEventData data)
    {
        switch (buttonType)
        {
            case Buttons.Button_60Fps:
                Managers.Game.SetFrameRate(ConstantData.FrameRate60);
                break;
            case Buttons.Button_30Fps:
                Managers.Game.SetFrameRate(ConstantData.FrameRate30);
                break;
        }
        SetFrameImages();
    }

    void SetFrameImages()
    {
        int curFrame = Managers.PlayerData.FrameRate;
        GetObject((int)Objects.Image_Off30Fps).SetActive(curFrame == ConstantData.FrameRate60);
        GetObject((int)Objects.Image_Off60Fps).SetActive(curFrame == ConstantData.FrameRate30);
    }
}

public class UI_LanguageSetting : UI_Base
{
    enum Objects
    {
        Text_LanguageTitle,
        Button_Kor,
        Button_Eng,
        Image_OffKor,
        Image_OffEng,
    }
    public Action<GameLanguage> OnClickedLanguageSetting;

    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));
        GetObject((int)Objects.Text_LanguageTitle).GetComponent<TextMeshProUGUI>().text =
            Language.GetLanguage("LanguageSettingTitle");
        SetLanguageButtons();
    }

    void SetLanguageButtons()
    {
        GetObject((int)Objects.Image_OffKor).SetActive(Managers.PlayerData.GameLanguage == GameLanguage.English);
        GetObject((int)Objects.Image_OffEng).SetActive(Managers.PlayerData.GameLanguage == GameLanguage.Korean);
        GetObject((int)Objects.Button_Kor).RemoveEvent();
        GetObject((int)Objects.Button_Eng).RemoveEvent();
        switch (Managers.PlayerData.GameLanguage)
        {
            case GameLanguage.English:
                GetObject((int)Objects.Button_Kor).AddUIEvent((PointerEventData data) 
                    => OnClickedLanguageSetting.Invoke(GameLanguage.Korean));
                break;
            case GameLanguage.Korean:
                GetObject((int)Objects.Button_Eng).AddUIEvent((PointerEventData data)
                    => OnClickedLanguageSetting.Invoke(GameLanguage.English));
                break;
        }
    }
}

public class UI_AccountSetting : UI_Base
{
    enum Texts
    {
        Text_Account,
        Text_UserID,
        Text_Copy,
        Text_CopiedToClipBoard,
    }
    enum Objects
    {
        Button_Copy,
    }
    RectTransform _copiedTextRect;
    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(Objects));

        GetText((int)Texts.Text_Account).text = Language.GetLanguage("AccountSettingTitle");
        GetText((int)Texts.Text_UserID).text = FirebaseManager.Instance.CurrentUserId;
        GetText((int)Texts.Text_Copy).text = Language.GetLanguage("Copy");
        GetText((int)Texts.Text_CopiedToClipBoard).text = Language.GetLanguage("CopiedToClipBoard");
        _copiedTextRect = GetText((int)Texts.Text_CopiedToClipBoard).rectTransform;

        GetObject((int)Objects.Button_Copy).AddUIEvent(ClickedCopy);

    }

    void ClickedCopy(PointerEventData data)
    {
        TextEditor te = new TextEditor { text = GetText((int)Texts.Text_UserID).text };
        te.SelectAll();
        te.Copy();
        Debug.Log($"Copied to clipboard: {te.text}");

        TextMeshProUGUI text = GetText((int)Texts.Text_CopiedToClipBoard);
        text.LeanAlphaText(1, 0.3f).setOnComplete(()
            => text.LeanAlphaText(0f, 0.3f));
    }
}
