// 호스트 전용.
// 씬의 좀비 스폰 지점을 돌아가며 좀비를 스폰한다.

using Fusion;
using UnityEngine;
using ZD.Gameplay.Waves;
using ZD.Gameplay.Zombies;
using ZD.Network.Session;

namespace ZD.Network.Zombies
{
    public sealed class FusionZombieSpawner : IZombieSpawner
    {
        private readonly NetworkRunnerHolder _runnerHolder;
        private readonly NetworkPrefabCatalog _catalog;
        private readonly SpawnPointSet _spawnPoints;
        private readonly ZombieRosterModel _zombies;
        private int _nextSpawnIndex;

        public FusionZombieSpawner(NetworkRunnerHolder runnerHolder, NetworkPrefabCatalog catalog, SpawnPointSet spawnPoints, ZombieRosterModel zombies)
        {
            _runnerHolder = runnerHolder;
            _catalog = catalog;
            _spawnPoints = spawnPoints;
            _zombies = zombies;
        }

        public void Spawn(ZombieDefinition definition)
        {
            NetworkPrefabRef prefab = _catalog.GetZombiePrefab(definition);
            if (!prefab.IsValid || _spawnPoints.ZombieSpawnCount == 0)
            {
                return;
            }

            Transform spawn = _spawnPoints.GetZombieSpawn(_nextSpawnIndex++);
            _runnerHolder.Runner.Spawn(prefab, spawn.position, spawn.rotation);
        }

        public void DespawnAll()
        {
            NetworkRunner runner = _runnerHolder.Runner;
            // 제거하면 명단에서 빠지면서 마지막 항목이 그 자리로 오므로, 끝에서부터 돈다.
            for (int zombieIndex = _zombies.Count - 1; zombieIndex >= 0; zombieIndex--)
            {
                if (_zombies.GetModel(zombieIndex) is ZombieNetwork zombie && zombie.Object != null)
                {
                    runner.Despawn(zombie.Object);
                }
            }
        }
    }
}
