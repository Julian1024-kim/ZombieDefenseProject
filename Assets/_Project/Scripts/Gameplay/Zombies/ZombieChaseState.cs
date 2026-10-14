// 추적 상태.
// 목표(플레이어나 거점)를 따라가다 공격 범위에 들어오면 공격으로 넘어간다.
// 방어막에 길이 완전히 막혔으면 막힌 경로 끝(벽 앞)까지 가서, 가장 가까운 벽이 닿으면 벽 공격으로 넘어간다.

namespace ZD.Gameplay.Zombies
{
    public sealed class ZombieChaseState : IZombieAIState
    {
        public void Enter(in ZombieTickContext context) { }

        public ZombieStateId Tick(in ZombieTickContext context)
        {
            if (context.Target.IsNone)
            {
                context.Motor.Stop();
                return ZombieStateId.Chase;
            }

            if (context.Barricade != null)
            {
                float reach = context.Zombie.Definition.AttackRange;
                if (context.SqrDistanceToBarricade <= reach * reach)
                {
                    return ZombieStateId.AttackBarricade;
                }

                // 막힌 경로도 갈 수 있는 데까지는 이어진다(벽 바로 앞). 그래서 목표는 그대로 두고 따라간다.
                context.Motor.MoveTowards(context.Target.MovePosition, context.Zombie.Definition.MoveSpeed);
                return ZombieStateId.Chase;
            }

            float attackRange = context.Zombie.Definition.AttackRange;
            if (context.Target.SqrDistance <= attackRange * attackRange)
            {
                return ZombieStateId.Attack;
            }

            context.Motor.MoveTowards(context.Target.MovePosition, context.Zombie.Definition.MoveSpeed);
            return ZombieStateId.Chase;
        }
    }
}
