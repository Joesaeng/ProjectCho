using Data;
using Interfaces;
using MagicianSpellUpgrade;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.Events;

public abstract class MagicianSpell : ISetData
{
    protected BaseSpellData BaseSpellData { get; set; }
    int IData.Id => id;
    public int id;

    public System.Action OnUpdateSpellDelay;
    public System.Action<Transform> OnAddProjectile;

    private Transform _ownMagicianTransform;

    public Transform OwnMagicianTransform { get => _ownMagicianTransform; set => _ownMagicianTransform = value; }

    public int EffectId { get; set; }
    public ElementType ElementType { get; set; }
    public float SpellDamage { get; set; }
    public float SpellDelay { get; set; }
    public float SpellRange { get; set; }
    public float SpellSpeed { get; set; }
    public float SpellDuration { get; set; }
    public float SpellSize { get; set; }
    public float BaseSpellSize { get; set; }
    public int PireceCount { get; set; }
    public int AddProjectileCount { get; set; }

    public List<ISpellUpgrade> Upgrades { get; set; } = new();

    protected void Init(BaseSpellData data)
    {
        id = data.id;
        EffectId = data.effectId;
        ElementType = data.elementType;
        SpellDamage = data.spellDamage;
        SpellDelay = data.spellDelay;
        SpellRange = data.spellRange;
        SpellSpeed = data.spellSpeed;
        SpellDuration = data.spellDuration;
        SpellSize = data.spellSize;
        BaseSpellSize = data.spellSize;
        PireceCount = data.pierceCount;
        AddProjectileCount = 0;
    }

    public void AddUpgrade(ISpellUpgrade upgrade)
    {
        Upgrades.Add(upgrade);
        upgrade.ApplyUpgrade(this);
    }
    public abstract void UseSpell(Vector3 targetPos, Transform projectileSpawnPoint = null);

    public abstract void UseSpellOfUpgrade(Vector3 targetPos, Transform projectileSpawnPoint = null);

    public abstract IHitable SearchTarget(Transform transform);

    public IHitable SearchTarget_Closest(Transform transform)
    {
        IHitable target = null;
        bool searched = false;
        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Enemy enemy in Managers.Game.Enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > SpellRange)
                continue;

            if (distance < closestDistance)
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

    public IHitable SearchTarget_Random()
    {
        List<Enemy> enemies = new ();

        foreach (Enemy enemy in Managers.Game.Enemies)
        {
            float distance = Vector3.Distance(_ownMagicianTransform.position, enemy.transform.position);
            if (distance > SpellRange)
                continue;

            enemies.Add(enemy);
        }

        if (enemies.Count == 0)
            return null;

        int randIndex = Random.Range(0, enemies.Count);
        return enemies[randIndex];
    }
}

public class TargetedProjecttile : MagicianSpell
{
    ProjectileData ProjectileData { get; set; }

    public ImpactUpgrade Impact;

    public TargetedProjecttile(BaseSpellData data)
    {
        BaseSpellData = data;
        Init(data);
        ProjectileData = Managers.Data.ProjectileDataDict[data.effectId];
    }

    public override void UseSpell(Vector3 targetPos, Transform projectileSpawnPoint = null)
    {
        Vector3 dir = new Vector3(targetPos.x - projectileSpawnPoint.position.x,0,
            targetPos.z - projectileSpawnPoint.position.z).normalized;

        PlayerBullet playerBullet = ShotProjectile(dir,projectileSpawnPoint);

        if (Impact != null)
        {
            playerBullet.OnImpact += Impact.ApplyImpact;
        }

        OnAddProjectile?.Invoke(projectileSpawnPoint);
    }

    public override void UseSpellOfUpgrade(Vector3 targetPos, Transform projectileSpawnPoint = null)
    {
        Vector3 dir = new Vector3(targetPos.x - projectileSpawnPoint.position.x,0,
            targetPos.z - projectileSpawnPoint.position.z).normalized;

        PlayerBullet playerBullet = ShotProjectile(dir,projectileSpawnPoint);

        if (Impact != null)
        {
            playerBullet.OnImpact += Impact.ApplyImpact;
        }
    }

    public PlayerBullet ShotProjectile(Vector3 dir, Transform projectileSpawnPoint = null)
    {
        GameObject obj = Managers.Resource.Instantiate("PlayerBullet",projectileSpawnPoint.position);
        Managers.CompCache.GetOrAddComponentCache(obj, out PlayerBullet playerBullet);
        obj.transform.localScale = Vector3.one * SpellSize;
        playerBullet.Init(ProjectileData);
        playerBullet.InitDamageDealer(this);
        playerBullet.InitMoveable(this);

        playerBullet.SetDir(dir);
        return playerBullet;
    }

    public override IHitable SearchTarget(Transform transform)
    {
        // Debug.Log("SearchTarget");
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

    public override void UseSpell(Vector3 targetPos, Transform projectileSpawnPoint = null)
    {
        Vector3 pos = new Vector3(targetPos.x, 0,0);
        GameObject obj = Managers.Resource.Instantiate("StarightTypePlayerBullet",pos);
        Managers.CompCache.GetOrAddComponentCache(obj, out StarightTypePlayerBullet playerBullet);
        playerBullet.Init(ProjectileData);
        playerBullet.InitDamageDealer(this);
        playerBullet.InitMoveable(this);
        Vector3 dir = Vector3.forward;
        playerBullet.SetDir(dir);

        float newSize = this.SpellSize;
        SetParticleSystemSize(obj, newSize);

        OnAddProjectile?.Invoke(projectileSpawnPoint);
    }

    public override void UseSpellOfUpgrade(Vector3 targetPos, Transform projectileSpawnPoint = null)
    {
        Vector3 pos = new Vector3(targetPos.x, 0,0);
        GameObject obj = Managers.Resource.Instantiate("StarightTypePlayerBullet",pos);
        Managers.CompCache.GetOrAddComponentCache(obj, out StarightTypePlayerBullet playerBullet);
        playerBullet.Init(ProjectileData);
        playerBullet.InitDamageDealer(this);
        playerBullet.InitMoveable(this);
        Vector3 dir = Vector3.forward;
        playerBullet.SetDir(dir);

        float newSize = this.SpellSize;
        SetParticleSystemSize(obj, newSize);
    }

    public override IHitable SearchTarget(Transform transform)
    {
        return SearchTarget_Random();
    }

    private void SetParticleSystemSize(GameObject obj, float newSize)
    {
        var particleSystems = obj.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            var shapeModule = ps.shape;
            shapeModule.radius = newSize;
        }
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

    public override void UseSpell(Vector3 targetPos, Transform projectileSpawnPoint = null)
    {
        Vector3 pos = new Vector3(targetPos.x, 0,targetPos.z);
        Vector3 rot = new Vector3(-90,0,0);
        GameObject obj = Managers.Resource.Instantiate("AOETypePlayerSpell",pos);
        obj.transform.rotation = Quaternion.Euler(rot);
        obj.transform.localScale = Vector3.one * SpellSize;
        Managers.CompCache.GetOrAddComponentCache(obj, out AOETypePlayerSpell spell);
        spell.Init(AOEEffectData);
        spell.InitDamageDealer(this);

        OnAddProjectile?.Invoke(projectileSpawnPoint);
    }

    public override void UseSpellOfUpgrade(Vector3 targetPos, Transform projectileSpawnPoint = null)
    {
        Vector3 pos = new Vector3(targetPos.x, 0,targetPos.z);
        Vector3 rot = new Vector3(-90,0,0);
        GameObject obj = Managers.Resource.Instantiate("AOETypePlayerSpell",pos);
        obj.transform.rotation = Quaternion.Euler(rot);
        obj.transform.localScale = Vector3.one * SpellSize;
        Managers.CompCache.GetOrAddComponentCache(obj, out AOETypePlayerSpell spell);
        spell.Init(AOEEffectData);
        spell.InitDamageDealer(this);
    }

    public override IHitable SearchTarget(Transform transform)
    {
        return SearchTarget_Random();
    }

    
}
