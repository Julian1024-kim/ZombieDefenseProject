// 벽 공격 상태.
// 길을 막은 벽 앞에서 멈춰 벽을 바라보고, 공격 간격마다 피해를 준다.
// 벽이 부서지거나 길이 다시 열리면(Barricade가 null) 추적으로 돌아간다.

using UnityEngine;
using ZD.Core;
using ZD.Gameplay.Building;
using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Zombies
{
    public sealed class ZombieAttackBarricadeState : IZombieAIState
    {
        // 벽이 공격 범위보다 이만큼 더 멀어져야 그만둔다. 상태 사이를 오가며 떨지 않게 한다.
        private const float LEAVE_RANGE_MULTIPLIER = 1.2f;

        private readonly INetworkClock _clock;
        private readonly DamageSystem _damageSystem;

        public ZombieAttackBarricadeState(INetworkClock clock, DamageSystem damageSystem)
        {
            _clock = clock;
            _damageSystem = damageSystem;
        }

        public void Enter(in ZombieTickContext context)
        {
            context.Motor.Stop();
            int windUpTicks = _clock.SecondsToTicks(context.Zombie.Definition.AttackIntervalSeconds * 0.5f);
            context.Zombie.NextAttackTick = Mathf.Max(context.Zombie.NextAttackTick, context.Tick + windUpTicks);
        }

        public ZombieStateId Tick(in ZombieTickContext context)
        {
            IBarricadeModel barricade = context.Barricade;
            ZombieDefinition definition = context.Zombie.Definition;
            float leaveRange = definition.AttackRange * LEAVE_RANGE_MULTIPLIER;
            if (barricade == null || context.SqrDistanceToBarricade > leaveRange * leaveRange)
            {
                return ZombieStateId.Chase;
            }

            context.Motor.FaceTowards(barricade.Position);
            if (context.Tick < context.Zombie.NextAttackTick)
            {
                return ZombieStateId.AttackBarricade;
            }

            _damageSystem.ApplyDamage(barricade, new DamageInfo(definition.AttackDamage, HitZone.Body, context.Zombie.EntityId));
            context.Zombie.NextAttackTick = context.Tick + Mathf.Max(1, _clock.SecondsToTicks(definition.AttackIntervalSeconds));
            return ZombieStateId.AttackBarricade;
        }
    }
}
