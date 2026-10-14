// Gameplay 정의를 네트워크 프리팹에 연결한다.
// 그래서 Gameplay는 Fusion 타입을 참조하지 않는다.
// 프리팹을 직접 참조하지 않고 NetworkPrefabRef(GUID)만 가지므로, 프리팹은 Addressables 번들에서 불러온다.

using Fusion;
using UnityEngine;
using ZD.Gameplay.Building;
using ZD.Gameplay.Zombies;

namespace ZD.Network.Session
{
    [CreateAssetMenu(menuName = "ZD/Network Prefab Catalog")]
    public sealed class NetworkPrefabCatalog : ScriptableObject
    {
        private const int FIXED_PREFAB_COUNT = 3;

        [SerializeField] private NetworkPrefabRef _playerPrefab;
        [SerializeField] private NetworkPrefabRef _gameDirectorPrefab;
        [SerializeField] private NetworkPrefabRef _basePrefab;
        [SerializeField] private ZombiePrefabEntry[] _zombiePrefabs;
        [SerializeField] private BarricadePrefabEntry[] _barricadePrefabs;

        public NetworkPrefabRef PlayerPrefab => _playerPrefab;
        public NetworkPrefabRef GameDirectorPrefab => _gameDirectorPrefab;
        public NetworkPrefabRef BasePrefab => _basePrefab;

        public int PrefabCount => FIXED_PREFAB_COUNT + _zombiePrefabs.Length + _barricadePrefabs.Length;

        public NetworkPrefabRef GetPrefab(int index)
        {
            if (index == 0)
            {
                return _playerPrefab;
            }

            if (index == 1)
            {
                return _gameDirectorPrefab;
            }

            if (index == 2)
            {
                return _basePrefab;
            }

            int zombieIndex = index - FIXED_PREFAB_COUNT;
            return zombieIndex < _zombiePrefabs.Length
                ? _zombiePrefabs[zombieIndex].Prefab
                : _barricadePrefabs[zombieIndex - _zombiePrefabs.Length].Prefab;
        }

        public NetworkPrefabRef GetZombiePrefab(ZombieDefinition definition)
        {
            for (int entryIndex = 0; entryIndex < _zombiePrefabs.Length; entryIndex++)
            {
                if (_zombiePrefabs[entryIndex].Definition == definition)
                {
                    return _zombiePrefabs[entryIndex].Prefab;
                }
            }

            return default;
        }

        public NetworkPrefabRef GetBarricadePrefab(BarricadeDefinition definition)
        {
            for (int entryIndex = 0; entryIndex < _barricadePrefabs.Length; entryIndex++)
            {
                if (_barricadePrefabs[entryIndex].Definition == definition)
                {
                    return _barricadePrefabs[entryIndex].Prefab;
                }
            }

            return default;
        }
    }
}
