// 네트워크 세션(방)을 만들거나 참가하거나 나간다.
// 호스트는 방을 만들고 참가 요청의 비밀번호를 확인하며, 참가자는 방 코드와 비밀번호로 들어간다.

using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZD.Core
{
    public interface ISessionService
    {
        UniTask<SessionStartResult> StartAsync(SessionStartOptions options, CancellationToken token);

        UniTask ShutdownAsync();
    }
}
