// 매치 상태의 로컬 표시용 복사본.
// 복제된 값이 바뀌면 네트워크 계층이 쓰고, View가 구독한다.

using R3;
using ZD.Gameplay.GameFlow;

namespace ZD.Gameplay.Hud
{
    public sealed class MatchHudModel
    {
        public ReactiveProperty<GameModeId> Mode { get; } = new(GameModeId.Survival);
        public ReactiveProperty<GamePhase> Phase { get; } = new(GamePhase.Preparation);
        public ReactiveProperty<int> WaveNumber { get; } = new(0);
        public ReactiveProperty<int> SecondsRemaining { get; } = new(0);
        public ReactiveProperty<int> ZombiesAlive { get; } = new(0);
        public ReactiveProperty<int> PlayerCount { get; } = new(0);
    }
}
