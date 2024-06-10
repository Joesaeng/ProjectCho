using Data;
using Define;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

// 적 객체
[RequireComponent(typeof(NavMeshAgent))]
public abstract class Enemy : AttackableCreature, IMoveable, IAttackable, IHitable
{
    #region Variable
    private Vector3         _destination;
    private Vector3         _direction;
    private Rigidbody       _rigid;
    private Collider        _collider;
    private float           _moveSpeed;

    private LayerMask       _targetLayer;

    private float           _maxHp;
    [SerializeField]
    private float           _curHp;
    private bool            _isDead;

    #endregion

    #region Property
    public Vector3 Destination { get => _destination; set => _destination = value; }
    public Vector3 Direction { get => _direction; set => _direction = value; }
    public Rigidbody Rigid { get => _rigid; set => _rigid = value; }
    public Collider Collider { get => _collider; set => _collider = value; }
    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }

    public LayerMask TargetLayer { get => _targetLayer; set => _targetLayer = value; }

    public float MaxHp { get => _maxHp; set => _maxHp = value; }
    public float CurHp { get => _curHp; set => _curHp = value; }
    public bool IsDead { get => _isDead; set => _isDead = value; }
    public Transform Tf { get => transform; }

    #endregion

    public System.Action OnDead;

    private NavMeshAgent Agent;
    private Transform PlayerWall;
    public ElementType ElementType { get; set; }
    public override void ChangeAttackerState()
    {
        switch (AttackerState)
        {
            case AttackableState.SearchTarget:
                StartCoroutine(CoSearchTarget());
                AnimationSetBool("IsMove", true);
                break;
            case AttackableState.Attack:
                Agent.speed = 0;
                Agent.isStopped = true;
                StartCoroutine(CoAttack());
                AnimationSetBool("IsMove", false);
                break;
            case AttackableState.Idle:
                StartCoroutine(CoIdle());
                AnimationSetBool("IsMove", false);
                break;
        }
    }

    public void InitMoveable(IData data)
    {
        SetEnemyData enemyData = data as SetEnemyData;
        MoveSpeed = enemyData.MoveSpeed;
        Agent.speed = MoveSpeed;
    }

    public override void InitAttackable(IData data)
    {
        SetEnemyData enemyData = data as SetEnemyData;
        AttackDelay = enemyData.AttackDelay;
        AttackRange = enemyData.AttackRange;
        TargetLayer = 1 << LayerMask.NameToLayer("Player");
        AttackerState = AttackableState.SearchTarget;
    }
    public void InitHitable(IData data)
    {
        SetEnemyData enemyData = data as SetEnemyData;
        ElementType = enemyData.ElementType;
        MaxHp = enemyData.Hp;
        CurHp = MaxHp;
    }
    public override void Init(IData data)
    {
        Managers.CompCache.GetOrAddComponentCache(gameObject, out Agent);
        Managers.CompCache.GetOrAddComponentCache(gameObject, out _collider);
        Managers.CompCache.GetOrAddComponentCache(gameObject, out _animationController);
        Collider.enabled = true;
        Agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        _animationController.Init();

        _animationController.OnAttackAnimEvent -= AttackAnimListner;
        _animationController.OnAttackAnimEvent += AttackAnimListner;

        _animationController.OnHitRecoverAnimEvent -= HitRecoverEventListner;
        _animationController.OnHitRecoverAnimEvent += HitRecoverEventListner;

        InitMoveable(data);
        InitHitable(data);
        InitAttackable(data);

        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        IsDead = false;
        Target = DefenseSceneManager.Instance.PlayerWall;
        PlayerWall = DefenseSceneManager.Instance.PlayerWall.transform;
        MoveToClosestPointOnWall();
    }

    void MoveToClosestPointOnWall()
    {
        // 벽의 콜라이더를 가져옵니다.
        Collider wallCollider = PlayerWall.GetComponent<Collider>();
        if (wallCollider == null)
        {
            Debug.LogError("벽에 콜라이더가 없습니다.");
            return;
        }

        // 벽의 콜라이더 경계 박스를 가져옵니다.
        Bounds wallBounds = wallCollider.bounds;

        // 벽의 콜라이더 경계 내의 가장 가까운 점을 계산합니다.
        Vector3 closestPoint = wallBounds.ClosestPoint(transform.position);

        // 네비메쉬 샘플링을 통해 유효한 위치를 찾습니다.
        NavMeshHit hit;
        if (NavMesh.SamplePosition(closestPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            // 유효한 위치가 발견되면 네비메쉬 에이전트의 목표 위치로 설정합니다.
            Agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogError("네비메쉬 상에서 유효한 목표 위치를 찾을 수 없습니다.");
        }
    }

    public void TakeDamage(IDamageDealer dealer)
    {
        float damage = ElementalDamageCalculator.CalculateDamage(dealer.ElementType, ElementType, dealer.AttackDamage);
        _curHp -= damage;
        GameObject damageTextObj = Managers.Resource.Instantiate("DamageText", transform.position + Vector3.up * 2);
        Managers.CompCache.GetOrAddComponentCache(damageTextObj, out DamageText damageText);
        damageText.Init(Mathf.RoundToInt(damage));

        Agent.isStopped = true;

        if (_curHp < 0)
        {
            Die();
        }
        else
            PlayAnimationOnTrigger("GetHit");
    }

    public override bool SearchTarget()
    {
        return Physics.Raycast(transform.position, transform.forward, AttackRange, TargetLayer);
    }

    public void HitRecoverEventListner()
    {
        if (IsDead)
            return;
        Agent.isStopped = false;
    }

    void Die()
    {
        // 코루틴 스테이트머신 정지
        StopAllCoroutines();
        // 사망 코루틴 실행
        StartCoroutine(CoDie());

        Collider.enabled = false;
        PlayAnimationOnTrigger("Dead");
        IsDead = true;
        OnDead?.Invoke();
        OnDead = null;
    }

    IEnumerator CoDie()
    {
        yield return YieldCache.WaitForSeconds(1.5f);
        DefenseSceneManager.Instance.KillEnemy(this);
    }

    public abstract void AttackAnimListner();
}
