// 무한 웨이브 조정 값.
// N번째 웨이브 예산 = BaseBudget + BudgetPerWave × (N − 1).
// 좀비 종류마다 SpawnCost만큼 예산을 쓰고, FirstWave부터 등장한다.

using System.Collections.Generic;
using UnityEngine;
using ZD.Gameplay.Zombies;

namespace ZD.Gameplay.Waves
{
    [CreateAssetMenu(menuName = "ZD/Wave Profile")]
    public sealed class WaveProfile : ScriptableObject
    {
        [SerializeField] private ZombieDefinition[] _zombieTypes;
        [SerializeField] private int _baseBudget = 6;
        [SerializeField] private int _budgetPerWave = 3;
        [SerializeField] private float _preparationSeconds = 30f;
        [SerializeField] private float _spawnIntervalSeconds = 0.75f;
        [Tooltip("Spawning pauses while this many zombies are alive.")]
        [SerializeField] private int _maxAliveZombies = 40;

        public IReadOnlyList<ZombieDefinition> ZombieTypes => _zombieTypes;
        public float PreparationSeconds => _preparationSeconds;
        public float SpawnIntervalSeconds => _spawnIntervalSeconds;
        public int MaxAliveZombies => _maxAliveZombies;

        public int GetBudget(int waveNumber) => _baseBudget + _budgetPerWave * Mathf.Max(0, waveNumber - 1);
    }
}
