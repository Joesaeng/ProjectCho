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
        SpellUpgradeType UpgradeType { get; set; }
        float UpgradeValue { get; }
    }

    public class IncreaseDamageUpgrade : ISpellUpgrade
    {
        public SpellUpgradeType UpgradeType { get; set; }
        public float UpgradeValue => _damageIncrease;
        private float _damageIncrease;

        public IncreaseDamageUpgrade(float damageIncrease)
        {
            UpgradeType = SpellUpgradeType.IncreaseDamage;
            _damageIncrease = damageIncrease;
        }


        public void ApplyUpgrade(MagicianSpell spell)
        {
            spell.SpellDamage += spell.SpellDamage * _damageIncrease;
        }
    }

    public class DecreaseDelayUpgrade : ISpellUpgrade
    {
        public SpellUpgradeType UpgradeType { get; set; }
        public float UpgradeValue => _delayDecrease;
        private float _delayDecrease;

        public DecreaseDelayUpgrade(float delayDecrease)
        {
            UpgradeType = SpellUpgradeType.DecreaseSpellDelay;
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
        public SpellUpgradeType UpgradeType { get; set; }
        public float UpgradeValue => _sizeIncrease;
        private float _sizeIncrease;

        public IncreaseSizeUpgrade(float sizeIncrease)
        {
            UpgradeType = SpellUpgradeType.IncreaseSize;
            _sizeIncrease = sizeIncrease;
        }

        public void ApplyUpgrade(MagicianSpell spell)
        {
            spell.SpellSize += spell.SpellSize * _sizeIncrease;
        }
    }

    public class IncreasePierceUpgrade : ISpellUpgrade
    {
        public SpellUpgradeType UpgradeType { get; set; }
        public float UpgradeValue => _pierceIncrease;
        private int _pierceIncrease;

        public IncreasePierceUpgrade(int pierceIncrease)
        {
            UpgradeType = SpellUpgradeType.IncreasePierce;
            _pierceIncrease = pierceIncrease;
        }

        public void ApplyUpgrade(MagicianSpell spell)
        {
            spell.PierceCount += _pierceIncrease;
        }
    }

    public class AddProjectileUpgrade : ISpellUpgrade
    {
        public SpellUpgradeType UpgradeType { get; set; }
        public float UpgradeValue => _addProjectileCount;
        private int _addProjectileCount;
        private MagicianSpell _spell;
        private float _delayBetweenShots = 0.15f;

        public AddProjectileUpgrade(int addProjectileCount)
        {
            UpgradeType = SpellUpgradeType.AddProjectile;
            _addProjectileCount = addProjectileCount;
        }

        public void ApplyUpgrade(MagicianSpell spell)
        {
            _spell = spell;
            _spell.AddProjectileCount += _addProjectileCount;
            if (_spell.OnAddProjectile == null)
                _spell.OnAddProjectile += Execute;
        }

        private void Execute(Transform projectileSpawnPoint)
        {
            for (int i = 0; i < _spell.AddProjectileCount; i++)
            {
                float delay = _delayBetweenShots * (i+1);
                Managers.Timer.StartTimer(delay, () => ShootProjectile(projectileSpawnPoint));
            }
        }

        private void ShootProjectile(Transform projectileSpawnPoint)
        {
            IHitable primaryTarget = _spell.SearchTarget(_spell.OwnTransform);

            if (primaryTarget == null)
                return;

            Vector3 targetPosition = primaryTarget.Tf.position;
            _spell.UseSpellOfUpgrade(targetPosition, projectileSpawnPoint);
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
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
    }
}
