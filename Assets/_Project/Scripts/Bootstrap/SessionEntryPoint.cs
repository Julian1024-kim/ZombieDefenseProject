// Game 씬에 들어가면 로비에서 정한 방에 접속한다: 방 만들기면 호스트(방 코드, 방 이름, 방장 닉네임, 모드, 비밀번호), 참가면 클라이언트(방 코드, 비밀번호).
// 개발용(명령줄·Game 씬 단독)은 모드별 고정 이름 세션에 먼저 온 사람이 호스트가 된다.

using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using ZD.Core;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Lobby;

namespace ZD.Bootstrap
{
    public sealed class SessionEntryPoint : IAsyncStartable
    {
        // GameModeId 순서와 같다. 개발용 세션은 방 목록 속성이 없어 로비에 보이지 않는다.
        private static readonly string[] DevSessionNames = { "zd-dev-survival", "zd-dev-base" };

        private readonly ISessionService _sessionService;
        private readonly ModeSelectionModel _modeSelection;
        private readonly SessionRequestModel _request;
        private readonly PlayerProfileModel _profile;

        public SessionEntryPoint(ISessionService sessionService, ModeSelectionModel modeSelection, SessionRequestModel request, PlayerProfileModel profile)
        {
            _sessionService = sessionService;
            _modeSelection = modeSelection;
            _request = request;
            _profile = profile;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            await _sessionService.StartAsync(CreateOptions(), cancellation);
        }

        private SessionStartOptions CreateOptions()
        {
            int mode = (int)_modeSelection.Mode;
            return _request.Kind switch
            {
                SessionRequestKind.Host => new SessionStartOptions(
                    SessionRole.Host, _request.RoomCode, !_request.IsPrivate, _request.RoomName, _profile.Nickname.Value, mode, _request.Password),
                SessionRequestKind.Join => new SessionStartOptions(
                    SessionRole.Client, _request.RoomCode, true, string.Empty, string.Empty, mode, _request.Password),
                _ => new SessionStartOptions(
                    SessionRole.Auto, DevSessionNames[mode], false, string.Empty, string.Empty, mode, string.Empty),
            };
        }
    }
}
