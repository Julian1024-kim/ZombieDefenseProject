// 게임 모드.
// 모드마다 세션이 따로 있고, 패배 조건과 좀비 목표 선정이 다르다.

namespace ZD.Gameplay.GameFlow
{
    public enum GameModeId : byte
    {
        Survival = 0,
        BaseDefense = 1,
    }
}
