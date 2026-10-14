// 공격자 앞의 구 안에 있는 모든 대상을 때린다.
// 한 번 휘두를 때 대상마다 최대 한 번.

using System;
using UnityEngine;
using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Weapons
{
    public sealed class MeleeWeaponBehavior : IWeaponBehavior
    {
        private const int OVERLAP_BUFFER_SIZE = 16;

        private readonly HitResult[] _overlapBuffer = new HitResult[OVERLAP_BUFFER_SIZE];
        private readonly IDamageable[] _alreadyHit;

        public MeleeWeaponBehavior(int maxTargets)
        {
            _alreadyHit = new IDamageable[Mathf.Max(1, maxTargets)];
        }

        public void Fire(WeaponDefinition definition, int shooterId, in WeaponCommand command, IHitQuery hitQuery, DamageSystem damageSystem)
        {
            Vector3 center = command.AimOrigin + command.AimDirection.normalized * definition.Range;
            int hitCount = hitQuery.OverlapSphere(shooterId, center, definition.MeleeRadius, _overlapBuffer);
            int damagedCount = 0;

            for (int hitIndex = 0; hitIndex < hitCount && damagedCount < _alreadyHit.Length; hitIndex++)
            {
                HitResult hit = _overlapBuffer[hitIndex];
                if (WasAlreadyHit(hit.Target, damagedCount))
                {
                    continue;
                }

                _alreadyHit[damagedCount] = hit.Target;
                damagedCount++;
                damageSystem.ApplyDamage(hit.Target, new DamageInfo(definition.GetDamage(hit.Zone), hit.Zone, shooterId));
            }

            Array.Clear(_alreadyHit, 0, _alreadyHit.Length);
            Array.Clear(_overlapBuffer, 0, hitCount);
        }

        private bool WasAlreadyHit(IDamageable target, int damagedCount)
        {
            for (int targetIndex = 0; targetIndex < damagedCount; targetIndex++)
            {
                if (_alreadyHit[targetIndex] == target)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
