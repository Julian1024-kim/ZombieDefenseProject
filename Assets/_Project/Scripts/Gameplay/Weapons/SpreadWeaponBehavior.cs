// 샷건: 해바라기 모양의 고정 패턴으로 여러 발을 쏜다.
// 패턴이 정해져 있어서 난수 시드를 공유하지 않아도 클라이언트의 예측 이펙트와 호스트의 판정이 일치한다.

using UnityEngine;
using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Weapons
{
    public sealed class SpreadWeaponBehavior : IWeaponBehavior
    {
        private const float GOLDEN_ANGLE_DEGREES = 137.508f;

        private readonly Vector2[] _pelletOffsets;

        public SpreadWeaponBehavior(int pelletCount, float spreadDegrees)
        {
            _pelletOffsets = BuildPattern(Mathf.Max(1, pelletCount), spreadDegrees);
        }

        public int PelletCount => _pelletOffsets.Length;

        public void Fire(WeaponDefinition definition, int shooterId, in WeaponCommand command, IHitQuery hitQuery, DamageSystem damageSystem)
        {
            Quaternion aim = Quaternion.LookRotation(command.AimDirection);
            for (int pelletIndex = 0; pelletIndex < _pelletOffsets.Length; pelletIndex++)
            {
                Vector3 direction = GetPelletDirection(aim, pelletIndex);
                if (!hitQuery.Raycast(shooterId, command.AimOrigin, direction, definition.Range, out HitResult hit))
                {
                    continue;
                }

                damageSystem.ApplyDamage(hit.Target, new DamageInfo(definition.GetDamage(hit.Zone), hit.Zone, shooterId));
            }
        }

        public Vector3 GetPelletDirection(Quaternion aim, int pelletIndex)
        {
            Vector2 offset = _pelletOffsets[pelletIndex];
            return aim * Quaternion.Euler(offset.y, offset.x, 0f) * Vector3.forward;
        }

        private static Vector2[] BuildPattern(int pelletCount, float spreadDegrees)
        {
            var offsets = new Vector2[pelletCount];
            for (int pelletIndex = 0; pelletIndex < pelletCount; pelletIndex++)
            {
                // 0번 펠릿은 정면으로, 나머지는 원뿔 가장자리까지 나선형으로 퍼진다.
                float radius = pelletCount == 1 ? 0f : spreadDegrees * Mathf.Sqrt(pelletIndex / (float)(pelletCount - 1));
                float angle = pelletIndex * GOLDEN_ANGLE_DEGREES * Mathf.Deg2Rad;
                offsets[pelletIndex] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            }

            return offsets;
        }
    }
}
