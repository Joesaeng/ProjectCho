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

    public override IEnumerator CoAttack(IHitable target)
    {
        yield return YieldCache.WaitForSeconds(AttackDelay);
        target.TakeDamage(this);
        AttackerState = AttackableState.Idle;
    }

    public override void Init(IData data)
    {
        base.Init(data);
        InitDamageDealer(data);
    }

    public void InitDamageDealer(IData data)
    {
        BaseEnemyData enemyData = data as BaseEnemyData;
        AttackDamage = enemyData.baseAttackDamage;
    }
}
