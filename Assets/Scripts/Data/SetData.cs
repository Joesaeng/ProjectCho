using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISetData : IData
{

}
public class SetEnemyData : ISetData
{
    int IData.Id => Id;
    public int Id;
    public ElementType ElementType { get; set; }

    public float Hp { get; set; }
    public float MoveSpeed { get; set; }
    public float AttackDamage { get; set; }
    public float AttackDelay { get; set; }
    public float AttackRange { get; set; }
    public bool IsRange { get; set; }
    public int ProjectileId { get; set; }

    public SetEnemyData(BaseEnemyData data)
    {
        Id = data.id;
        ElementType = data.elementType;
        Hp = data.baseHp;
        MoveSpeed = data.baseMoveSpeed;
        AttackDamage = data.baseAttackDamage;
        AttackDelay = data.baseAttackDelay;
        AttackRange = data.baseAttackRange;
        IsRange = data.isRange;
        ProjectileId = data.projectileId;
    }
}
