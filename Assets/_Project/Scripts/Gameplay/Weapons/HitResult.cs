// 맞은 대상, 부위, 위치를 담는 판정 결과.

using UnityEngine;
using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Weapons
{
    public readonly struct HitResult
    {
        public readonly IDamageable Target;
        public readonly HitZone Zone;
        public readonly Vector3 Point;

        public HitResult(IDamageable target, HitZone zone, Vector3 point)
        {
            Target = target;
            Zone = zone;
            Point = point;
        }
    }
}
