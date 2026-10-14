// 무기 하나의 수치(피해, 연사 속도, 사거리, 탄창, 재장전, 근접 범위, 산탄 패턴)와 발사 방식.

using System;
using UnityEngine;
using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Weapons
{
    [CreateAssetMenu(menuName = "ZD/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private WeaponCategory _category;
        [SerializeField] private WeaponBehaviorType _behaviorType;
        [SerializeField] private int _damage = 25;
        [SerializeField] private float _shotsPerSecond = 4f;
        [SerializeField] private bool _isAutomatic;
        [SerializeField] private float _range = 60f;
        [SerializeField] private bool _allowsHeadshot = true;
        [SerializeField] private float _headshotMultiplier = 2f;

        [Header("Ammo (ignored for melee)")]
        [SerializeField] private int _magazineSize = 12;
        [SerializeField] private int _startingReserveAmmo = 48;
        [SerializeField] private float _reloadSeconds = 1.5f;

        [Header("Melee")]
        [SerializeField] private float _meleeRadius = 0.8f;
        [SerializeField] private int _maxMeleeTargets = 3;

        [Header("Spread (shotgun)")]
        [Tooltip("Pellets per shot. Damage applies per pellet.")]
        [SerializeField] private int _pelletCount = 8;
        [Tooltip("Cone half-angle of the pellet pattern, in degrees.")]
        [SerializeField] private float _spreadDegrees = 6f;

        public string DisplayName => _displayName;
        public WeaponCategory Category => _category;
        public float ShotsPerSecond => _shotsPerSecond;
        public bool IsAutomatic => _isAutomatic;
        public float Range => _range;
        public bool UsesAmmo => _behaviorType != WeaponBehaviorType.Melee;
        public int MagazineSize => _magazineSize;
        public int StartingReserveAmmo => _startingReserveAmmo;
        public float ReloadSeconds => _reloadSeconds;
        public float MeleeRadius => _meleeRadius;

        public int GetDamage(HitZone zone)
        {
            if (_allowsHeadshot && zone == HitZone.Head)
            {
                return Mathf.RoundToInt(_damage * _headshotMultiplier);
            }

            return _damage;
        }

        public IWeaponBehavior CreateBehavior()
        {
            return _behaviorType switch
            {
                WeaponBehaviorType.Hitscan => new HitscanWeaponBehavior(),
                WeaponBehaviorType.Melee => new MeleeWeaponBehavior(_maxMeleeTargets),
                WeaponBehaviorType.Spread => new SpreadWeaponBehavior(_pelletCount, _spreadDegrees),
                _ => throw new ArgumentOutOfRangeException(nameof(_behaviorType)),
            };
        }
    }
}
