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
        Image_LobbyCheck
    }

    enum Texts
    {
        Text_Setting,
        Text_Lobby,
        Text_BGM,
        Text_SFX,
    }

    public Action OnClickedResume;
    public Action OnClickedLobby;
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
}
