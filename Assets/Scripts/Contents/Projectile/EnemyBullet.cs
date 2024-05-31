using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : Projectile
{
    public override void InitDamageDealer(IData data)
    {
        SetEnemyData enemydata = data as SetEnemyData;
        AttackDamage = enemydata.AttackDamage;
        PierceCount = 1;
    }
}
