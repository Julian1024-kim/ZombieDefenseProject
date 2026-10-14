// 매치 상태의 모델 계약.
// [Networked] 값으로 구현되어 모든 클라이언트가 모드, 단계, 웨이브, 게임 오버 결과를 본다.

using ZD.Gameplay.Stats;

namespace ZD.Gameplay.GameFlow
{
    public interface IGameFlowModel
    {
        GameModeId Mode { get; set; }
        GamePhase Phase { get; set; }
        int PhaseEndTick { get; set; }
        int WaveNumber { get; set; }
        IMatchResultModel Result { get; }
    }
}
