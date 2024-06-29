using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_DefenseScenePause : UI_Popup
{
    enum Buttons
    {
        Button_BGM,
        Button_SFX,
        Button_Lobby,
        Button_Resume,
        Button_Agree,
        Button_Cancel,
    }
    enum Images
    {
        Image_LobbyCheckBlur,
        Image_LobbyCheck,
        Image_BGM,
        Image_SFX
    }

    enum Texts
    {
        Text_Lobby,
        Text_BGM,
        Text_SFX,
        Text_LobbyCheck,
    }

    public Action OnClickedResume;
    public Action OnClickedLobby;

    UI_DefeatEnemies _defeatEnemiesUI;
    Sprite _soundOn;
    Sprite _soundOff;

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        GetButton((int)Buttons.Button_Lobby).gameObject.AddUIEvent(ClickedLobby);
        GetButton((int)Buttons.Button_Resume).gameObject.AddUIEvent(ClickedResume);

        GetImage((int)Images.Image_LobbyCheckBlur).gameObject.SetActive(false);
        GetImage((int)Images.Image_LobbyCheck).gameObject.SetActive(false);
        GetButton((int)Buttons.Button_Agree).gameObject.AddUIEvent(ClickedAgree);
        GetButton((int)Buttons.Button_Cancel).gameObject.AddUIEvent(ClickedCancel);

        GetText((int)Texts.Text_BGM).text = Language.GetLanguage("BGM");
        GetText((int)Texts.Text_SFX).text = Language.GetLanguage("SFX");
        GetText((int)Texts.Text_Lobby).text = Language.GetLanguage("Lobby");
        GetText((int)Texts.Text_LobbyCheck).text = Language.GetLanguage("ReturnToLobby");

        _defeatEnemiesUI = Util.FindChild<UI_DefeatEnemies>(gameObject,"UI_DefeatEnemies",true);
        _defeatEnemiesUI.Init();

        _soundOn = Resources.Load<Sprite>("UI/Useds/SoundOn");
        _soundOff = Resources.Load<Sprite>("UI/Useds/SoundOff");

        GetButton((int)Buttons.Button_BGM).gameObject.AddUIEvent((PointerEventData data) => ClickedBGM());
        GetButton((int)Buttons.Button_SFX).gameObject.AddUIEvent((PointerEventData data) => ClickedSFX());

        SetSoundImage();
    }

    void ClickedBGM()
    {
        Managers.Game.SetBGMOnOff();
        SetSoundImage();
    }

    void ClickedSFX()
    {
        Managers.Game.SetSFXOnOff();
        SetSoundImage();
    }

    void SetSoundImage()
    {
        GetImage((int)Images.Image_BGM).sprite = Managers.PlayerData.BgmOn ? _soundOn : _soundOff;
        GetImage((int)Images.Image_SFX).sprite = Managers.PlayerData.SfxOn ? _soundOn : _soundOff;
    }

    void ClickedLobby(PointerEventData data)
    {
        GetImage((int)Images.Image_LobbyCheckBlur).gameObject.SetActive(true);
        GetImage((int)Images.Image_LobbyCheck).gameObject.SetActive(true);
    }
    void ClickedResume(PointerEventData data)
    {
        OnClickedResume.Invoke();
    }

    void ClickedAgree(PointerEventData data)
    {
        OnClickedLobby.Invoke();
    }

    void ClickedCancel(PointerEventData data)
    {
        GetImage((int)Images.Image_LobbyCheckBlur).gameObject.SetActive(false);
        GetImage((int)Images.Image_LobbyCheck).gameObject.SetActive(false);
    }

    public void SetDefeatEnemies(Dictionary<ElementType, int> defeatEnemies)
    {
        _defeatEnemiesUI.SetDefeatEnemies(defeatEnemies);
    }
}
