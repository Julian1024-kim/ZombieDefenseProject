// 방어막 한 종류의 고정 데이터: 이름, 가격, 크기, 체력, 함정 수치.
// 바리케이드·콘크리트·지뢰·곰덫이 각각 이 에셋 하나다.

using UnityEngine;

namespace ZD.Gameplay.Building
{
    [CreateAssetMenu(menuName = "ZD/Barricade Definition")]
    public sealed class BarricadeDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private BarricadeKind _kind;
        [SerializeField] private int _price = 300;
        [SerializeField] private Vector3 _size = new(2f, 1.2f, 0.4f);
        [SerializeField] private int _maxHealth = 300;

        [Header("함정 (지뢰, 곰덫)")]
        [SerializeField] private float _triggerRadius = 0.8f;
        [SerializeField] private float _effectRadius = 3f;
        [SerializeField] private int _trapDamage = 150;
        [SerializeField] private float _holdSeconds = 3f;
        [SerializeField] private int _uses = 1;

        public string DisplayName => _displayName;
        public BarricadeKind Kind => _kind;
        public int Price => _price;
        public Vector3 Size => _size;
        public int MaxHealth => _maxHealth;
        public float TriggerRadius => _triggerRadius;
        public float EffectRadius => _effectRadius;
        public int TrapDamage => _trapDamage;
        public float HoldSeconds => _holdSeconds;
        public int Uses => _uses;

        public bool BlocksPath => _kind == BarricadeKind.Wall;
    }
}
