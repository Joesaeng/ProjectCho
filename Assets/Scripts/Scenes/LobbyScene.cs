using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        LobbySceneManager.Instance.Init();
    }
    public override void Clear()
    {
        
    }
}
