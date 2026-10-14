// 클라이언트가 애니메이션을 정할 수 있게 byte로 동기화된다.

namespace ZD.Gameplay.Zombies
{
    public enum ZombieStateId : byte
    {
        Chase = 0,
        Attack = 1,
        Dead = 2,
        AttackBarricade = 3,
    }
}
