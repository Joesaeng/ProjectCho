using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
