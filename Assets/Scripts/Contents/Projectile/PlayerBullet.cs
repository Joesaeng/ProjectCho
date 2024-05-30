using Data;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : Projectile
{
    public override void InitDamageDealer(IData data)
    {
        BaseSpellData spellData = data as BaseSpellData;
        AttackDamage = spellData.spellDamage;
        PierceCount = spellData.pierceCount;
        Managers.CompCache.GetOrAddComponentCache(gameObject, out SphereCollider sphCol);
        sphCol.radius = spellData.spellSize;
    }

    public override void InitMoveable(IData data)
    {
        BaseSpellData baseSpellData = data as BaseSpellData;
        MoveSpeed = baseSpellData.spellSpeed;
    }
}

public class StarightTypePlayerBullet : PlayerBullet
{
    public override void InitDamageDealer(IData data)
    {
        BaseSpellData spellData = data as BaseSpellData;
        AttackDamage = spellData.spellDamage;
        PierceCount = spellData.pierceCount;
        Managers.CompCache.GetOrAddComponentCache(gameObject, out BoxCollider boxCol);
        boxCol.size = new Vector3(spellData.spellSize * 2, 1, 1);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            DestroyBullet();
            return;
        }
        if (other.gameObject.TryGetComponent(out IHitable hitable))
        {
            hitable.TakeDamage(this);

            // GameObject obj = Managers.Resource.Instantiate(_explosionPath, hitable.Tf.position);
            // Managers.CompCache.GetOrAddComponentCache(obj, out ProjectileExplosion projectileExplosion);
            // projectileExplosion.Init();
        }
    }
}


