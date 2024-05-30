using Data;
using Define;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

// 적 객체
[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : AttackableCreature, IMoveable, IAttackable, IHitable
{
    #region Variable
    private Vector3         _destination;
    private Vector3         _direction;
    private Rigidbody       _rigid;
    private float           _moveSpeed;

    private LayerMask       _targetLayer;

    private float           _maxHp;
    private float           _curHp;
    private bool            _isDead;
    #endregion

    #region Property
    public Vector3 Destination { get => _destination; set => _destination = value; }
    public Vector3 Direction { get => _direction; set => _direction = value; }
    public Rigidbody Rigid { get => _rigid; set => _rigid = value; }
    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }

    public LayerMask TargetLayer { get => _targetLayer; set => _targetLayer = value; }

    public float MaxHp { get => _maxHp; set => _maxHp = value; }
    public float CurHp { get => _curHp; set => _curHp = value; }
    public bool IsDead { get => _isDead; set => _isDead = value; }
    public Transform Tf { get => transform; }

    private bool IsHit { get; set; } = false;
    #endregion

    public override void ChangeAttackerState()
    {
        switch (AttackerState)
        {
            case AttackableState.SearchTarget:
                StartCoroutine(CoSearchTarget());
                PlayAnimationOnBool("IsMove", true);
                break;
            case AttackableState.Attack:
                StartCoroutine(CoAttack());
                PlayAnimationOnBool("IsMove", false);
                break;
            case AttackableState.Idle:
                StartCoroutine(CoIdle());
                PlayAnimationOnBool("IsMove", false);
                break;
        }
    }

    public void InitMoveable(IData data)
    {
        BaseEnemyData enemyData = data as BaseEnemyData;
        MoveSpeed = enemyData.baseMoveSpeed;
        Rigid = GetComponent<Rigidbody>();
        Rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ
            | RigidbodyConstraints.FreezeRotationY;
    }

    public override void InitAttackable(IData data)
    {
        BaseEnemyData enemyData = data as BaseEnemyData;
        AttackDelay = enemyData.baseAttackDelay;
        AttackRange = enemyData.baseAttackRange;
        TargetLayer = 1 << LayerMask.NameToLayer("Player");
        AttackerState = AttackableState.SearchTarget;
    }
    public void InitHitable(IData data)
    {
        BaseEnemyData enemyData = data as BaseEnemyData;
        MaxHp = enemyData.baseHp;
        CurHp = MaxHp;
    }
    public override void Init(IData data)
    {
        _animationController = transform.GetOrAddComponent<AnimationController>();
        _animationController.Init();

        _animationController.OnAttackAnimEvent -= AttackAnimListner;
        _animationController.OnAttackAnimEvent += AttackAnimListner;

        _animationController.OnHitRecoverAnimEvent -= HitRecoverEventListner;
        _animationController.OnHitRecoverAnimEvent += HitRecoverEventListner;

        InitMoveable(data);
        InitHitable(data);
        InitAttackable(data);


        IsDead = false;
        IsHit = false;
        Target = Managers.Game.PlayerWall;
    }

    public void SetDir(Vector3 direction)
    {
        // Destination = destination;
        Direction = direction;
        transform.LookAt(Direction);
    }

    public void Move()
    {
        Rigid.velocity = Direction * MoveSpeed + new Vector3(0, Rigid.velocity.y, 0);
    }

    public void TakeDamage(IDamageDealer dealer)
    {
        _curHp -= dealer.AttackDamage;
        PlayAnimationOnTrigger("GetHit");
        IsHit = true;

        if(_curHp < 0 )
        {
            IsDead = true;
            Managers.Game.KillEnemy(this);
        }
    }

    public override void OnUpdate()
    {
        if (AttackerState != AttackableState.SearchTarget || IsHit)
            return;
        Move();
    }

    public override bool SearchTarget()
    {
        return Physics.Raycast(transform.position, Direction,AttackRange,TargetLayer);
    }

    public void HitRecoverEventListner()
    {
        IsHit = false;
    }

    public abstract void AttackAnimListner();
}
