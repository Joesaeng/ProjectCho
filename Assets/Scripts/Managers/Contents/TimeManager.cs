using System;
using UnityEngine;

public class TimeManager
{
    public bool IsPause { get; private set; }
    public int CurTimeScale { get; private set; } = 1;

    public event Action OnGamePause;
    public event Action OnGameResume;
    public Action OnChangeTimeScale;

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
        OnGamePause?.Invoke();
    }

    public void GameResume()
    {
        Time.timeScale = CurTimeScale;
        IsPause = false;
        OnGameResume?.Invoke();
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
        OnChangeTimeScale?.Invoke();
        return CurTimeScale;
    }

    public void Clear()
    {
        IsPause = false;
        CurTimeScale = 1;
        Time.timeScale = CurTimeScale;
    }
}
