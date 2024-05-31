using Data;
using Define;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackEnemy : Enemy, IDamageDealer
{
    private float _attackDamage;

    public float AttackDamage { get => _attackDamage; set => _attackDamage = value; }

    public override IEnumerator CoAttack()
    {
        PlayAnimationOnTrigger("Attack");
        yield return YieldCache.WaitForSeconds(AttackDelay);
        AttackerState = AttackableState.Idle;
    }

    public override void Init(IData data)
    {
        base.Init(data);
        InitDamageDealer(data);
    }

    public void InitDamageDealer(IData data)
    {
        SetEnemyData enemyData = data as SetEnemyData;
        AttackDamage = enemyData.AttackDamage;
    }

    public override void AttackAnimListner()
    {
        Target.TakeDamage(this);
    }
}
