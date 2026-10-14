// 접속·끊기 횟수만 세는 테스트용 로비 서비스.
// 접속은 늘 성공한다.

using System.Threading;
using Cysharp.Threading.Tasks;
using ZD.Gameplay.Lobby;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeLobbyService : ILobbyService
    {
        public int ConnectCount { get; private set; }
        public int DisconnectCount { get; private set; }

        public UniTask<bool> ConnectAsync(CancellationToken token)
        {
            ConnectCount++;
            return UniTask.FromResult(true);
        }

        public UniTask DisconnectAsync()
        {
            DisconnectCount++;
            return UniTask.CompletedTask;
        }
    }
}
