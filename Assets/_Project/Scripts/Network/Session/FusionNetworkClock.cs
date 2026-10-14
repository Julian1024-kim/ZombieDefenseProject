// Fusion 틱을 Gameplay에 제공한다.
// 재시뮬레이션 중에는 Runner.Tick이 다시 계산 중인 틱이다.

using UnityEngine;
using ZD.Core;

namespace ZD.Network.Session
{
    public sealed class FusionNetworkClock : INetworkClock
    {
        private readonly NetworkRunnerHolder _runnerHolder;

        public FusionNetworkClock(NetworkRunnerHolder runnerHolder)
        {
            _runnerHolder = runnerHolder;
        }

        public int Tick => _runnerHolder.Runner.Tick.Raw;
        public float DeltaTime => _runnerHolder.Runner.DeltaTime;

        public int SecondsToTicks(float seconds) => Mathf.CeilToInt(seconds / _runnerHolder.Runner.DeltaTime);
    }
}
