using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    public Animator _animator;
    public Action OnAttackAnimEvent;
    public Action OnHitRecoverAnimEvent;
    public void Init()
    {
        _animator = gameObject.GetOrAddComponent<Animator>();
    }

    public void PlayAnimationOnTrigger(string animTrigger)
    {
        _animator.SetTrigger(animTrigger);
    }

    public void PlayAnimationOnBool(string animBool, bool value)
    {
        _animator.SetBool(animBool, value);
    }

    public void AttackAnimEvent() 
    {
        OnAttackAnimEvent.Invoke();
    }

    public void HitRecoverAnimEvent()
    {
        OnHitRecoverAnimEvent.Invoke();
    }
}
