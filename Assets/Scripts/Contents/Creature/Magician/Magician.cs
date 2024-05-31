using Data;
using Define;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Magician : AttackableCreature
{
    private MagicianSpell _spell;
    private string _animTrigger;

    public MagicianSpell Spell { get { return _spell; } set => _spell = value; }
    private Transform ProjectileSpawnPoint { get; set; }

    public override void Init(IData data)
    {
        base.Init(data);
        BaseSpellData spelldata = data as BaseSpellData;
        Spell = Managers.Game._SpellDataBase.SpellDict[spelldata.id];
        ProjectileSpawnPoint = Util.FindChild<Transform>(gameObject, "ProjectileSpawnPoint");

        _animTrigger = spelldata.animType.ToString();

        InitAttackable(data);

        _animationController.OnAttackAnimEvent -= AttackAnimListner;
        _animationController.OnAttackAnimEvent += AttackAnimListner;

    }

    public override IEnumerator CoAttack()
    {
        PlayAnimationOnTrigger(_animTrigger);
        yield return YieldCache.WaitForSeconds(AttackDelay);
        AttackerState = AttackableState.Idle;
    }

    public void AttackAnimListner()
    {
        Spell.UseSpell(Target, ProjectileSpawnPoint);
    }

    public override void InitAttackable(IData data)
    {
        AttackDelay = Spell.SpellDelay;
        AttackRange = Spell.SpellRange;

        AttackerState = AttackableState.SearchTarget;
    }

    public override void OnUpdate()
    {
        if(Target == null) return;
        Vector3 direction = Target.Tf.position - transform.position;
        direction.y = 0;  // y축 값만 사용
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
    }

    public override bool SearchTarget()
    {
        Target = Spell.SearchTarget(transform);
        if (Target != null)
            return true;
        return false;
    }
}
