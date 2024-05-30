using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class MagicianSpell
{
    protected BaseSpellData BaseSpellData { get; set; }

    public abstract void UseSpell(IHitable target, Transform projectileSpawnPoint = null);

    public virtual IHitable SearchTarget(Transform transform)
    {
        IHitable target = null;
        bool searched = false;
        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Enemy enemy in Managers.Game.Enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > BaseSpellData.spellRange)
                continue;

            if (closestEnemy == null || distance < closestDistance)
            {
                closestEnemy = enemy;
                closestDistance = distance;
                searched = true;
            }
        }

        if (searched)
        {
            target = closestEnemy;
        }

        return target;
    }
}

public class TargetedProjecttile : MagicianSpell
{
    ProjectileData ProjectileData { get; set; }
    public TargetedProjecttile(BaseSpellData data)
    {
        BaseSpellData = data;
        ProjectileData = Managers.Data.ProjectileDataDict[data.effectId];
    }

    public override void UseSpell(IHitable target, Transform projectileSpawnPoint = null)
    {
        GameObject obj = Managers.Resource.Instantiate("PlayerBullet",projectileSpawnPoint.position);
        Managers.CompCache.GetOrAddComponentCache(obj, out PlayerBullet playerBullet);
        playerBullet.Init(ProjectileData);
        playerBullet.InitDamageDealer(BaseSpellData);
        playerBullet.InitMoveable(BaseSpellData);
        Vector3 dir = new Vector3(target.Tf.position.x - projectileSpawnPoint.position.x,0,
            target.Tf.position.z - projectileSpawnPoint.position.z);
        playerBullet.SetDir(dir.normalized);
    }
}

public class StraightProjectile : MagicianSpell
{
    ProjectileData ProjectileData { get; set; }
    public StraightProjectile(BaseSpellData data)
    {
        BaseSpellData = data;
        ProjectileData = Managers.Data.ProjectileDataDict[data.effectId];
    }

    public override void UseSpell(IHitable target, Transform projectileSpawnPoint = null)
    {
        Vector3 pos = new Vector3(target.Tf.position.x, 0,0);
        GameObject obj = Managers.Resource.Instantiate("StarightTypePlayerBullet",pos);
        Managers.CompCache.GetOrAddComponentCache(obj, out StarightTypePlayerBullet playerBullet);
        playerBullet.Init(ProjectileData);
        playerBullet.InitDamageDealer(BaseSpellData);
        playerBullet.InitMoveable(BaseSpellData);
        Vector3 dir = Vector3.forward;
        playerBullet.SetDir(dir);
    }

    public override IHitable SearchTarget(Transform transform)
    {
        List<Enemy> enemies = new List<Enemy>(Managers.Game.Enemies);
        int randIndex = Random.Range(0, enemies.Count);
        if(enemies.Count == 0)
            return null;
        return enemies[randIndex];
    }
}

public class TargetedAOE : MagicianSpell
{
    AOEEffectData AOEEffectData { get; set; }
    public TargetedAOE(BaseSpellData data)
    {
        BaseSpellData = data;
        AOEEffectData = Managers.Data.AOEEffectDataDict[data.effectId];
    }

    public override void UseSpell(IHitable target, Transform projectileSpawnPoint = null)
    {
        Vector3 pos = new Vector3(target.Tf.position.x, 0,target.Tf.position.z);
        Vector3 rot = new Vector3(-90,0,0);
        GameObject obj = Managers.Resource.Instantiate("AOETypePlayerSpell",pos);
        obj.transform.rotation = Quaternion.Euler(rot);
        Managers.CompCache.GetOrAddComponentCache(obj, out AOETypePlayerSpell playerBullet);
        playerBullet.Init(AOEEffectData);
        playerBullet.InitDamageDealer(BaseSpellData);
    }

    public override IHitable SearchTarget(Transform transform)
    {
        List<Enemy> enemies = new List<Enemy>(Managers.Game.Enemies);
        int randIndex = Random.Range(0, enemies.Count);
        if (enemies.Count == 0)
            return null;
        return enemies[randIndex];
    }

}
