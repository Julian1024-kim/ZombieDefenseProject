// 시작 장비와 회복 키트 설정(최대 개수, 회복량, 사용 시간).

using UnityEngine;
using ZD.Gameplay.Weapons;

namespace ZD.Gameplay.Inventory
{
    [CreateAssetMenu(menuName = "ZD/Inventory Settings")]
    public sealed class InventorySettings : ScriptableObject
    {
        [SerializeField] private WeaponDefinition _startingSecondary;
        [SerializeField] private WeaponDefinition _startingMelee;

        [Header("Health kit")]
        [SerializeField] private int _maxHealKits = 2;
        [SerializeField] private int _healAmount = 70;
        [SerializeField] private float _healSeconds = 3f;

        public WeaponDefinition StartingSecondary => _startingSecondary;
        public WeaponDefinition StartingMelee => _startingMelee;
        public int MaxHealKits => _maxHealKits;
        public int HealAmount => _healAmount;
        public float HealSeconds => _healSeconds;
    }
}
