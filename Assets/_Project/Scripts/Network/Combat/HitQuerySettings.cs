// 피격 판정에 쓰는 레이어 마스크(히트박스 + 총알을 막는 환경).

using UnityEngine;

namespace ZD.Network.Combat
{
    [CreateAssetMenu(menuName = "ZD/Hit Query Settings")]
    public sealed class HitQuerySettings : ScriptableObject
    {
        [Tooltip("Hitbox layer plus the environment layers that block shots.")]
        [SerializeField] private LayerMask _hitMask = ~0;

        public int HitMask => _hitMask;
    }
}
