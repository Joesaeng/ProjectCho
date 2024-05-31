using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOETypePlayerSpell : MonoBehaviour, IDamageDealer
{
    GameObject AOESpellObject { get; set; }
    HashSet<IHitable> Targets { get; set; } = new();

    string AOESpellPath;
    string ExplosionPath;

    public float AttackDamage { get; set; }

    public void Init(IData data)
    {
        AOEEffectData aOEEffectData = data as AOEEffectData;
        AOESpellPath = "ParticleEffects/AOE/" + aOEEffectData.effectName;
        ExplosionPath = "ParticleEffects/Explosions/" + aOEEffectData.explosionName;

        AOESpellObject = Managers.Resource.Instantiate(AOESpellPath, transform);
    }

    public void InitDamageDealer(IData data)
    {
        BaseSpellData spellData = data as BaseSpellData;
        AttackDamage = spellData.spellDamage;

        Managers.CompCache.GetOrAddComponentCache(gameObject, out SphereCollider sphCol);
        sphCol.radius = spellData.spellSize;

        StartCoroutine(CoImpact());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IHitable hitable))
        {
            Targets.Add(hitable);
        }
    }

    IEnumerator CoImpact()
    {
        yield return YieldCache.WaitForSeconds(0.1f);
        foreach (IHitable target in Targets)
        {
            target.TakeDamage(this);

            GameObject obj = Managers.Resource.Instantiate(ExplosionPath, target.Tf.position);
            Managers.CompCache.GetOrAddComponentCache(obj, out ProjectileExplosion projectileExplosion);
            projectileExplosion.Init();

        }
        yield return YieldCache.WaitForSeconds(1f);
        DestroyAOE();
    }

    protected void DestroyAOE()
    {
        Targets.Clear();
        Managers.Resource.Destroy(AOESpellObject);
        Managers.Resource.Destroy(gameObject);
    }
}
