// 죽음 상태.
// 움직이지 않는다.

namespace ZD.Gameplay.Zombies
{
    public sealed class ZombieDeadState : IZombieAIState
    {
        public void Enter(in ZombieTickContext context)
        {
            context.Motor.Stop();
        }

        public ZombieStateId Tick(in ZombieTickContext context) => ZombieStateId.Dead;
    }
}
