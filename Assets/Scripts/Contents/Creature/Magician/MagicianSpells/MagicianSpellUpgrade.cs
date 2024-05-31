using Data;
using Interfaces;
using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace MagicianSpellUpgrade
{
    public interface ISpellUpgrade
    {
        void ApplyUpgrade(MagicianSpell spell);
    }

    public class IncreaseDamageUpgrade : ISpellUpgrade
    {
        private float _damageIncrease;

        public IncreaseDamageUpgrade(float damageIncrease)
        {
            _damageIncrease = damageIncrease;
        }

        public void ApplyUpgrade(MagicianSpell spell)
        {
            spell.SpellDamage *= _damageIncrease;
        }
    }

    public class DecreaseDelayUpgrade : ISpellUpgrade
    {
        private float _delayDecrease;

        public DecreaseDelayUpgrade(float delayDecrease)
        {
            _delayDecrease = delayDecrease;
        }

        public void ApplyUpgrade(MagicianSpell spell)
        {
            spell.SpellDelay *= _delayDecrease;
            spell.OnUpdateSpellDelay?.Invoke();
        }
    }

    public class IncreaseSizeUpgrade : ISpellUpgrade
    {
        private float _sizeIncrease;

        public IncreaseSizeUpgrade(float sizeIncrease)
        {
            _sizeIncrease = sizeIncrease;
        }

        public void ApplyUpgrade(MagicianSpell spell)
        {
            spell.SpellSize *= _sizeIncrease;
        }
    }

    public class IncreasePierceUpgrade : ISpellUpgrade
    {
        private int _pierceIncrease;

        public IncreasePierceUpgrade(int pierceIncrease)
        {
            _pierceIncrease = pierceIncrease;
        }

        public void ApplyUpgrade(MagicianSpell spell)
        {
            spell.PireceCount += _pierceIncrease;
        }
    }


    public class AddProjectileUpgrade : ISpellUpgrade
    {
        private int _addProjectileCount;
        private MagicianSpell _spell;

        public AddProjectileUpgrade(int  addProjectileCount)
        {
            _addProjectileCount = addProjectileCount;
        }

        public void ApplyUpgrade(MagicianSpell spell)
        {
            _spell = spell;
            _spell.OnAddProjectile += UseSpellOfUpgrade;
        }

        public void UseSpellOfUpgrade(Transform projectileSpawnPoint = null)
        {
            for(int i = 0; i <  _addProjectileCount; i++)
            {
                IHitable hitable = _spell.SearchTarget_Random();
                if(hitable != null)
                    _spell.UseSpellOfUpgrade(hitable.Tf.position, projectileSpawnPoint);
            }
        }
    }

    public abstract class ImpactUpgrade : ISpellUpgrade
    {
        public virtual void ApplyUpgrade(MagicianSpell spell)
        {
            if (spell is TargetedProjecttile targeted)
            {
                targeted.Impact = this;
            }
        }

        public abstract void ApplyImpact(Transform tf);
    }

    public class AddExplosionOnImpactUpgrade : ImpactUpgrade, IData
    {
        public int id => throw new System.NotImplementedException();
        private float _explosionDamage;
        private float _explosionRadius;
        private float _explosionColSize = 1f;
        private ElementType _elementType;
        private AOEEffectData _AOEEffectData;

        public AddExplosionOnImpactUpgrade(float explosionDamage, float explosionRadius, ElementType elementType, AOEEffectData aOEEffectData)
        {
            _explosionDamage = explosionDamage;
            _explosionRadius = explosionRadius;
            _elementType = elementType;
            _AOEEffectData = aOEEffectData;
        }

        public float AttackDamage { get => _explosionDamage; set => _explosionDamage = value; }
        public float ExplosionRadius { get => _explosionRadius; }
        public float ExplosionColSize { get => _explosionColSize; }
        public ElementType ElementType { get => _elementType; set => _elementType = value; }

        public override void ApplyImpact(Transform tf)
        {
            // 폭발 데미지를 적용하는 로직 구현
            GameObject obj = Managers.Resource.Instantiate("AOETypePlayerSpell",tf.position);
            obj.transform.localScale = Vector3.one * ExplosionRadius;
            Managers.CompCache.GetOrAddComponentCache(obj, out ExplosionImpact Impact);
            Impact.Init(_AOEEffectData);
            Impact.InitDamageDealer(this);
        }
    }

    public class ChainProjectileUpgrade : ImpactUpgrade
    {
        private int _chainProjectileCount;
        private float _damageDecrease;
        private MagicianSpell _spell;

        public ChainProjectileUpgrade(int chainProjectileCount, float damageDecrease)
        {
            _chainProjectileCount = chainProjectileCount;
            _damageDecrease = damageDecrease;
        }

        public override void ApplyUpgrade(MagicianSpell spell)
        {
            base.ApplyUpgrade(spell);
            _spell = spell;
            spell.SpellDamage *= _damageDecrease;
            
        }
        public override void ApplyImpact(Transform tf)
        {
            if(_spell is TargetedProjecttile spell)
            {
                Vector3[] dirs = GenerateDirctions(tf,_chainProjectileCount);
                foreach(Vector3 dir in dirs)
                {
                    spell.ShotProjectile(dir, tf);
                }
            }
        }
        
        private Vector3[] GenerateDirctions(Transform tf,int count)
        {
            Vector3[] directions = new Vector3[count];
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep;
                Vector3 dir = Quaternion.Euler(0,angle,0) * tf.forward;
                directions[i] = dir.normalized;
            }

            return directions;
        }
    }
}
