using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TimeManager
{
    public bool IsPause { get; private set; }
    public int CurTimeScale { get; private set; } = 1;
    public void Init()
    {
        IsPause = false;

        CurTimeScale = 1;
        Time.timeScale = CurTimeScale;
    }

    public void GamePause()
    {
        Time.timeScale = 0f;
        IsPause = true;
    }

    public void GameResume()
    {
        Time.timeScale = CurTimeScale;
        IsPause = false;
    }

    public int ChangeTimeScale()
    {
        switch (CurTimeScale)
        {
            case 1:
                Time.timeScale = 2f;
                CurTimeScale = 2;
                break;
            case 2:
                Time.timeScale = 1f;
                CurTimeScale = 1;
                break;
        }
        return CurTimeScale;
    }

    public void Clear()
    {
        IsPause = false;
        CurTimeScale = 1;
        Time.timeScale = CurTimeScale;
    }
}