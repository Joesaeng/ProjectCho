using Define;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager 
{
    private int selectedStage = 0;
    public int SelectedStage { get => selectedStage; set => selectedStage = value; }

    public Action OnSetBgmOnOff;

    public void Init()
    {
        SetFrameRate(Managers.PlayerData.FrameRate);
    }

    public void SetFrameRate(int frameRateValue)
    {
        Application.targetFrameRate = frameRateValue;
        Managers.PlayerData.FrameRate = frameRateValue;
    }

    public void SetBGMOnOff()
    {
        Managers.PlayerData.BgmOn = !Managers.PlayerData.BgmOn;
        OnSetBgmOnOff.Invoke();
    }

    public void SetSFXOnOff()
    {
        Managers.PlayerData.SfxOn = !Managers.PlayerData.SfxOn;
    }

    public void SetGameLanguage(GameLanguage language)
    {
        Managers.PlayerData.GameLanguage = language;
        Managers.Scene.LoadSceneWithLoadingScene(Scene.Lobby);
    }
}
