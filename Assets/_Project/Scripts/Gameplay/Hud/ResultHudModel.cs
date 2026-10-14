// 결과 화면의 로컬 표시용 값.
// 확정된 매치 결과를 ResultScreenSystem이 옮겨 쓰고, View가 구독한다.
// Rows는 앞에서부터 RowCount줄만 쓴다.
// Version이 오르면 줄을 다시 그린다.

using R3;
using ZD.Gameplay.Stats;

namespace ZD.Gameplay.Hud
{
    public sealed class ResultHudModel
    {
        public ReactiveProperty<int> Version { get; } = new(0);
        public ReactiveProperty<int> TeamScore { get; } = new(0);
        public ReactiveProperty<int> WavesCleared { get; } = new(0);
        public ReactiveProperty<int> BestScore { get; } = new(0);
        public ReactiveProperty<bool> IsNewBest { get; } = new(false);
        public ReactiveProperty<bool> IsBaseDestroyed { get; } = new(false);
        public int RowCount { get; set; }
        public PlayerResult[] Rows { get; } = new PlayerResult[MatchResultSystem.MAX_PLAYERS];
    }
}
