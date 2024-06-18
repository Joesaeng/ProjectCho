using Data;
using Define;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCharge : AttackableCreature, ISpellUseable
{
    private MagicianSpell _spell;
    private Vector3 _targetPos;

    string _enchantPath;

    public MagicianSpell Spell { get { return _spell; } set => _spell = value; }

    public override void Init(IData data)
    {
        InitSpellUseable(data);

        InitAttackable(data);
        Managers.Resource.Instantiate($"Charge/{Spell.ElementType}Charge",transform);
    }

    public void InitSpellUseable(IData data)
    {
        BaseSpellData spelldata = data as BaseSpellData;
        Spell = DefenseSceneManager.Instance.PlayerSpells.SpellDict[spelldata.id];
        Spell.OnUpdateSpellDelay += UpdateSpellDelay;
        Spell.OwnTransform = transform;
        _enchantPath = $"Effects/ChargeUseSpell/{_spell.ElementType}Enchant";
    }

    public override void InitAttackable(IData data)
    {
        UpdateSpellDelay();
        AttackRange = Spell.SpellRange;

        AttackerState = AttackableState.SearchTarget;
    }

    public override IEnumerator CoAttack()
    {
        GameObject enchant = Managers.Resource.Instantiate($"{_enchantPath}", transform);
        enchant.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
        Managers.CompCache.GetOrAddComponentCache(enchant, out HitEffect effect);
        effect.Init();
        Spell.UseSpell(_targetPos, transform);
        yield return YieldCache.WaitForSeconds(AttackDelay);
        AttackerState = AttackableState.Idle;
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
    }

    private void OnDeadListner()
    {
        Target = null;
    }

    public void OnUpdate()
    {
        if (Target == null)
            return;
        _targetPos = Target.Tf.position;
    }

}
