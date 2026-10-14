// 좀비 상태가 한 틱에 필요한 모든 것.
// 노릴 대상(플레이어나 거점)이 없으면 Target이 None이다.
// Barricade는 목표까지 길이 막혔거나 가까운 목표와의 사이를 벽이 가로막을 때만 그 벽이고, 아니면 null이다.

using ZD.Gameplay.Building;

namespace ZD.Gameplay.Zombies
{
    public readonly struct ZombieTickContext
    {
        public readonly IZombieModel Zombie;
        public readonly IZombieMotor Motor;
        public readonly ZombieTarget Target;
        public readonly int Tick;
        public readonly IBarricadeModel Barricade;
        public readonly float SqrDistanceToBarricade;

        public ZombieTickContext(IZombieModel zombie, IZombieMotor motor, in ZombieTarget target, int tick, IBarricadeModel barricade, float sqrDistanceToBarricade)
        {
            Zombie = zombie;
            Motor = motor;
            Target = target;
            Tick = tick;
            Barricade = barricade;
            SqrDistanceToBarricade = sqrDistanceToBarricade;
        }
    }
}
