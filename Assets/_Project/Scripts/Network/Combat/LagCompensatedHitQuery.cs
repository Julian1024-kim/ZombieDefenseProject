// 호스트 쪽 피격 판정.
// 쏜 사람이 보던 시점으로 히트박스를 되돌린다(최대 200ms, NetworkProjectConfig에서 설정).
// 클라이언트에서는 아무것도 맞지 않는다: 클라이언트는 탄약과 이펙트만 예측하고 피해는 호스트가 정한다.

using System.Collections.Generic;
using Fusion;
using UnityEngine;
using ZD.Gameplay.Weapons;
using ZD.Network.Session;

namespace ZD.Network.Combat
{
    public sealed class LagCompensatedHitQuery : IHitQuery
    {
        private const int OVERLAP_CAPACITY = 32;
        private const HitOptions QUERY_OPTIONS = HitOptions.IncludePhysX | HitOptions.SubtickAccuracy | HitOptions.IgnoreInputAuthority;

        private readonly NetworkRunnerHolder _runnerHolder;
        private readonly List<LagCompensatedHit> _overlapHits = new(OVERLAP_CAPACITY);
        private readonly int _hitMask;

        public LagCompensatedHitQuery(NetworkRunnerHolder runnerHolder, HitQuerySettings settings)
        {
            _runnerHolder = runnerHolder;
            _hitMask = settings.HitMask;
        }

        public bool Raycast(int shooterId, Vector3 origin, Vector3 direction, float range, out HitResult hit)
        {
            hit = default;
            NetworkRunner runner = _runnerHolder.Runner;
            if (runner == null || !runner.IsServer)
            {
                return false;
            }

            if (!runner.LagCompensation.Raycast(origin, direction, range, PlayerRef.FromIndex(shooterId), out LagCompensatedHit lagHit, _hitMask, QUERY_OPTIONS))
            {
                return false;
            }

            // 벽(PhysX 충돌)에 맞으면 탄이 막힌다: 히트박스가 없으니 피해도 없다.
            if (lagHit.Hitbox is not ZoneHitbox zoneHitbox || zoneHitbox.Owner == null)
            {
                return false;
            }

            hit = new HitResult(zoneHitbox.Owner, zoneHitbox.Zone, lagHit.Point);
            return true;
        }

        public int OverlapSphere(int attackerId, Vector3 center, float radius, HitResult[] results)
        {
            NetworkRunner runner = _runnerHolder.Runner;
            if (runner == null || !runner.IsServer)
            {
                return 0;
            }

            int found = runner.LagCompensation.OverlapSphere(center, radius, PlayerRef.FromIndex(attackerId), _overlapHits, _hitMask, HitOptions.IgnoreInputAuthority);
            int count = 0;
            for (int hitIndex = 0; hitIndex < found && count < results.Length; hitIndex++)
            {
                LagCompensatedHit lagHit = _overlapHits[hitIndex];
                if (lagHit.Hitbox is ZoneHitbox zoneHitbox && zoneHitbox.Owner != null)
                {
                    results[count] = new HitResult(zoneHitbox.Owner, zoneHitbox.Zone, lagHit.Point);
                    count++;
                }
            }

            _overlapHits.Clear();
            return count;
        }
    }
}
