// Fusion 러너 콜백: 틱마다 로컬 입력을 한 번 담고, 참가/퇴장을 스폰 서비스에 전달한다.
// 호스트는 참가 요청의 비밀번호(접속 토큰)를 확인해 틀리면 거절한다.

using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using ZD.Gameplay.Hud;
using ZD.Gameplay.Players;
using ZD.Network.Input;

namespace ZD.Network.Session
{
    public sealed class SessionCallbacks : INetworkRunnerCallbacks
    {
        private readonly LocalInputModel _localInput;
        private readonly LocalInputSystem _localInputSystem;
        private readonly PlayerSpawnService _playerSpawnService;
        private readonly NetworkRunnerHolder _runnerHolder;
        private readonly SessionStatusModel _status;
        private readonly SessionSecurityModel _security;

        public SessionCallbacks(
            LocalInputModel localInput,
            LocalInputSystem localInputSystem,
            PlayerSpawnService playerSpawnService,
            NetworkRunnerHolder runnerHolder,
            SessionStatusModel status,
            SessionSecurityModel security)
        {
            _security = security;
            _localInput = localInput;
            _localInputSystem = localInputSystem;
            _playerSpawnService = playerSpawnService;
            _runnerHolder = runnerHolder;
            _status = status;
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();
            data.SetMove(_localInput.Move);
            data.SetLook(_localInput.Yaw, _localInput.Pitch);
            data.Buttons.Set(InputButton.Fire, _localInputSystem.IsFireDown());
            data.Buttons.Set(InputButton.Reload, _localInputSystem.IsReloadDown());
            data.Buttons.Set(InputButton.Restart, _localInputSystem.IsRestartDown());
            data.Buttons.Set(InputButton.Revive, _localInputSystem.IsReviveHeld());
            data.RequestedSlot = (byte)_localInputSystem.RequestedSlot;
            data.SlotSequence = _localInputSystem.SlotRequestSequence;
            input.Set(data);
            _localInputSystem.AdvanceTick();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) => _playerSpawnService.HandlePlayerJoined(runner, player);

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) => _playerSpawnService.HandlePlayerLeft(runner, player);

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (_runnerHolder.Runner == runner)
            {
                _runnerHolder.Runner = null;
            }

            if (_status.Status.Value == SessionStatus.Connected)
            {
                _status.Status.Value = SessionStatus.Disconnected;
            }
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            if (_security.Accepts(token))
            {
                request.Accept();
            }
            else
            {
                request.Refuse();
            }
        }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}
