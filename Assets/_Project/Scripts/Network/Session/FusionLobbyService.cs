// 방 목록 전용 Fusion 러너(Bootstrap 씬).
// 로비(ClientServer)에 붙어 있는 동안 공개된 방 목록을 LobbyModel에 옮긴다.
// 게임 러너와 따로라서, 게임에 들어갈 때 끊고 로비로 돌아오면 다시 붙는다.

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Lobby;

namespace ZD.Network.Session
{
    public sealed class FusionLobbyService : ILobbyService, INetworkRunnerCallbacks
    {
        private readonly LobbyModel _lobby;
        private NetworkRunner _runner;

        public FusionLobbyService(LobbyModel lobby)
        {
            _lobby = lobby;
        }

        public async UniTask<bool> ConnectAsync(CancellationToken token)
        {
            if (_runner != null)
            {
                return _lobby.Status.Value == LobbyStatus.Connected;
            }

            _lobby.Status.Value = LobbyStatus.Connecting;
            var runnerObject = new GameObject("LobbyRunner");
            _runner = runnerObject.AddComponent<NetworkRunner>();
            _runner.AddCallbacks(this);
            StartGameResult result = await _runner.JoinSessionLobby(SessionLobby.ClientServer, cancellationToken: token).AsUniTask();
            if (!result.Ok)
            {
                DestroyRunner();
                _lobby.Status.Value = LobbyStatus.Failed;
                return false;
            }

            _lobby.Status.Value = LobbyStatus.Connected;
            return true;
        }

        public async UniTask DisconnectAsync()
        {
            if (_runner == null)
            {
                return;
            }

            NetworkRunner runner = _runner;
            _runner = null;
            await runner.Shutdown().AsUniTask();
            ClearRooms();
            _lobby.Status.Value = LobbyStatus.Disconnected;
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            _lobby.Rooms.Clear();
            for (int sessionIndex = 0; sessionIndex < sessionList.Count; sessionIndex++)
            {
                SessionInfo session = sessionList[sessionIndex];
                if (session.IsValid && TryReadRoom(session, out RoomInfo room))
                {
                    _lobby.Rooms.Add(room);
                }
            }

            _lobby.RoomsVersion.Value++;
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (runner != _runner)
            {
                return;
            }

            _runner = null;
            ClearRooms();
            _lobby.Status.Value = LobbyStatus.Disconnected;
        }

        private static bool TryReadRoom(SessionInfo session, out RoomInfo room)
        {
            room = default;
            IReadOnlyDictionary<string, SessionProperty> properties = session.Properties;
            if (properties == null
                || !properties.TryGetValue(SessionProperties.ROOM_NAME, out SessionProperty name)
                || !properties.TryGetValue(SessionProperties.MODE, out SessionProperty mode))
            {
                return false;
            }

            string hostName = properties.TryGetValue(SessionProperties.HOST_NAME, out SessionProperty host) ? (string)host : string.Empty;
            bool isLocked = properties.TryGetValue(SessionProperties.LOCKED, out SessionProperty locked) && (bool)locked;
            room = new RoomInfo(session.Name, name, hostName, (GameModeId)(int)mode, session.PlayerCount, session.MaxPlayers, isLocked, session.IsOpen);
            return true;
        }

        private void DestroyRunner()
        {
            if (_runner != null)
            {
                UnityEngine.Object.Destroy(_runner.gameObject);
                _runner = null;
            }
        }

        private void ClearRooms()
        {
            _lobby.Rooms.Clear();
            _lobby.RoomsVersion.Value++;
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}
