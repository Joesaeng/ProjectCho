using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : Creature ,IMoveable , IAttackable, IHitable
{
    // 변수
    private Vector3 _destination;
    private Vector3 _direction;
    private NavMeshAgent _agent;

    private float _attackDamage;
    private float _attackDelay;
    private float _attackRange;
    private float _maxHp;
    private float _curHp;

    
    // 프로퍼티
    public Vector3 Destination { get => _destination; set => _destination = value; }
    public Vector3 Direction { get => _direction; set => _direction = value; }
    public NavMeshAgent Agent { get => _agent; set => _agent = value; }
    public float AttackDamage { get => _attackDamage; set => _attackDamage = value; }
    public float AttackDelay { get => _attackDelay; set => _attackDelay = value; }
    public float AttackRange { get => _attackRange; set => _attackRange = value; }
    public float MapHp { get => _maxHp; set => _maxHp = value; }
    public float CurHp { get => _curHp; set => _curHp = value; }



    public void InitMoveable(IData data)
    {
        BaseEnemyData enemyData = data as BaseEnemyData;
        Agent = GetComponent<NavMeshAgent>();
        Agent.radius = 0.2f;
        Agent.speed = enemyData.baseMoveSpeed;
        Agent.autoBraking = false;
    }

    public void InitAttackable(IData data)
    {
        BaseEnemyData enemyData = data as BaseEnemyData;
        AttackDamage = enemyData.baseAttackDamage;
        AttackDelay = enemyData.baseAttackDelay;
        AttackRange = enemyData.baseAttackRange;
    }
    public void InitHitable(IData data)
    {
        BaseEnemyData enemyData = data as BaseEnemyData;
        MapHp = enemyData.baseHp;
    }
    public override void Init(IData data)
    {
        base.Init(data);
        InitMoveable(data);
        InitAttackable(data);
        InitHitable(data);
    }

    public void SetDestination(Vector3 destination)
    {
        Destination = destination;
        Move();
    }

    public void Move()
    {
        Agent.SetDestination(Destination);
    }

    public void Attack(IHitable target)
    {

    }

    public void TakeDamage()
    {
        
    }

    public override void OnUpdate()
    {
        
    }
}
