using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbySceneManager : MonoBehaviour
{
    private static LobbySceneManager instance;
    public static LobbySceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (LobbySceneManager)FindObjectOfType(typeof(LobbySceneManager));
                if (instance == null)
                {
                    GameObject obj = new(typeof(LobbySceneManager).Name, typeof(LobbySceneManager));
                    instance = obj.GetComponent<LobbySceneManager>();
                }
            }
            return instance;
        }
    }

    UI_LobbyScene UI;
    public void Init()
    {
        UI = Managers.UI.ShowSceneUI<UI_LobbyScene>();
        UI.Init();
    }

    public void PlayGame(int stageNum)
    {
        Managers.Game.SelectedStage = stageNum;
        Managers.Scene.LoadSceneWithLoadingScene(Define.Scene.Defense);
    }
}
