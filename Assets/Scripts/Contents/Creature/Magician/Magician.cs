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
    private string _animName;
    private Vector3 _targetPos;

    public MagicianSpell Spell { get { return _spell; } set => _spell = value; }
    private Transform ProjectileSpawnPoint { get; set; }

    public override void Init(IData data)
    {
        base.Init(data);
        BaseSpellData spelldata = data as BaseSpellData;
        Spell = Managers.Game._SpellDataBase.SpellDict[spelldata.id];
        Spell.OnUpdateSpellDelay += UpdateSpellDelay;
        Spell.OwnMagicianTransform = transform;
        ProjectileSpawnPoint = Util.FindChild<Transform>(gameObject, "ProjectileSpawnPoint");

        _animName = spelldata.animType.ToString();

        InitAttackable(data);

        _animationController.OnAttackAnimEvent -= AttackAnimListner;
        _animationController.OnAttackAnimEvent += AttackAnimListner;

    }

    public override IEnumerator CoAttack()
    {
        PlayAnimationOnTrigger(_animName);
        yield return YieldCache.WaitForSeconds(AttackDelay);
        AttackerState = AttackableState.Idle;
    }

    public void AttackAnimListner()
    {
        Spell.UseSpell(_targetPos, ProjectileSpawnPoint);
    }

    public override void InitAttackable(IData data)
    {
        UpdateSpellDelay();
        AttackRange = Spell.SpellRange;

        AttackerState = AttackableState.SearchTarget;
    }

    public override void OnUpdate()
    {
        if(Target == null) return;
        _targetPos = Target.Tf.position;
        Vector3 direction = Target.Tf.position - transform.position;
        direction.y = 0;  // y축 값만 사용
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
    }

    public override bool SearchTarget()
    {
        Target = Spell.SearchTarget(transform);
        if (Target != null)
        {
            Enemy enemy = Target as Enemy;
            _targetPos = Target.Tf.position;
            enemy.OnDead += OnDeadListner;
            return true;
        }
        return false;
    }

    private void UpdateSpellDelay()
    {
        AttackDelay = Spell.SpellDelay;
        _animationController.SetAttackSpeed("Magician" + _animName,AttackDelay);
    }

    private void OnDeadListner()
    {
        Target = null;
    }
}
