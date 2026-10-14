// 호스트 전용.
// 웨이브를 만들고, 동시 생존 수 제한을 지키며 좀비를 하나씩 생성한다.

using System.Collections.Generic;
using UnityEngine;
using ZD.Core;
using ZD.Gameplay.Zombies;

namespace ZD.Gameplay.Waves
{
    public sealed class WaveSystem
    {
        private const int SPAWN_QUEUE_CAPACITY = 128;

        private readonly INetworkClock _clock;
        private readonly WaveProfile _profile;
        private readonly WaveBudgetGenerator _generator;
        private readonly IZombieSpawner _spawner;
        private readonly ZombieRosterModel _zombies;
        private readonly List<ZombieDefinition> _spawnQueue = new(SPAWN_QUEUE_CAPACITY);
        private int _nextSpawnIndex;
        private int _nextSpawnTick;

        public WaveSystem(INetworkClock clock, WaveProfile profile, WaveBudgetGenerator generator, IZombieSpawner spawner, ZombieRosterModel zombies)
        {
            _clock = clock;
            _profile = profile;
            _generator = generator;
            _spawner = spawner;
            _zombies = zombies;
        }

        public bool IsCleared => _nextSpawnIndex >= _spawnQueue.Count && _zombies.AliveCount == 0;

        public int BeginWave(int waveNumber)
        {
            _generator.Generate(_profile, waveNumber, _spawnQueue);
            _nextSpawnIndex = 0;
            _nextSpawnTick = _clock.Tick;
            return _spawnQueue.Count;
        }

        public void ResetMatch()
        {
            _spawnQueue.Clear();
            _nextSpawnIndex = 0;
            _spawner.DespawnAll();
        }

        public void Tick()
        {
            int tick = _clock.Tick;
            if (_nextSpawnIndex >= _spawnQueue.Count || tick < _nextSpawnTick)
            {
                return;
            }

            if (_zombies.AliveCount >= _profile.MaxAliveZombies)
            {
                return;
            }

            _spawner.Spawn(_spawnQueue[_nextSpawnIndex]);
            _nextSpawnIndex++;
            _nextSpawnTick = tick + Mathf.Max(1, _clock.SecondsToTicks(_profile.SpawnIntervalSeconds));
        }
    }
}
