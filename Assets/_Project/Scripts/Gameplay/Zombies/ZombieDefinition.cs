// 좀비 종류 하나의 수치(체력, 속도, 공격, 처치 Point, 웨이브 비용, 첫 등장 웨이브).

using UnityEngine;

namespace ZD.Gameplay.Zombies
{
    [CreateAssetMenu(menuName = "ZD/Zombie Definition")]
    public sealed class ZombieDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private float _moveSpeed = 2.5f;
        [SerializeField] private int _attackDamage = 10;
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _attackIntervalSeconds = 1.2f;

        [Header("Rewards")]
        [SerializeField] private int _killPoints = 50;
        [SerializeField] private int _headshotBonusPoints = 20;

        [Header("Wave generation")]
        [Tooltip("Budget points one zombie of this type costs in a wave.")]
        [SerializeField] private int _spawnCost = 1;
        [Tooltip("First wave (1-based) this type can appear in.")]
        [SerializeField] private int _firstWave = 1;

        public string DisplayName => _displayName;
        public int MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public int AttackDamage => _attackDamage;
        public float AttackRange => _attackRange;
        public float AttackIntervalSeconds => _attackIntervalSeconds;
        public int KillPoints => _killPoints;
        public int HeadshotBonusPoints => _headshotBonusPoints;
        public int SpawnCost => _spawnCost;
        public int FirstWave => _firstWave;
    }
}
