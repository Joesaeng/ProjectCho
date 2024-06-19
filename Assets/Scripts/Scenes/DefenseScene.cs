using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseScene : BaseScene
{
    public override void Clear()
    {
        // Managers.Game.Clear();
        DefenseSceneManager.Instance.Clear();
        Managers.CompCache.Clear();
    }

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Defense;

        Managers.Init();

        Managers.Pool.Init();
        DefenseSceneManager.Instance.Init();
        // Managers.Game.Init();
    }
}
