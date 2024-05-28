using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Define;
using UnityEngine.AI;

namespace Interfaces
{
    public interface IMoveable
    {
        Vector3 Destination { get; set; }
        Vector3 Direction { get; set; }
        Rigidbody Rigid { get; set; }
        float MoveSpeed { get; set; }
        void InitMoveable(IData data);
        void Move();
    }

    public interface IAttackable
    {
        float AttackDelay { get; set; }
        float AttackRange { get; set; }
        AttackableState AttackerState { get; set; }
        IHitable Target {  get; set; }
        LayerMask TargetLayer { get; set; }

        void InitAttackable(IData data);
        void ChangeAttackerState();
        IEnumerator CoAttack();
        IEnumerator CoSearchTarget();
        IEnumerator CoIdle();
        bool SearchTarget();
    }

    public interface IHitable
    {
        float MaxHp { get; set; }
        float CurHp { get; set; }
        Transform Tf { get;}
        void InitHitable(IData data);
        void TakeDamage(IDamageDealer dealer);
    }

    public interface IDamageDealer
    {
        float AttackDamage { get; set; }

        void InitDamageDealer(IData data);
    }
}
