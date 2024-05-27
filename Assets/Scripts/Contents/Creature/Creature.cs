using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 애니메이션을 사용할 모든 객체
[RequireComponent(typeof(AnimationController))]
public abstract class Creature : MonoBehaviour
{
    protected AnimationController _animationController;
    public virtual void Init(IData data)
    {
        _animationController = GetComponentInChildren<AnimationController>();
        _animationController.Init();
    }

    public abstract void OnUpdate();
}
