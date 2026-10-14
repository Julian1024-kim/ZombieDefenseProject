// 설치 가능 여부와 안 되는 이유.
// 미리보기 색과 거절 안내에 쓴다.

namespace ZD.Gameplay.Building
{
    public enum PlacementResult : byte
    {
        Valid = 0,
        NoGround = 1,
        TooFar = 2,
        TooSteep = 3,
        Blocked = 4,
        NotEnoughPoints = 5,
        Unavailable = 6,
    }
}
