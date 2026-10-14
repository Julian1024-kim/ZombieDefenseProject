// 로컬 전용: 플레이어의 복제된 발사 횟수가 늘어날 때 각 클라이언트에서 발행된다.
// 네트워크로는 보내지 않는다.

using UnityEngine;

namespace ZD.Core.Messages
{
    public readonly struct WeaponFiredMessage
    {
        public readonly Vector3 MuzzlePosition;
        public readonly Vector3 AimOrigin;
        public readonly Vector3 AimDirection;
        public readonly float Range;
        public readonly bool IsMelee;

        public WeaponFiredMessage(Vector3 muzzlePosition, Vector3 aimOrigin, Vector3 aimDirection, float range, bool isMelee)
        {
            MuzzlePosition = muzzlePosition;
            AimOrigin = aimOrigin;
            AimDirection = aimDirection;
            Range = range;
            IsMelee = isMelee;
        }
    }
}
