using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOETypePlayerSpell : MonoBehaviour, IDamageDealer
{
    GameObject AOESpellObject { get; set; }
    private ElementType _elementType;
    HashSet<IHitable> Targets { get; set; } = new();
    public ElementType ElementType { get => _elementType; set => _elementType = value; }

    string AOESpellPath;
    string ExplosionPath;

    public float AttackDamage { get; set; }

    public void Init(IData data)
    {
        AOEEffectData aOEEffectData = data as AOEEffectData;
        AOESpellPath = "Effects/AOE/" + aOEEffectData.effectName;
        ExplosionPath = "Effects/Explosions/" + aOEEffectData.explosionName;

        AOESpellObject = Managers.Resource.Instantiate(AOESpellPath, transform);
    }

    public void InitDamageDealer(IData data)
    {
        MagicianSpell spellData = data as MagicianSpell;
        AttackDamage = spellData.SpellDamage;
        ElementType = spellData.ElementType;
        Managers.CompCache.GetOrAddComponentCache(gameObject, out SphereCollider sphCol);
        sphCol.radius = spellData.BaseSpellSize;

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
        yield return YieldCache.WaitForSeconds(0.05f);
        foreach (IHitable target in Targets)
        {
            if(target.IsDead) 
                continue;
            target.TakeDamage(this);

            GameObject obj = Managers.Resource.Instantiate(ExplosionPath, target.Tf.position);
            Managers.CompCache.GetOrAddComponentCache(obj, out HitEffect hitEffect);
            hitEffect.Init();

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
