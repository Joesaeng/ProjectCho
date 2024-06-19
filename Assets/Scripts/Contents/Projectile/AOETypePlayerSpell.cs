using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AOETypePlayerSpell : MonoBehaviour, IDamageDealer
{
    GameObject AOESpellObject { get; set; }
    protected HashSet<IHitable> Targets { get; set; } = new();

    private ElementType _elementType;
    public ElementType ElementType { get => _elementType; set => _elementType = value; }

    string AOESpellPath;
    protected string ExplosionPath;

    public float AttackDamage { get; set; }
    public float SpellDuration { get; set; }

    public void Init(IData data)
    {
        AOEEffectData aOEEffectData = data as AOEEffectData;
        AOESpellPath = "Effects/AOE/" + aOEEffectData.effectName;
        ExplosionPath = "Effects/Explosions/" + aOEEffectData.explosionName;

        AOESpellObject = Managers.Resource.Instantiate(AOESpellPath, transform);
        AOESpellObject.transform.localScale = Vector3.one;
    }

    public void InitDamageDealer(IData data)
    {
        MagicianSpell spellData = data as MagicianSpell;
        AttackDamage = spellData.SpellDamage;
        ElementType = spellData.ElementType;
        Managers.CompCache.GetOrAddComponentCache(gameObject, out SphereCollider sphCol);
        sphCol.radius = 1;
        SpellDuration = spellData.FloatParam1;

        StartCoroutine(CoImpact());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IHitable hitable))
        {
            Targets.Add(hitable);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (Targets.TryGetValue(other.gameObject.GetComponent<IHitable>(), out IHitable hitable))
        {
            Targets.Remove(hitable);
        }
    }

    IEnumerator CoImpact()
    {
        yield return YieldCache.WaitForSeconds(0.05f);
        float curDuration = 0f;
        List<IHitable> deadTargets = new();
        while (true)
        {
            curDuration += 0.5f;
            foreach (IHitable target in Targets)
            {
                if (target.IsDead)
                {
                    continue;
                }
                if(target.TakeDamage(this))
                    deadTargets.Add(target);

                GameObject obj = Managers.Resource.Instantiate(ExplosionPath, target.Tf.position);
                Managers.CompCache.GetOrAddComponentCache(obj, out HitEffect hitEffect);
                hitEffect.Init();
            }
            for(int i = 0; i < deadTargets.Count; i++)
            {
                Targets.Remove(deadTargets[i]);
            }
            yield return YieldCache.WaitForSeconds(0.5f);
            if (curDuration >= SpellDuration)
            {
                DestroyAOE();
            }
        }
    }

    void DestroyAOE()
    {
        Targets.Clear();
        Managers.Resource.Destroy(AOESpellObject);
        Managers.Resource.Destroy(gameObject);
    }
}
