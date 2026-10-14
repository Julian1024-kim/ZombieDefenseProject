// 게임 모드는 이 조건들의 목록이고, 하나라도 충족되면 게임이 끝난다.

namespace ZD.Gameplay.GameFlow
{
    public interface IDefeatCondition
    {
        bool IsMet();
    }
}
