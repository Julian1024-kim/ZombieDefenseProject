// 지뢰.
// 살아 있는 좀비가 발동 반경에 들어오면 폭발 반경 안의 모든 좀비에게 피해를 주고 사라진다(1회용).
// 처치 보상은 설치한 플레이어가 받는다.

using ZD.Gameplay.Combat;
using ZD.Gameplay.Zombies;

namespace ZD.Gameplay.Building
{
    public sealed class MineBehavior : IBarricadeBehavior
    {
        private readonly DamageSystem _damageSystem;

        public MineBehavior(DamageSystem damageSystem)
        {
            _damageSystem = damageSystem;
        }

        public void Tick(IBarricadeModel barricade, in BarricadeTickContext context)
        {
            BarricadeDefinition definition = barricade.Definition;
            if (!HasZombieWithin(context.Zombies, barricade, definition.TriggerRadius))
            {
                return;
            }

            float sqrEffectRadius = definition.EffectRadius * definition.EffectRadius;
            for (int zombieIndex = 0; zombieIndex < context.Zombies.Count; zombieIndex++)
            {
                IZombieModel zombie = context.Zombies.GetModel(zombieIndex);
                if (zombie.Health > 0 && (zombie.Position - barricade.Position).sqrMagnitude <= sqrEffectRadius)
                {
                    _damageSystem.ApplyDamage(zombie, new DamageInfo(definition.TrapDamage, HitZone.Body, barricade.BuilderId));
                }
            }

            barricade.Health = 0;
        }

        private static bool HasZombieWithin(ZombieRosterModel zombies, IBarricadeModel barricade, float radius)
        {
            float sqrRadius = radius * radius;
            for (int zombieIndex = 0; zombieIndex < zombies.Count; zombieIndex++)
            {
                IZombieModel zombie = zombies.GetModel(zombieIndex);
                if (zombie.Health > 0 && (zombie.Position - barricade.Position).sqrMagnitude <= sqrRadius)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
