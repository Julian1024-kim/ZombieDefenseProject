// 공격 상태.
// 멈춰서 목표(플레이어나 거점)를 바라보고, 공격 간격마다 피해를 준다.

using UnityEngine;
using ZD.Core;
using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Zombies
{
    public sealed class ZombieAttackState : IZombieAIState
    {
        // 목표가 공격 범위보다 이만큼 더 멀어져야 공격을 그만둔다. 그래야 좀비가 상태 사이를 오가며 떨지 않는다.
        private const float LEAVE_RANGE_MULTIPLIER = 1.2f;

        private readonly INetworkClock _clock;
        private readonly DamageSystem _damageSystem;

        public ZombieAttackState(INetworkClock clock, DamageSystem damageSystem)
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
            // 길이 막혔으면 벽 너머의 목표는 때릴 수 없다. 추적 상태가 벽으로 데려간다.
            if (context.Target.IsNone || context.Barricade != null)
            {
                return ZombieStateId.Chase;
            }

            ZombieDefinition definition = context.Zombie.Definition;
            float leaveRange = definition.AttackRange * LEAVE_RANGE_MULTIPLIER;
            if (context.Target.SqrDistance > leaveRange * leaveRange)
            {
                return ZombieStateId.Chase;
            }

            context.Motor.FaceTowards(context.Target.AimPosition);
            if (context.Tick < context.Zombie.NextAttackTick)
            {
                return ZombieStateId.Attack;
            }

            _damageSystem.ApplyDamage(context.Target.Damageable, new DamageInfo(definition.AttackDamage, HitZone.Body, context.Zombie.EntityId));
            context.Zombie.NextAttackTick = context.Tick + Mathf.Max(1, _clock.SecondsToTicks(definition.AttackIntervalSeconds));
            return ZombieStateId.Attack;
        }
    }
}
