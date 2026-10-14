// 좀비 하나가 이번 틱에 노리는 대상.
// 플레이어든 거점이든 상태들은 같은 방식으로 다룬다: MovePosition으로 걸어가고, AimPosition을 바라보고, SqrDistance가 공격 범위 안이면 Damageable을 때린다.

using UnityEngine;
using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Zombies
{
    public readonly struct ZombieTarget
    {
        public static readonly ZombieTarget None = default;

        public readonly ZombieTargetKind Kind;
        public readonly IDamageable Damageable;
        public readonly Vector3 MovePosition;
        public readonly Vector3 AimPosition;
        public readonly float SqrDistance;

        public ZombieTarget(ZombieTargetKind kind, IDamageable damageable, Vector3 movePosition, Vector3 aimPosition, float sqrDistance)
        {
            Kind = kind;
            Damageable = damageable;
            MovePosition = movePosition;
            AimPosition = aimPosition;
            SqrDistance = sqrDistance;
        }

        public bool IsNone => Kind == ZombieTargetKind.None;
    }
}
