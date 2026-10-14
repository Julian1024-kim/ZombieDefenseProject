// 지금 세션의 매치 상태(복제된 게임 흐름 모델)를 다른 시스템이 읽게 해 준다.
// 디렉터가 스폰될 때 넣고 사라질 때 뺀다.
// 게임 오버면 플레이어 행동·구매·설치를 막고, 대기 중이면 웨이브 없이 기다린다.

namespace ZD.Gameplay.GameFlow
{
    public sealed class MatchStateModel
    {
        public IGameFlowModel Flow { get; set; }

        public bool IsGameOver => Flow != null && Flow.Phase == GamePhase.GameOver;
    }
}
