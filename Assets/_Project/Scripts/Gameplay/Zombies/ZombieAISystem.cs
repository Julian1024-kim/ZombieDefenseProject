// 호스트에서 모든 좀비를 루프 하나로 갱신한다(좀비마다 Update를 두지 않는 중앙 갱신).
// 상태 객체를 미리 만들어 ZombieStateId로 꺼내 쓰므로 상태 전환에 할당이 없다.
// 목표(플레이어나 거점)는 모드가 정한 고르기 방식으로 정하고, 곰덫에 묶인 좀비는 멈춰 있고, 목표까지 길이 막힌 좀비에게는 가장 가까운 완성된 벽을 함께 넘긴다.

using Unity.Profiling;
using ZD.Core;
using ZD.Gameplay.Building;
using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Zombies
{
    public sealed class ZombieAISystem
    {
        // 설계 문서의 성능 예산(좀비 100마리 1.5ms)을 Profiler에서 이 이름으로 잰다.
        private static readonly ProfilerMarker TickMarker = new("ZombieAI.Tick");

        // 목표가 공격 범위의 이 배수 안에 있을 때만 사이에 벽이 있는지 직접 본다.
        private const float CLOSE_RANGE_MULTIPLIER = 2f;

        private readonly INetworkClock _clock;
        private readonly ZombieRosterModel _zombies;
        private readonly BarricadeRosterModel _barricades;
        private readonly IZombieAIState[] _states;

        public ZombieAISystem(INetworkClock clock, ZombieRosterModel zombies, BarricadeRosterModel barricades, DamageSystem damageSystem)
        {
            _clock = clock;
            _zombies = zombies;
            _barricades = barricades;
            _states = new IZombieAIState[]
            {
                new ZombieChaseState(),
                new ZombieAttackState(clock, damageSystem),
                new ZombieDeadState(),
                new ZombieAttackBarricadeState(clock, damageSystem),
            };
        }

        public void Tick(ITargetSelector targetSelector)
        {
            using (TickMarker.Auto())
            {
                int tick = _clock.Tick;
                for (int zombieIndex = 0; zombieIndex < _zombies.Count; zombieIndex++)
                {
                    TickZombie(_zombies.GetModel(zombieIndex), _zombies.GetMotor(zombieIndex), tick, targetSelector);
                }
            }
        }

        private void TickZombie(IZombieModel zombie, IZombieMotor motor, int tick, ITargetSelector targetSelector)
        {
            if (zombie.State == ZombieStateId.Dead)
            {
                return;
            }

            ZombieTarget target = targetSelector.Select(zombie);
            // 가까운 목표와의 사이를 벽이 직접 가로막으면 그 벽을(벽 너머로 때리지 않게), 아니면 경로가 막혔을 때 가장 가까운 벽을 넘긴다.
            // 멀리 있는 목표는 벽을 돌아갈 수 있으니 직선 검사를 하지 않는다.
            IBarricadeModel barricade = null;
            float sqrDistanceToBarricade = float.MaxValue;
            if (!target.IsNone)
            {
                float closeRange = zombie.Definition.AttackRange * CLOSE_RANGE_MULTIPLIER;
                if (target.SqrDistance <= closeRange * closeRange)
                {
                    barricade = _barricades.FindWallBetween(zombie.Position, target.MovePosition, out sqrDistanceToBarricade);
                }

                if (barricade == null && motor.IsPathBlocked)
                {
                    barricade = _barricades.FindNearestActiveWall(zombie.Position, out sqrDistanceToBarricade);
                }
            }

            var context = new ZombieTickContext(zombie, motor, target, tick, barricade, sqrDistanceToBarricade);

            if (zombie.Health <= 0)
            {
                ChangeState(ZombieStateId.Dead, context);
                return;
            }

            if (tick < zombie.HeldUntilTick)
            {
                motor.Stop();
                return;
            }

            ZombieStateId next = _states[(int)zombie.State].Tick(context);
            if (next != zombie.State)
            {
                ChangeState(next, context);
            }
        }

        public void StopAll()
        {
            for (int zombieIndex = 0; zombieIndex < _zombies.Count; zombieIndex++)
            {
                IZombieModel zombie = _zombies.GetModel(zombieIndex);
                if (zombie.State == ZombieStateId.Dead)
                {
                    continue;
                }

                _zombies.GetMotor(zombieIndex).Stop();
                zombie.State = ZombieStateId.Chase;
            }
        }

        private void ChangeState(ZombieStateId next, in ZombieTickContext context)
        {
            context.Zombie.State = next;
            _states[(int)next].Enter(context);
        }
    }
}
