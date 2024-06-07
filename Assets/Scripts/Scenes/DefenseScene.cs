using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseScene : BaseScene
{
    public override void Clear()
    {
        
    }

    protected override void Init()
    {
        base.Init();
        Managers.Init();

        Managers.UI.ShowSceneUI<UI_DefenseScene>();

        Managers.Pool.Init();

        Managers.Game.Init();
    }
}
