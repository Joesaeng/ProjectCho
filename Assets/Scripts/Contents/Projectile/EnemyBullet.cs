using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : Projectile
{
    public override void InitDamageDealer(IData data)
    {
        BaseEnemyData enemydata = data as BaseEnemyData;
        AttackDamage = enemydata.baseAttackDamage;
    }

    public override void InitMoveable(IData data)
    {
        ProjectileData enemyBulletData = data as ProjectileData;
        MoveSpeed = enemyBulletData.baseMoveSpeed;
    }
}
