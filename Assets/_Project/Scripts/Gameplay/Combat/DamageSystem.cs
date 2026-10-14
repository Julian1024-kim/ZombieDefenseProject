// 호스트에서 피해를 적용한다.
// 체력이 0이 되면 플레이어는 쓰러짐, 좀비는 처치 메시지를 발행한다.
// 방어막이 부서지는 것은 방어막 시스템이 상태 전환으로 처리한다.

using MessagePipe;
using UnityEngine;
using ZD.Core.Messages;

namespace ZD.Gameplay.Combat
{
    public sealed class DamageSystem
    {
        private readonly IPublisher<PlayerDownedMessage> _playerDownedPublisher;
        private readonly IPublisher<ZombieKilledMessage> _zombieKilledPublisher;

        public DamageSystem(
            IPublisher<PlayerDownedMessage> playerDownedPublisher,
            IPublisher<ZombieKilledMessage> zombieKilledPublisher)
        {
            _playerDownedPublisher = playerDownedPublisher;
            _zombieKilledPublisher = zombieKilledPublisher;
        }

        public void ApplyDamage(IDamageable target, in DamageInfo info)
        {
            if (target.Health <= 0 || info.Amount <= 0)
            {
                return;
            }

            target.Health = Mathf.Max(0, target.Health - info.Amount);
            if (target.Health > 0)
            {
                return;
            }

            if (target.Kind == DamageTargetKind.Player)
            {
                _playerDownedPublisher.Publish(new PlayerDownedMessage(target.EntityId));
            }
            else if (target.Kind == DamageTargetKind.Zombie)
            {
                _zombieKilledPublisher.Publish(new ZombieKilledMessage(target.EntityId, info.AttackerId, info.Zone == HitZone.Head));
            }
        }
    }
}
