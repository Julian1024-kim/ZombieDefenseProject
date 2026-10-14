// Fusion 세션을 시작한다.
// 방 만들기는 Host로, 방 코드가 세션 이름이고 방 이름·방장·모드·잠금은 목록용 속성으로 올린다.
// 참가는 Client로, 비밀번호는 방 코드와 같이 해시해서 접속 토큰으로 보낸다.
// 개발용은 AutoHostOrClient(먼저 온 사람이 호스트).
// 실패하면 이유(방 없음, 꽉 참, 비밀번호 틀림)를 접속 상태에 남긴다.

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using VContainer;
using ZD.Core;
using ZD.Gameplay.Hud;

namespace ZD.Network.Session
{
    public sealed class FusionSessionService : ISessionService
    {
        private const int MAX_PLAYERS = 4;

        private readonly NetworkRunnerHolder _runnerHolder;
        private readonly SessionCallbacks _callbacks;
        private readonly IObjectResolver _resolver;
        private readonly SessionStatusModel _status;
        private readonly NetworkPrefabPreloader _prefabPreloader;
        private readonly SessionSecurityModel _security;

        public FusionSessionService(
            NetworkRunnerHolder runnerHolder,
            SessionCallbacks callbacks,
            IObjectResolver resolver,
            SessionStatusModel status,
            NetworkPrefabPreloader prefabPreloader,
            SessionSecurityModel security)
        {
            _runnerHolder = runnerHolder;
            _callbacks = callbacks;
            _resolver = resolver;
            _status = status;
            _prefabPreloader = prefabPreloader;
            _security = security;
        }

        public async UniTask<SessionStartResult> StartAsync(SessionStartOptions options, CancellationToken token)
        {
            _status.Status.Value = SessionStatus.Connecting;
            _security.SetPassword(options.SessionName, options.Role == SessionRole.Host ? options.Password : string.Empty);
            await _prefabPreloader.PreloadAsync(token);

            var runnerObject = new GameObject("NetworkRunner");
            NetworkRunner runner = runnerObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;
            runner.AddCallbacks(_callbacks);
            InjectingObjectProvider objectProvider = runnerObject.AddComponent<InjectingObjectProvider>();
            objectProvider.SetResolver(_resolver);
            _runnerHolder.Runner = runner;

            StartGameResult result = await runner.StartGame(CreateArgs(options, objectProvider, token)).AsUniTask();
            if (!result.Ok)
            {
                _runnerHolder.Runner = null;
                Object.Destroy(runnerObject);
                _status.FailureReason = ToFailure(result.ShutdownReason);
                _status.Status.Value = SessionStatus.Failed;
                return _status.FailureReason;
            }

            _status.RoomCode = runner.SessionInfo.IsValid ? runner.SessionInfo.Name : options.SessionName;
            _status.IsHost = runner.IsServer;
            _status.Status.Value = SessionStatus.Connected;
            return SessionStartResult.Ok;
        }

        public async UniTask ShutdownAsync()
        {
            NetworkRunner runner = _runnerHolder.Runner;
            if (runner == null)
            {
                return;
            }

            _runnerHolder.Runner = null;
            await runner.Shutdown().AsUniTask();
        }

        private static StartGameArgs CreateArgs(in SessionStartOptions options, INetworkObjectProvider objectProvider, CancellationToken token)
        {
            var args = new StartGameArgs
            {
                GameMode = ToGameMode(options.Role),
                SessionName = options.SessionName,
                PlayerCount = MAX_PLAYERS,
                ObjectProvider = objectProvider,
                StartGameCancellationToken = token,
            };

            if (options.Role == SessionRole.Host)
            {
                args.IsVisible = options.IsVisible;
                args.SessionProperties = new Dictionary<string, SessionProperty>
                {
                    { SessionProperties.ROOM_NAME, options.RoomName },
                    { SessionProperties.HOST_NAME, options.HostName },
                    { SessionProperties.MODE, options.ModeIndex },
                    { SessionProperties.LOCKED, options.IsLocked },
                };
            }
            else if (options.Role == SessionRole.Client)
            {
                args.ConnectionToken = SessionProperties.ToToken(options.SessionName, options.Password);
            }

            return args;
        }

        private static GameMode ToGameMode(SessionRole role)
        {
            return role switch
            {
                SessionRole.Host => GameMode.Host,
                SessionRole.Client => GameMode.Client,
                _ => GameMode.AutoHostOrClient,
            };
        }

        private static SessionStartResult ToFailure(ShutdownReason reason)
        {
            return reason switch
            {
                ShutdownReason.GameNotFound => SessionStartResult.RoomNotFound,
                ShutdownReason.GameClosed => SessionStartResult.RoomNotFound,
                ShutdownReason.GameIsFull => SessionStartResult.RoomFull,
                ShutdownReason.ConnectionRefused => SessionStartResult.WrongPassword,
                _ => SessionStartResult.Failed,
            };
        }
    }
}
