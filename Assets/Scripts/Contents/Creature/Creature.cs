using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


// 애니메이션을 사용할 모든 객체
public abstract class Creature : MonoBehaviour
{
    protected AnimationController _animationController;

    public virtual void Init(IData data)
    {
        _animationController = transform.GetChild(0).GetOrAddComponent<AnimationController>();
        _animationController.Init();
    }

    protected void PlayAnimationOnTrigger(string animTrigger)
    {
        _animationController.PlayAnimationOnTrigger(animTrigger);
    }

    protected void AnimationSetBool(string animBool, bool value)
    {
        _animationController.AnimationSetBool(animBool, value);
    }
}
