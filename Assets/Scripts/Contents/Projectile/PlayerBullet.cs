using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : Projectile
{
    public override void InitDamageDealer(IData data)
    {
        MagicianSpell spellData = data as MagicianSpell;
        ElementType = spellData.ElementType;
        AttackDamage = spellData.SpellDamage;
        PierceCount = spellData.PierceCount;
        Managers.CompCache.GetOrAddComponentCache(gameObject, out SphereCollider sphCol);
        sphCol.radius = spellData.BaseSpellSize;
    }

    public override void InitMoveable(IData data)
    {
        MagicianSpell spellData = data as MagicianSpell;
        MoveSpeed = spellData.SpellSpeed;
    }
}

public class StarightTypePlayerBullet : PlayerBullet
{
    public override void InitDamageDealer(IData data)
    {
        MagicianSpell spellData = data as MagicianSpell;
        ElementType = spellData.ElementType;
        AttackDamage = spellData.SpellDamage;
        PierceCount = spellData.PierceCount;
        Managers.CompCache.GetOrAddComponentCache(gameObject, out BoxCollider boxCol);
        boxCol.size = new Vector3(spellData.SpellSize * 2, 1, 1);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("FrontWall"))
        {
            DestroyBullet();
            return;
        }
        if (other.gameObject.TryGetComponent(out IHitable hitable))
        {
            hitable.TakeDamage(this);

            GameObject obj = Managers.Resource.Instantiate(_explosionPath, hitable.Tf.position);
            Managers.CompCache.GetOrAddComponentCache(obj, out HitEffect hitEffect);
            hitEffect.Init();
        }
    }
}


