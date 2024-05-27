using Define;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magician : Creature, IAttackable
{


    private float           _attackDelay;
    private float           _attackRange;
    private AttackableState _attackerState;
    private IHitable        _target;
    private LayerMask       _targetLayer;

    public float AttackDelay { get => _attackDelay; set => _attackDelay = value; }
    public float AttackRange { get => _attackRange; set => _attackRange = value; }
    public AttackableState AttackerState { get => _attackerState; set => _attackerState = value; }
    public IHitable Target { get => _target; set => _target = value; }
    public LayerMask TargetLayer { get => _targetLayer; set => _targetLayer = value; }

    public void Init()
    {

    }

    public void ChangeAttackerState()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator CoAttack(IHitable target)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator CoIdle()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator CoSearchTarget()
    {
        throw new System.NotImplementedException();
    }

    public void InitAttackable(IData data)
    {
        throw new System.NotImplementedException();
    }

    public override void OnUpdate()
    {
        
    }
}
