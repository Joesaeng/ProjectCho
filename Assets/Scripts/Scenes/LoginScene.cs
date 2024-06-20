using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        UI_LoginScene uI_LoginScene = FindObjectOfType<UI_LoginScene>();
        uI_LoginScene.Init();

        FirebaseManager.Instance.Init();
    }
    public override void Clear()
    {
        
    }
}
