using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class MagicianSpell : ISetData
{
    protected BaseSpellData BaseSpellData { get; set; }
    int IData.id => id;
    public int id;

    #region 변동가능
    public int EffectId { get; set; }
    public float SpellDamage { get; set; }
    public float SpellDelay { get; set; }
    public float SpellRange { get; set; }
    public float SpellSpeed { get; set; }
    public float SpellDuration { get; set; }
    public float SpellSize { get; set; }
    public int PireceCount { get; set; }

    #endregion
    protected void Init(BaseSpellData data)
    {
        EffectId = data.effectId;
        SpellDamage = data.spellDamage;
        SpellDelay = data.spellDelay;
        SpellRange = data.spellRange;
        SpellSpeed = data.spellSpeed;
        SpellDuration = data.spellDuration;
        SpellSize = data.spellSize;
        PireceCount = data.pierceCount;
    }

    public abstract void UseSpell(IHitable target, Transform projectileSpawnPoint = null);

    public abstract IHitable SearchTarget(Transform transform);

    protected IHitable SearchTarget_Closest(Transform transform)
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

    protected IHitable SearchTarget_Random(Transform transform)
    {
        List<Enemy> enemies = new List<Enemy>(Managers.Game.Enemies);
        int randIndex = Random.Range(0, enemies.Count);
        if (enemies.Count == 0)
            return null;
        return enemies[randIndex];
    }
}

public class TargetedProjecttile : MagicianSpell
{
    ProjectileData ProjectileData { get; set; }
    public TargetedProjecttile(BaseSpellData data)
    {
        BaseSpellData = data;
        Init(data);
        ProjectileData = Managers.Data.ProjectileDataDict[data.effectId];
    }

    public override void UseSpell(IHitable target, Transform projectileSpawnPoint = null)
    {
        GameObject obj = Managers.Resource.Instantiate("PlayerBullet",projectileSpawnPoint.position);
        Managers.CompCache.GetOrAddComponentCache(obj, out PlayerBullet playerBullet);
        playerBullet.Init(ProjectileData);
        playerBullet.InitDamageDealer(this);
        playerBullet.InitMoveable(this);
        Vector3 dir = new Vector3(target.Tf.position.x - projectileSpawnPoint.position.x,0,
            target.Tf.position.z - projectileSpawnPoint.position.z);
        playerBullet.SetDir(dir.normalized);
    }

    public override IHitable SearchTarget(Transform transform)
    {
        return SearchTarget_Closest(transform);
    }
}

public class StraightProjectile : MagicianSpell
{
    ProjectileData ProjectileData { get; set; }
    public StraightProjectile(BaseSpellData data)
    {
        BaseSpellData = data;
        Init(data);
        ProjectileData = Managers.Data.ProjectileDataDict[data.effectId];
    }

    public override void UseSpell(IHitable target, Transform projectileSpawnPoint = null)
    {
        Vector3 pos = new Vector3(target.Tf.position.x, 0,0);
        GameObject obj = Managers.Resource.Instantiate("StarightTypePlayerBullet",pos);
        Managers.CompCache.GetOrAddComponentCache(obj, out StarightTypePlayerBullet playerBullet);
        playerBullet.Init(ProjectileData);
        playerBullet.InitDamageDealer(this);
        playerBullet.InitMoveable(this);
        Vector3 dir = Vector3.forward;
        playerBullet.SetDir(dir);
    }

    public override IHitable SearchTarget(Transform transform)
    {
        return SearchTarget_Random(transform);
    }
}

public class TargetedAOE : MagicianSpell
{
    AOEEffectData AOEEffectData { get; set; }
    public TargetedAOE(BaseSpellData data)
    {
        BaseSpellData = data;
        Init(data);
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
        return SearchTarget_Random(transform);
    }

}
