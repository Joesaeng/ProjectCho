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

        SetParticleSystemSize(obj, spell.SpellSize);
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
    protected SpellDataByPlayerOwnedSpell SpellData { get; set; }
    int IData.Id => id;
    public int id;

    public ISpellEffectData EffectData { get; set; }

    public ISpellBehavior SpellBehavior { get; set; }

    public System.Action OnUpdateSpellDelay;
    public System.Action<Transform> OnAddProjectile;

    private Transform _ownTransform;

    public Transform OwnTransform { get => _ownTransform; set => _ownTransform = value; }

    #region 데이터
    public int EffectId { get; set; }
    public ElementType ElementType { get; set; }
    public float SpellDamage { get; set; }
    public float SpellDelay { get; set; }
    public float SpellRange { get; set; }
    public float SpellSpeed { get; set; }
    public float SpellSize { get; set; }
    public float BaseSpellSize { get; set; }
    public int PierceCount { get; set; }
    public int AddProjectileCount { get; set; }

    public int IntegerParam1 { get; set; }
    public int IntegerParam2 { get; set; }
    public float FloatParam1 { get; set; }
    public float FloatParam2 { get; set; }
    #endregion

    public List<ISpellUpgrade> Upgrades { get; set; } = new();
    public ExplosionOnImpact Impact = null;

    protected void Init(SpellDataByPlayerOwnedSpell data)
    {
        id = data.id;
        EffectId = data.effectId;
        ElementType = data.elementType;
        SpellDamage = data.spellDamageCoefficient;
        SpellDelay = data.spellDelay;
        SpellRange = data.spellRange;
        SpellSpeed = data.spellSpeed;
        SpellSize = data.spellSize;
        BaseSpellSize = data.spellSize;
        PierceCount = data.pierceCount;
        IntegerParam1 = data.integerParam1;
        IntegerParam2 = data.integerParam2;
        FloatParam1 = data.floatParam1;
        FloatParam2 = data.floatParam2;

        ApplyPlayerStatusToSpell();
    }

    private void ApplyPlayerStatusToSpell()
    {
        PlayerStatus playerStatus = Managers.Player.PlayerStatus;
        SpellDamage *= playerStatus.damage;

        if (playerStatus.floatOptions.TryGetValue(StatusType.DecreaseSpellDelay, out float decreaseSpellDelay))
            SpellDelay *= (float)(1 - decreaseSpellDelay);
        if (playerStatus.integerOptions.TryGetValue(StatusType.IncreasePierce, out int increasePierce))
            PierceCount += increasePierce;
        if (playerStatus.integerOptions.TryGetValue(StatusType.AddProjectile, out int addProjectile))
            AddProjectileCount += addProjectile;

        StatusType increaseElementDamage = Util.Parse<StatusType>($"Increase{ElementType}SpellDamage");

        if(playerStatus.floatOptions.TryGetValue(increaseElementDamage, out float value))
            SpellDamage *= (float)(1 + value);
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

        foreach (Enemy enemy in DefenseSceneManager.Instance.Enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (enemy.IsDead || distance > SpellRange)
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

        foreach (Enemy enemy in DefenseSceneManager.Instance.Enemies)
        {
            float distance = Vector3.Distance(_ownTransform.position, enemy.transform.position);
            if (enemy.IsDead || distance > SpellRange)
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
    public TargetedProjectile(SpellDataByPlayerOwnedSpell data)
    {
        SpellData = data;
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
    public TargetedProjectileOfExplosion(SpellDataByPlayerOwnedSpell data)
    {
        SpellData = data;
        Init(data);
        
        EffectData = Managers.Data.ProjectileDataDict[data.effectId];
        SpellBehavior = new TargetedDirectionBehavior();
        Impact = new ExplosionOnImpact
        {
            explosionRange = (float)data.floatParam2
        };
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
        List<IHitable> hits = new List<IHitable>();
        foreach (Enemy enemy in DefenseSceneManager.Instance.Enemies)
        {
            float distance = Vector3.Distance(tf.position, enemy.transform.position);
            if (enemy.IsDead || distance > explosionRange)
                continue;
            hits.Add(enemy);
        }
        foreach (IHitable hit in hits)
            hit.TakeDamage(dealer);
    }
}

public class StraightProjectile : MagicianSpell
{
    public StraightProjectile(SpellDataByPlayerOwnedSpell data)
    {
        SpellData = data;
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
    public TargetedAOE(SpellDataByPlayerOwnedSpell data)
    {
        SpellData = data;
        Init(data);
        EffectData = Managers.Data.AOEEffectDataDict[data.effectId];
        SpellBehavior = new TargetPositionBehavior();
    }

    public override IHitable SearchTarget(Transform transform)
    {
        return SearchTarget_Random();
    }  
}
