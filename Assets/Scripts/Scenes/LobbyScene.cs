using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        // Test
        // Managers.PlayerData.NewPlayerLogin();
        LobbySceneManager.Instance.Init();
    }
    public override void Clear()
    {
        
    }
}
