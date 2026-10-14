// 곰덫.
// 발동 반경에 들어온, 묶여 있지 않은 좀비 하나를 정해진 시간 동안 묶고 피해를 준다.
// 묶는 동안은 다음 좀비를 잡지 않고, 정해진 횟수를 다 쓰면 마지막 묶기가 끝난 뒤 부서진다.

using UnityEngine;
using ZD.Core;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Zombies;

namespace ZD.Gameplay.Building
{
    public sealed class BearTrapBehavior : IBarricadeBehavior
    {
        private readonly INetworkClock _clock;
        private readonly DamageSystem _damageSystem;

        public BearTrapBehavior(INetworkClock clock, DamageSystem damageSystem)
        {
            _clock = clock;
            _damageSystem = damageSystem;
        }

        public void Tick(IBarricadeModel barricade, in BarricadeTickContext context)
        {
            if (context.Tick < barricade.BusyUntilTick)
            {
                return;
            }

            if (barricade.UsesLeft <= 0)
            {
                barricade.Health = 0;
                return;
            }

            IZombieModel target = FindFreeZombie(context, barricade);
            if (target == null)
            {
                return;
            }

            BarricadeDefinition definition = barricade.Definition;
            int holdTicks = Mathf.Max(1, _clock.SecondsToTicks(definition.HoldSeconds));
            target.HeldUntilTick = context.Tick + holdTicks;
            _damageSystem.ApplyDamage(target, new DamageInfo(definition.TrapDamage, HitZone.Legs, barricade.BuilderId));
            barricade.UsesLeft--;
            barricade.BusyUntilTick = context.Tick + holdTicks;
        }

        private static IZombieModel FindFreeZombie(in BarricadeTickContext context, IBarricadeModel barricade)
        {
            float radius = barricade.Definition.TriggerRadius;
            float nearestSqrDistance = radius * radius;
            IZombieModel nearest = null;
            for (int zombieIndex = 0; zombieIndex < context.Zombies.Count; zombieIndex++)
            {
                IZombieModel zombie = context.Zombies.GetModel(zombieIndex);
                if (zombie.Health <= 0 || context.Tick < zombie.HeldUntilTick)
                {
                    continue;
                }

                float sqrDistance = (zombie.Position - barricade.Position).sqrMagnitude;
                if (sqrDistance <= nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearest = zombie;
                }
            }

            return nearest;
        }
    }
}
