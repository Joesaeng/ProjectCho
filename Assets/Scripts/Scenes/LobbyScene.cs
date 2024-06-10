using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Lobby;

        Managers.UI.ShowSceneUI<UI_LobbyScene>();

        LobbySceneManager.Instance.Init();
    }
    public override void Clear()
    {
        
    }
}
