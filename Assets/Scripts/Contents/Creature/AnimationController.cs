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
    public void Init()
    {
        _animator = GetComponent<Animator>();
    }

    public void PlayAnimation(string name)
    {

    }

    public void AttackAnimEvent() 
    {
        OnAttackAnimEvent.Invoke();
    }
}
