// 실행 중인 NetworkRunner.
// 세션이 시작되면 설정되고, 그 전에는 null이다.

using Fusion;

namespace ZD.Network.Session
{
    public sealed class NetworkRunnerHolder
    {
        public NetworkRunner Runner { get; set; }
    }
}
