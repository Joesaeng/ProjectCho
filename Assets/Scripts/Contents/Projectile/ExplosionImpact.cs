using Data;
using Interfaces;
using MagicianSpellUpgrade;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionImpact : MonoBehaviour, IDamageDealer
{
    GameObject AOESpellObject { get; set; }
    private ElementType _elementType;
    HashSet<IHitable> Targets { get; set; } = new();
    public ElementType ElementType { get => _elementType; set => _elementType = value; }

    string AOESpellPath;

    public float AttackDamage { get; set; }

    public void Init(IData data)
    {
        AOEEffectData aOEEffectData = data as AOEEffectData;
        AOESpellPath = "Effects/AOE/" + aOEEffectData.effectName;

        AOESpellObject = Managers.Resource.Instantiate(AOESpellPath, transform);
    }

    public void InitDamageDealer(IData data)
    {
        AddExplosionOnImpactUpgrade explosionData = data as AddExplosionOnImpactUpgrade;
        AttackDamage = explosionData.AttackDamage;
        ElementType = explosionData.ElementType;
        Managers.CompCache.GetOrAddComponentCache(gameObject, out SphereCollider sphCol);
        sphCol.radius = 1f;

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
        yield return YieldCache.WaitForSeconds(0.01f);
        foreach (IHitable target in Targets)
        {
            if (target.IsDead)
                continue;
            target.TakeDamage(this);
        }
        yield return YieldCache.WaitForSeconds(1f);
        DestroyImpact();
    }

    protected void DestroyImpact()
    {
        Targets.Clear();
        Managers.Resource.Destroy(AOESpellObject);
        Managers.Resource.Destroy(gameObject);
    }
}
