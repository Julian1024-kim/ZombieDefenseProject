// 방어막의 동작 방식.
// 벽(길을 막고 좀비가 부숨), 지뢰(밟으면 폭발), 곰덫(밟은 좀비를 묶음).

namespace ZD.Gameplay.Building
{
    public enum BarricadeKind : byte
    {
        Wall = 0,
        Mine = 1,
        BearTrap = 2,
    }
}
