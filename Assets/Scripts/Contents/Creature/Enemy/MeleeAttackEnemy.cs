using Data;
using Define;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackEnemy : Enemy, IDamageDealer
{
    private float _attackDamage;
    private Transform _hitEffectTf;
    private string _hitEffectPath;

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
        SetEnemyData enemyData = data as SetEnemyData;
        _hitEffectPath = $"Effects/Explosions/{enemyData.ElementType}SlashHit";
        if (_hitEffectTf == null)
        {
            _hitEffectTf = Util.FindChild(gameObject, "HitEffectPos").transform;
        }
    }

    public void InitDamageDealer(IData data)
    {
        SetEnemyData enemyData = data as SetEnemyData;
        AttackDamage = enemyData.AttackDamage;
    }

    public override void AttackAnimListner()
    {
        Target.TakeDamage(this);
        GameObject obj = Managers.Resource.Instantiate(_hitEffectPath, _hitEffectTf.position);
        Managers.CompCache.GetOrAddComponentCache(obj, out HitEffect hitEffect);
        hitEffect.Init();
    }
}
