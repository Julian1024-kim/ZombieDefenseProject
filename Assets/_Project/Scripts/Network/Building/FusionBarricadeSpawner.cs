// 호스트 전용.
// 방어막 정의에 맞는 네트워크 프리팹을 스폰하고, 재시작 때 모두 제거한다.
// 프리팹은 세션 시작 전에 미리 불러와 있어서 스폰이 메인 스레드를 막지 않는다.

using Fusion;
using UnityEngine;
using ZD.Gameplay.Building;
using ZD.Network.Session;

namespace ZD.Network.Building
{
    public sealed class FusionBarricadeSpawner : IBarricadeSpawner
    {
        private readonly NetworkRunnerHolder _runnerHolder;
        private readonly NetworkPrefabCatalog _catalog;
        private readonly BarricadeRosterModel _barricades;

        public FusionBarricadeSpawner(NetworkRunnerHolder runnerHolder, NetworkPrefabCatalog catalog, BarricadeRosterModel barricades)
        {
            _runnerHolder = runnerHolder;
            _catalog = catalog;
            _barricades = barricades;
        }

        public IBarricadeModel Spawn(BarricadeDefinition definition, Vector3 position, Quaternion rotation)
        {
            NetworkPrefabRef prefab = _catalog.GetBarricadePrefab(definition);
            if (!prefab.IsValid)
            {
                return null;
            }

            NetworkObject spawned = _runnerHolder.Runner.Spawn(prefab, position, rotation);
            return spawned != null ? spawned.GetComponent<BarricadeNetwork>() : null;
        }

        public void DespawnAll()
        {
            NetworkRunner runner = _runnerHolder.Runner;
            // 제거하면 명단에서 빠지면서 마지막 항목이 그 자리로 오므로, 끝에서부터 돈다.
            for (int barricadeIndex = _barricades.Count - 1; barricadeIndex >= 0; barricadeIndex--)
            {
                if (_barricades.Get(barricadeIndex) is BarricadeNetwork barricade && barricade.Object != null)
                {
                    runner.Despawn(barricade.Object);
                }
            }
        }
    }
}
