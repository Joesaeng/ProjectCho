using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class DefenseScene : BaseScene
{
    public override void Clear()
    {
        
    }

    protected override void Init()
    {
        base.Init();

        // TEST
        // Managers.PlayerData.NewPlayerLogin();
        
        Managers.Pool.Init();
        DefenseSceneManager.Instance.Init();
        Managers.Sound.Play("bgm_defense", Define.Sound.Bgm);
    }
}
