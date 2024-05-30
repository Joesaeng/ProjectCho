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
    private Transform ProjectileSpawnPoint { get; set; }

    public override void Init(IData data)
    {
        base.Init(data);
        EnemyData = data as BaseEnemyData;
        ProjectileData = Managers.Data.ProjectileDataDict[EnemyData.projectileId];
        ProjectileSpawnPoint = Util.FindChild<Transform>(gameObject, "ProjectileSpawnPoint");
    }
    public override IEnumerator CoAttack()
    {
        PlayAnimationOnTrigger("Attack");

        yield return YieldCache.WaitForSeconds(AttackDelay);
        AttackerState = AttackableState.Idle;
    }
    public override void AttackAnimListner()
    {
        GameObject obj = Managers.Resource.Instantiate("EnemyBullet",ProjectileSpawnPoint.position);
        Managers.CompCache.GetOrAddComponentCache(obj, out EnemyBullet enemyBullet);
        enemyBullet.Init(ProjectileData);
        enemyBullet.InitDamageDealer(EnemyData);
        enemyBullet.InitMoveable(ProjectileData);
        enemyBullet.SetDir(Direction);
    }
}
