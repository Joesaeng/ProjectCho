using Data;
using Define;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeAttackEnemy : Enemy
{
    ProjectileData ProjectileData { get; set; }
    BaseEnemyData EnemyData { get; set; }

    public override void Init(IData data)
    {
        base.Init(data);
        EnemyData = data as BaseEnemyData;
        ProjectileData = Managers.Data.ProjectileDataDict[EnemyData.projectileId];
    }
    public override IEnumerator CoAttack()
    {
        yield return YieldCache.WaitForSeconds(AttackDelay);
        // 투사체 생성
        GameObject obj = Managers.Resource.Instantiate("EnemyBullet",transform.position);
        Managers.CompCache.GetOrAddComponentCache(obj, out EnemyBullet projectile);
        projectile.Init(ProjectileData);
        projectile.InitDamageDealer(EnemyData);
        projectile.InitMoveable(ProjectileData);
        projectile.SetDir(Destination);

        AttackerState = AttackableState.Idle;
    }
}
