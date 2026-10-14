// 조준 방향으로 레이 한 발을 쏘는 즉시 판정 무기(권총, 소총).

using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Weapons
{
    public sealed class HitscanWeaponBehavior : IWeaponBehavior
    {
        public void Fire(WeaponDefinition definition, int shooterId, in WeaponCommand command, IHitQuery hitQuery, DamageSystem damageSystem)
        {
            if (!hitQuery.Raycast(shooterId, command.AimOrigin, command.AimDirection, definition.Range, out HitResult hit))
            {
                return;
            }

            damageSystem.ApplyDamage(hit.Target, new DamageInfo(definition.GetDamage(hit.Zone), hit.Zone, shooterId));
        }
    }
}
