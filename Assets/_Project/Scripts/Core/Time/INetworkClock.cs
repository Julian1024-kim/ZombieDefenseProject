// 시뮬레이션 시계 계약.
// 네트워크 계층이 Fusion 틱으로 구현한다.

namespace ZD.Core
{
    public interface INetworkClock
    {
        int Tick { get; }
        float DeltaTime { get; }
        int SecondsToTicks(float seconds);
    }
}
