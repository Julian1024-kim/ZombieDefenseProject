// 호스트 전용.
// 맵의 거점 자리에 거점 네트워크 프리팹을 스폰한다(세션 시작 전에 미리 불러와 있음).

using Fusion;
using UnityEngine;
using ZD.Core;
using ZD.Gameplay.Defense;
using ZD.Network.Session;

namespace ZD.Network.Defense
{
    public sealed class FusionBaseSpawner : IBaseSpawner
    {
        private readonly NetworkRunnerHolder _runnerHolder;
        private readonly NetworkPrefabCatalog _catalog;
        private readonly SpawnPointSet _spawnPoints;

        public FusionBaseSpawner(NetworkRunnerHolder runnerHolder, NetworkPrefabCatalog catalog, SpawnPointSet spawnPoints)
        {
            _runnerHolder = runnerHolder;
            _catalog = catalog;
            _spawnPoints = spawnPoints;
        }

        public void Spawn()
        {
            Transform point = _spawnPoints.BaseSpawnPoint;
            if (!_catalog.BasePrefab.IsValid || point == null)
            {
                DevLog.Info("거점 프리팹이나 거점 자리가 없어 거점을 만들지 못했습니다.");
                return;
            }

            _runnerHolder.Runner.Spawn(_catalog.BasePrefab, point.position, Quaternion.identity);
        }
    }
}
