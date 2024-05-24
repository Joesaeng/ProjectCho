using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    Animator _animator;
    public void Init()
    {
        _animator = GetComponent<Animator>();
    }
}
