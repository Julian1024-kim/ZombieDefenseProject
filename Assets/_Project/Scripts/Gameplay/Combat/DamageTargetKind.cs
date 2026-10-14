// 피해 대상의 종류(플레이어, 좀비, 방어막).

namespace ZD.Gameplay.Combat
{
    public enum DamageTargetKind : byte
    {
        Player = 0,
        Zombie = 1,
        Barricade = 2,
        Base = 3,
    }
}
