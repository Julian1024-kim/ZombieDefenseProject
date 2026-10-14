// 씬에 놓인 스폰 위치(플레이어, 좀비, 거점).
// 데이터만 갖고, 스포너가 여기서 고른다.

using UnityEngine;

namespace ZD.Network.Session
{
    [DisallowMultipleComponent]
    public sealed class SpawnPointSet : MonoBehaviour
    {
        [SerializeField] private Transform[] _playerSpawnPoints;
        [SerializeField] private Transform[] _zombieSpawnPoints;
        [SerializeField] private Transform _baseSpawnPoint;

        public int PlayerSpawnCount => _playerSpawnPoints.Length;
        public int ZombieSpawnCount => _zombieSpawnPoints.Length;
        public Transform BaseSpawnPoint => _baseSpawnPoint;

        public Transform GetPlayerSpawn(int index) => _playerSpawnPoints[index % _playerSpawnPoints.Length];

        public Transform GetZombieSpawn(int index) => _zombieSpawnPoints[index % _zombieSpawnPoints.Length];
    }
}
