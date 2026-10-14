// 호스트 전용.
// 매치 디렉터를 한 번 스폰하고, 참가하는 피어마다 플레이어 오브젝트를 하나씩 스폰한다.

using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace ZD.Network.Session
{
    public sealed class PlayerSpawnService
    {
        private readonly NetworkPrefabCatalog _catalog;
        private readonly SpawnPointSet _spawnPoints;
        private readonly Dictionary<PlayerRef, NetworkObject> _players = new();
        private NetworkObject _director;
        private int _nextSpawnIndex;

        public PlayerSpawnService(NetworkPrefabCatalog catalog, SpawnPointSet spawnPoints)
        {
            _catalog = catalog;
            _spawnPoints = spawnPoints;
        }

        public void HandlePlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer)
            {
                return;
            }

            if (_director == null)
            {
                _director = runner.Spawn(_catalog.GameDirectorPrefab, Vector3.zero, Quaternion.identity);
            }

            Transform spawn = _spawnPoints.GetPlayerSpawn(_nextSpawnIndex++);
            NetworkObject playerObject = runner.Spawn(_catalog.PlayerPrefab, spawn.position, spawn.rotation, player);
            runner.SetPlayerObject(player, playerObject);
            _players[player] = playerObject;
        }

        public void HandlePlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer || !_players.TryGetValue(player, out NetworkObject playerObject))
            {
                return;
            }

            _players.Remove(player);
            if (playerObject != null)
            {
                runner.Despawn(playerObject);
            }
        }
    }
}
