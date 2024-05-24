using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Interfaces
{
    public interface IMoveable
    {
        Vector3 Destination { get; set; }
        Vector3 Direction { get; set; }
        NavMeshAgent Agent { get; set; }
        void InitMoveable(IData data);
        void Move();
    }

    public interface IAttackable
    {
        float AttackDamage { get; set; }
        float AttackDelay { get; set; }
        float AttackRange { get; set; }
        void InitAttackable(IData data);
        void Attack(IHitable target);
    }

    public interface IHitable
    {
        float MapHp { get; set; }
        float CurHp { get; set; }
        void InitHitable(IData data);
        void TakeDamage();
    }
}
