// 좀비가 노리는 대상의 종류.
// None이면 노릴 대상이 없어 멈춘다.

namespace ZD.Gameplay.Zombies
{
    public enum ZombieTargetKind : byte
    {
        None = 0,
        Player = 1,
        Base = 2,
    }
}
