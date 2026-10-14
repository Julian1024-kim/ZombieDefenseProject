// 세션을 시작하기 전에 카탈로그의 네트워크 프리팹을 Addressables에서 비동기로 미리 불러온다.
// 그래서 호스트의 Spawn이 메인 스레드를 막는 동기 로드를 하지 않는다.
// 불러온 프리팹은 Dispose에서 해제한다.

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;

namespace ZD.Network.Session
{
    public sealed class NetworkPrefabPreloader : IDisposable
    {
        private readonly NetworkPrefabCatalog _catalog;
        private readonly List<NetworkPrefabId> _loadedIds = new();
        private readonly CancellationTokenSource _cts = new();

        public NetworkPrefabPreloader(NetworkPrefabCatalog catalog)
        {
            _catalog = catalog;
        }

        public async UniTask PreloadAsync(CancellationToken token)
        {
            using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token);
            NetworkPrefabTable table = NetworkProjectConfig.Global.PrefabTable;
            for (int prefabIndex = 0; prefabIndex < _catalog.PrefabCount; prefabIndex++)
            {
                NetworkPrefabRef prefab = _catalog.GetPrefab(prefabIndex);
                if (!prefab.IsValid)
                {
                    continue;
                }

                NetworkPrefabId id = table.GetId((NetworkObjectGuid)prefab);
                if (!id.IsValid || _loadedIds.Contains(id))
                {
                    continue;
                }

                // isSynchronous false: 로드를 시작만 하고, 끝날 때까지는 null을 돌려준다.
                while (table.Load(id, isSynchronous: false) == null)
                {
                    await UniTask.Yield(linked.Token);
                }

                _loadedIds.Add(id);
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            NetworkPrefabTable table = NetworkProjectConfig.Global.PrefabTable;
            for (int idIndex = 0; idIndex < _loadedIds.Count; idIndex++)
            {
                table.Unload(_loadedIds[idIndex]);
            }

            _loadedIds.Clear();
        }
    }
}
