using Data;
using Interfaces;
using MagicianSpellUpgrade;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.Events;


#region Spell Behavior
public interface ISpellBehavior
{
    void Execute(MagicianSpell spell, Vector3 targetPos, Transform projectileSpawnPoint = null);
}

public class TargetedDirectionBehavior : ISpellBehavior
{
    public virtual void Execute(MagicianSpell spell, Vector3 targetPos, Transform projectileSpawnPoint = null)
    {
        Vector3 dir = new Vector3(targetPos.x - projectileSpawnPoint.position.x,0,
            targetPos.z - projectileSpawnPoint.position.z).normalized;

        GameObject obj = Managers.Resource.Instantiate("PlayerBullet",projectileSpawnPoint.position);
        Managers.CompCache.GetOrAddComponentCache(obj, out PlayerBullet playerBullet);
        obj.transform.localScale = Vector3.one * spell.SpellSize;
        playerBullet.Init(spell.EffectData as ProjectileData);
        playerBullet.InitDamageDealer(spell);
        playerBullet.InitMoveable(spell);

        playerBullet.SetDir(dir);

        if (spell.Impact != null)
        {
            playerBullet.OnImpact += spell.Impact.OnImpact;
        }
    }
}

public class StraightDirectionBehavior : ISpellBehavior
{
    public void Execute(MagicianSpell spell, Vector3 targetPos, Transform projectileSpawnPoint = null)
    {
        Vector3 pos = new Vector3(targetPos.x, 0,0);
        GameObject obj = Managers.Resource.Instantiate("StarightTypePlayerBullet",pos);
        Managers.CompCache.GetOrAddComponentCache(obj, out StarightTypePlayerBullet playerBullet);
        playerBullet.Init(spell.EffectData as ProjectileData);
        playerBullet.InitDamageDealer(spell);
        playerBullet.InitMoveable(spell);
        Vector3 dir = Vector3.forward;
        playerBullet.SetDir(dir);

        float newSize = spell.SpellSize;
        SetParticleSystemSize(obj, newSize);
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

public class TargetPositionBehavior : ISpellBehavior
{
    public void Execute(MagicianSpell spell, Vector3 targetPos, Transform projectileSpawnPoint = null)
    {
        Vector3 pos = new Vector3(targetPos.x, 0,targetPos.z);
        Vector3 rot = new Vector3(-90,0,0);
        GameObject obj = Managers.Resource.Instantiate("AOETypePlayerSpell",pos);
        obj.transform.rotation = Quaternion.Euler(rot);
        obj.transform.localScale = Vector3.one * spell.SpellSize;
        Managers.CompCache.GetOrAddComponentCache(obj, out AOETypePlayerSpell aoespell);
        aoespell.Init(spell.EffectData as AOEEffectData);
        aoespell.InitDamageDealer(spell);
    }
}
#endregion

public abstract class MagicianSpell : ISetData
{
    protected BaseSpellData BaseSpellData { get; set; }
    int IData.Id => id;
    public int id;

    public ISpellEffectData EffectData { get; set; }

    public ISpellBehavior SpellBehavior { get; set; }

    public System.Action OnUpdateSpellDelay;
    public System.Action<Transform> OnAddProjectile;

    private Transform _ownMagicianTransform;

    public Transform OwnMagicianTransform { get => _ownMagicianTransform; set => _ownMagicianTransform = value; }

    #region 데이터
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
    #endregion

    public List<ISpellUpgrade> Upgrades { get; set; } = new();
    public ExplosionOnImpact Impact = null;

    protected void Init(BaseSpellData data)
    {
        id = data.id;
        EffectId = data.effectId;
        ElementType = data.elementType;
        SpellDamage = data.spellDamage * Managers.Game.PlayerAttackPower;
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

    public virtual void UseSpell(Vector3 targetPos, Transform projectileSpawnPoint = null)
    {
        SpellBehavior.Execute(this, targetPos, projectileSpawnPoint);
        OnAddProjectile?.Invoke(projectileSpawnPoint);
    }

    public virtual void UseSpellOfUpgrade(Vector3 targetPos, Transform projectileSpawnPoint = null)
    {
        SpellBehavior.Execute(this, targetPos, projectileSpawnPoint);
    }

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

public class TargetedProjectile : MagicianSpell
{
    public TargetedProjectile(BaseSpellData data)
    {
        BaseSpellData = data;
        Init(data);
        EffectData = Managers.Data.ProjectileDataDict[data.effectId];
        SpellBehavior = new TargetedDirectionBehavior();
    }

    public override IHitable SearchTarget(Transform transform)
    {
        return SearchTarget_Closest(transform);
    }
}

public class TargetedProjectileOfExplosion : MagicianSpell
{
    public TargetedProjectileOfExplosion(BaseSpellData data)
    {
        BaseSpellData = data;
        Init(data);
        
        EffectData = Managers.Data.ProjectileDataDict[data.effectId];
        SpellBehavior = new TargetedDirectionBehavior();
        Impact = new ExplosionOnImpact();

        Impact.explosionRange = (float)data.explosionRange;
    }

    public override IHitable SearchTarget(Transform transform)
    {
        return SearchTarget_Closest(transform);
    }
}

public class ExplosionOnImpact
{
    public float explosionRange;

    public void OnImpact(IDamageDealer dealer,Transform tf)
    {
        // 폭발 데미지를 적용하는 로직 구현
        List<IHitable> hits = new List<IHitable>();
        foreach (Enemy enemy in Managers.Game.Enemies)
        {
            float distance = Vector3.Distance(tf.position, enemy.transform.position);
            if (distance > explosionRange)
                continue;
            hits.Add(enemy);
        }
        foreach (IHitable hit in hits)
            hit.TakeDamage(dealer);
    }
}

public class StraightProjectile : MagicianSpell
{
    public StraightProjectile(BaseSpellData data)
    {
        BaseSpellData = data;
        Init(data);
        EffectData = Managers.Data.ProjectileDataDict[data.effectId];
        SpellBehavior = new StraightDirectionBehavior();
    }

    public override IHitable SearchTarget(Transform transform)
    {
        return SearchTarget_Random();
    } 
}

public class TargetedAOE : MagicianSpell
{
    public TargetedAOE(BaseSpellData data)
    {
        BaseSpellData = data;
        Init(data);
        EffectData = Managers.Data.AOEEffectDataDict[data.effectId];
        SpellBehavior = new TargetPositionBehavior();
    }

    public override IHitable SearchTarget(Transform transform)
    {
        return SearchTarget_Random();
    }  
}
