// 모드별 패배 규칙.
// 게임 흐름은 이 계약만 보고, 어떤 조건을 묶을지는 GameModeSystem 한 곳이 정한다.

namespace ZD.Gameplay.GameFlow
{
    public interface IGameModeRules
    {
        bool IsDefeated(GameModeId mode);
    }
}
