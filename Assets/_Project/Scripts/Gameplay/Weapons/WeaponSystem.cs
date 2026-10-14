// 무기 하나의 연사 속도, 탄약, 재장전을 처리한다.
// 판정과 피해는 무기의 전략이 맡아서 이 시스템은 무기 종류로 분기하지 않는다.

using System.Collections.Generic;
using UnityEngine;
using ZD.Core;
using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Weapons
{
    public sealed class WeaponSystem
    {
        private readonly INetworkClock _clock;
        private readonly IHitQuery _hitQuery;
        private readonly DamageSystem _damageSystem;
        private readonly Dictionary<WeaponDefinition, IWeaponBehavior> _behaviors = new();

        public WeaponSystem(INetworkClock clock, IHitQuery hitQuery, DamageSystem damageSystem)
        {
            _clock = clock;
            _hitQuery = hitQuery;
            _damageSystem = damageSystem;
        }

        public void Equip(IWeaponModel weapon, WeaponDefinition definition)
        {
            weapon.AmmoInMagazine = definition.UsesAmmo ? definition.MagazineSize : 0;
            weapon.ReserveAmmo = definition.UsesAmmo ? definition.StartingReserveAmmo : 0;
            weapon.NextFireTick = 0;
            weapon.ReloadEndTick = 0;
        }

        public bool Tick(IWeaponModel weapon, WeaponDefinition definition, int shooterId, in WeaponCommand command)
        {
            int tick = _clock.Tick;

            if (weapon.ReloadEndTick != 0)
            {
                if (tick < weapon.ReloadEndTick)
                {
                    return false;
                }

                FinishReload(weapon, definition);
            }

            if (command.ReloadPressed && CanReload(weapon, definition))
            {
                StartReload(weapon, definition, tick);
                return false;
            }

            bool wantsToFire = definition.IsAutomatic ? command.FireHeld : command.FirePressed;
            if (!wantsToFire || tick < weapon.NextFireTick)
            {
                return false;
            }

            if (definition.UsesAmmo && weapon.AmmoInMagazine <= 0)
            {
                if (CanReload(weapon, definition))
                {
                    StartReload(weapon, definition, tick);
                }

                return false;
            }

            if (definition.UsesAmmo)
            {
                weapon.AmmoInMagazine--;
            }

            weapon.NextFireTick = tick + Mathf.Max(1, _clock.SecondsToTicks(1f / definition.ShotsPerSecond));
            GetBehavior(definition).Fire(definition, shooterId, command, _hitQuery, _damageSystem);
            return true;
        }

        public void CancelReload(IWeaponModel weapon)
        {
            weapon.ReloadEndTick = 0;
        }

        private static bool CanReload(IWeaponModel weapon, WeaponDefinition definition)
        {
            return definition.UsesAmmo
                && weapon.ReloadEndTick == 0
                && weapon.ReserveAmmo > 0
                && weapon.AmmoInMagazine < definition.MagazineSize;
        }

        private void StartReload(IWeaponModel weapon, WeaponDefinition definition, int tick)
        {
            weapon.ReloadEndTick = tick + Mathf.Max(1, _clock.SecondsToTicks(definition.ReloadSeconds));
        }

        private static void FinishReload(IWeaponModel weapon, WeaponDefinition definition)
        {
            int needed = definition.MagazineSize - weapon.AmmoInMagazine;
            int loaded = Mathf.Min(needed, weapon.ReserveAmmo);
            weapon.AmmoInMagazine += loaded;
            weapon.ReserveAmmo -= loaded;
            weapon.ReloadEndTick = 0;
        }

        private IWeaponBehavior GetBehavior(WeaponDefinition definition)
        {
            if (!_behaviors.TryGetValue(definition, out IWeaponBehavior behavior))
            {
                behavior = definition.CreateBehavior();
                _behaviors.Add(definition, behavior);
            }

            return behavior;
        }
    }
}
