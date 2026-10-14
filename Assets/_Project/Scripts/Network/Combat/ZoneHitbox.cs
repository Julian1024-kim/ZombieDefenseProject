// 자기가 덮는 신체 부위를 아는 Lag Compensation 히트박스(머리는 추가 피해).

using Fusion;
using UnityEngine;
using ZD.Gameplay.Combat;

namespace ZD.Network.Combat
{
    public sealed class ZoneHitbox : Hitbox
    {
        [SerializeField] private HitZone _zone;

        private IDamageable _owner;

        public HitZone Zone => _zone;

        public IDamageable Owner
        {
            get
            {
                if (_owner == null && Root != null)
                {
                    _owner = Root.GetComponent<IDamageable>();
                }

                return _owner;
            }
        }
    }
}
