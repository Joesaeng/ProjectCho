using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : Projectile
{
    public override void InitDamageDealer(IData data)
    {
        BaseEnemyData enemydata = data as BaseEnemyData;
        AttackDamage = enemydata.baseAttackDamage;
        PierceCount = 1;
    }
}
