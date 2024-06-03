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
            spell.SpellDamage += spell.SpellDamage * _damageIncrease;
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
            spell.SpellDelay -= spell.SpellDelay * _delayDecrease;
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
            spell.SpellSize += spell.SpellSize * _sizeIncrease;
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
            _spell.AddProjectileCount += _addProjectileCount;
            _spell.OnAddProjectile += UseSpellOfUpgrade;
        }

        public void UseSpellOfUpgrade(Transform projectileSpawnPoint = null)
        {
            for(int i = 0; i <  _spell.AddProjectileCount; i++)
            {
                IHitable hitable = _spell.SearchTarget_Random();
                if(hitable != null)
                    _spell.UseSpellOfUpgrade(hitable.Tf.position, projectileSpawnPoint);
            }
        }
    }

    public abstract class ImpactUpgrade : ISpellUpgrade
    {
        public ElementType _elementType;
        public MagicianSpell _spell;
        public virtual void ApplyUpgrade(MagicianSpell spell)
        {
            if (spell is TargetedProjecttile targeted)
            {
                targeted.Impact = this;
                _spell = spell;
                _elementType = spell.ElementType;
            }
        }

        public abstract void ApplyImpact(Transform tf);
    }

    public class AddExplosionOnImpactUpgrade : ImpactUpgrade, IData
    {
        public int Id => throw new System.NotImplementedException();
        private float _explosionRadius;
        private AOEEffectData _AOEEffectData;

        public AddExplosionOnImpactUpgrade(float explosionRadius, AOEEffectData aOEEffectData)
        {
            _explosionRadius = explosionRadius;
            _AOEEffectData = aOEEffectData;
        }

        public float AttackDamage { get => _spell.SpellDamage; }
        public float ExplosionRadius { get => _explosionRadius; }
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


    public static class SpellUpgradeFactory
    {
        public static ISpellUpgrade CreateUpgrade(SpellUpgradeData upgradeData)
        {
            switch (upgradeData.spellUpgradeType)
            {
                case SpellUpgradeType.IncreaseDamage:
                    return new IncreaseDamageUpgrade(upgradeData.floatValue);
                case SpellUpgradeType.DecreaseSpellDelay:
                    return new DecreaseDelayUpgrade(upgradeData.floatValue);
                case SpellUpgradeType.IncreaseSize:
                    return new IncreaseSizeUpgrade(upgradeData.floatValue);
                case SpellUpgradeType.IncreasePierce:
                    return new IncreasePierceUpgrade(upgradeData.integerValue);
                case SpellUpgradeType.AddProjectile:
                    return new AddProjectileUpgrade(upgradeData.integerValue);
                case SpellUpgradeType.AddExplosionOnImpact:
                    return new AddExplosionOnImpactUpgrade(upgradeData.floatValue,
                        Managers.Data.AOEEffectDataDict[1]);
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
    }
}
