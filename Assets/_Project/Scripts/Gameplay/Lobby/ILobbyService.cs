// 방 목록 서버.
// 접속해 있는 동안 LobbyModel에 공개된 방 목록을 계속 갱신한다.
// 게임에 들어갈 때 끊고(접속 수 절약), 로비로 돌아오면 다시 붙는다.

using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZD.Gameplay.Lobby
{
    public interface ILobbyService
    {
        UniTask<bool> ConnectAsync(CancellationToken token);

        UniTask DisconnectAsync();
    }
}
