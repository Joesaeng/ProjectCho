using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
#if UNITY_EDITOR
        Managers.PlayerData.NewPlayerLogin();
#endif
        LobbySceneManager.Instance.Init();
    }
    public override void Clear()
    {
        
    }
}
