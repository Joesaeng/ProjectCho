using Data;
using Define;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magician : AttackableCreature
{
    private MagicianSpell _spell;
    private string _animName;

    //private float           _attackDelay;
    //private float           _attackRange;
    //private AttackableState _attackerState;
    //private IHitable        _target;
    //private LayerMask       _targetLayer;

    public MagicianSpell Spell { get { return _spell; } set => _spell = value; }

    //public float AttackDelay { get => _attackDelay; set => _attackDelay = value; }
    //public float AttackRange { get => _attackRange; set => _attackRange = value; }
    //public AttackableState AttackerState { get => _attackerState; set => _attackerState = value; }
    //public IHitable Target { get => _target; set => _target = value; }
    //public LayerMask TargetLayer { get => _targetLayer; set => _targetLayer = value; }

    public override void Init(IData data)
    {
        base.Init(data);
        BaseSpellData spelldata = data as BaseSpellData;
        _animName = spelldata.animType.ToString();

        InitAttackable(data);

        _animationController.OnAttackAnimEvent -= AttackAnimListner;
        _animationController.OnAttackAnimEvent += AttackAnimListner;

        Spell.Init(spelldata);
    }

    //public void ChangeAttackerState()
    //{
    //    switch (AttackerState)
    //    {
    //        case AttackableState.SearchTarget:
    //            StartCoroutine(CoSearchTarget());
    //            break;
    //        case AttackableState.Attack:
    //            StartCoroutine(CoAttack(Target));
    //            break;
    //        case AttackableState.Idle:
    //            StartCoroutine(CoIdle());
    //            break;
    //    }
    //}

    public override IEnumerator CoAttack()
    {
        yield return YieldCache.WaitForSeconds(AttackDelay);
        PlayAnimation(_animName);
        AttackerState = AttackableState.Idle;
    }

    public void AttackAnimListner()
    {
        Spell.UseSpell(Target);
    }

    //public IEnumerator CoIdle()
    //{
    //    yield return null;
    //    if (SearchTarget(out IHitable hitable))
    //        AttackerState = AttackableState.Attack;
    //    else
    //        AttackerState = AttackableState.SearchTarget;
    //}

    //public IEnumerator CoSearchTarget()
    //{
    //    yield return YieldCache.WaitForSeconds(0.1f);
    //    if (SearchTarget(out IHitable hitable))
    //    {
    //        Target = hitable;
    //        AttackerState = AttackableState.Idle;
    //    }
    //    else
    //        AttackerState = AttackableState.SearchTarget;
    //}

    //public bool SearchTarget(out IHitable hitable)
    //{
    //    hitable = null;
    //    // 타겟 서칭
    //    throw new System.NotImplementedException();
    //}

    public override void InitAttackable(IData data)
    {
        BaseSpellData spellData = data as BaseSpellData;
        AttackDelay = spellData.spellDelay;
        AttackRange = spellData.spellRange;
        TargetLayer = 1 << LayerMask.NameToLayer("Enemy");
    }

    public override void OnUpdate()
    {

    }

    public override bool SearchTarget()
    {
        bool searched = false;
        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Enemy enemy in Managers.Game.Enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (closestEnemy == null || distance < closestDistance)
            {
                closestEnemy = enemy;
                closestDistance = distance;
                searched = true;
            }
        }

        if (searched)
        {
            Target = closestEnemy;
        }

        return searched;
    }
}
