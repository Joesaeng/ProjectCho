using Data;
using Define;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;

public abstract class AttackableCreature : Creature, IAttackable
{
    private float           _attackDelay;
    private float           _attackRange;
    private AttackableState _attackerState;
    private IHitable        _target;

    public float AttackDelay { get => _attackDelay; set => _attackDelay = value; }
    public float AttackRange { get => _attackRange; set => _attackRange = value; }
    public AttackableState AttackerState
    {
        get => _attackerState;
        set
        {
            _attackerState = value;
            ChangeAttackerState();
        }
    }
    public IHitable Target { get => _target; set => _target = value; }
    
    public abstract void InitAttackable(IData data);

    public virtual void ChangeAttackerState()
    {
        
        switch (AttackerState)
        {
            case AttackableState.SearchTarget:
                StartCoroutine(CoSearchTarget());
                break;
            case AttackableState.Attack:
                StartCoroutine(CoAttack());
                break;
            case AttackableState.Idle:
                StartCoroutine(CoIdle());
                break;
        }
    }

    public abstract IEnumerator CoAttack();

    public IEnumerator CoIdle()
    {
        yield return null;
        // if (Target.IsDead == false && Vector3.Distance(Target.Tf.position,transform.position) < AttackRange)
        if(Target != null)
            AttackerState = AttackableState.Attack;
        else
            AttackerState = AttackableState.SearchTarget;
    }

    public IEnumerator CoSearchTarget()
    {
        yield return null;
        if (SearchTarget())
        {
            AttackerState = AttackableState.Attack;
        }
        else
            AttackerState = AttackableState.SearchTarget;
    }

    public abstract bool SearchTarget();
}
