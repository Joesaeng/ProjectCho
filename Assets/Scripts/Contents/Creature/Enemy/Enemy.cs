using Data;
using Define;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 적 객체
[RequireComponent(typeof(Rigidbody))]
public class Enemy : Creature, IMoveable, IAttackable, IHitable
{
    #region Variable
    private Vector3         _destination;
    private Vector3         _direction;
    private Rigidbody       _rigid;
    private float           _moveSpeed;

    private float           _attackDelay;
    private float           _attackRange;
    private AttackableState _attackerState;
    private IHitable        _target;
    private LayerMask       _targetLayer;

    private float           _maxHp;
    private float           _curHp;
    #endregion

    #region Property
    public Vector3 Destination { get => _destination; set => _destination = value; }
    public Vector3 Direction { get => _direction; set => _direction = value; }
    public Rigidbody Rigid { get => _rigid; set => _rigid = value; }
    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }

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
    public LayerMask TargetLayer { get => _targetLayer; set => _targetLayer = value; }

    public float MaxHp { get => _maxHp; set => _maxHp = value; }
    public float CurHp { get => _curHp; set => _curHp = value; }
    #endregion

    public void InitMoveable(IData data)
    {
        BaseEnemyData enemyData = data as BaseEnemyData;
        MoveSpeed = enemyData.baseMoveSpeed;
        Rigid = GetComponent<Rigidbody>();
        Rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ
            | RigidbodyConstraints.FreezeRotationY;
    }

    public void InitAttackable(IData data)
    {
        BaseEnemyData enemyData = data as BaseEnemyData;
        AttackDelay = enemyData.baseAttackDelay;
        AttackRange = enemyData.baseAttackRange;
        TargetLayer = 1 << LayerMask.NameToLayer("EnemyTarget");
    }
    public void InitHitable(IData data)
    {
        BaseEnemyData enemyData = data as BaseEnemyData;
        MaxHp = enemyData.baseHp;
        CurHp = MaxHp;
    }
    public override void Init(IData data)
    {
        base.Init(data);
        InitMoveable(data);
        InitAttackable(data);
        InitHitable(data);

        AttackerState = AttackableState.SearchTarget;
    }

    public void SetDir(Vector3 destination)
    {
        Destination = destination;
        Direction = (Destination - transform.position).normalized;
        transform.LookAt(Direction);
    }

    public void Move()
    {
        Rigid.velocity = MoveSpeed * Direction;
    }

    public void TakeDamage(IDamageDealer dealer)
    {

    }

    public override void OnUpdate()
    {
        if (AttackerState != AttackableState.SearchTarget)
            return;
        Move();
    }

    public void ChangeAttackerState()
    {
        switch (AttackerState)
        {
            case AttackableState.SearchTarget:
                StartCoroutine(CoSearchTarget());
                break;
            case AttackableState.Attack:
                StartCoroutine(CoAttack(Target));
                break;
            case AttackableState.Idle:
                StartCoroutine(CoIdle());
                break;
        }
    }

    public IEnumerator CoSearchTarget()
    {
        yield return YieldCache.WaitForSeconds(0.1f);
        if (SearchTarget(out IHitable hitable))
        {
            Target = hitable;
            AttackerState = AttackableState.Idle;
        }
        else
            AttackerState = AttackableState.SearchTarget;
    }
    private bool SearchTarget(out IHitable hitable)
    {
        hitable = null;
        return Physics.Raycast(transform.position, Direction, out RaycastHit hit, AttackRange,TargetLayer) &&
            hit.transform.TryGetComponent<IHitable>(out hitable);
    }

    public virtual IEnumerator CoAttack(IHitable target)
    {
        // 공격 실행
        Debug.Log("적의 공격!");
        yield return YieldCache.WaitForSeconds(AttackDelay);
        AttackerState = AttackableState.Idle;
    }

    public IEnumerator CoIdle()
    {
        yield return null;
        if (SearchTarget(out IHitable hitable))
            AttackerState = AttackableState.Attack;
        else
            AttackerState = AttackableState.SearchTarget;
    }
}
