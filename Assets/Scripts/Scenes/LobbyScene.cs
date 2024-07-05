using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        // Test
#if UNITY_EDITOR
        // Managers.PlayerData.NewPlayerLogin();
#endif
        LobbySceneManager.Instance.Init();
        Managers.Sound.Play("bgm_lobby", Define.Sound.Bgm);
    }
    public override void Clear()
    {
        
    }
}
