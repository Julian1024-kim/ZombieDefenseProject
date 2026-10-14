// 호스트 전용.
// 좀비를 처치한 사람에게 그 좀비의 처치 Point와 헤드샷 보너스를 준다.
// 바닥에 떨어지는 Point는 로컬 연출일 뿐이고 지갑에는 바로 들어간다.

using System;
using MessagePipe;
using ZD.Core.Messages;
using ZD.Gameplay.Players;
using ZD.Gameplay.Zombies;

namespace ZD.Gameplay.Economy
{
    public sealed class KillRewardSystem : IDisposable
    {
        private readonly PlayerRosterModel _players;
        private readonly ZombieRosterModel _zombies;
        private readonly WalletSystem _walletSystem;
        private readonly IDisposable _subscription;

        public KillRewardSystem(
            PlayerRosterModel players,
            ZombieRosterModel zombies,
            WalletSystem walletSystem,
            ISubscriber<ZombieKilledMessage> zombieKilledSubscriber)
        {
            _players = players;
            _zombies = zombies;
            _walletSystem = walletSystem;
            _subscription = zombieKilledSubscriber.Subscribe(OnZombieKilled);
        }

        public void Dispose() => _subscription.Dispose();

        private void OnZombieKilled(ZombieKilledMessage message)
        {
            IPlayerModel killer = _players.FindById(message.KillerId);
            IZombieModel zombie = _zombies.FindById(message.ZombieId);
            if (killer == null || zombie == null)
            {
                return;
            }

            ZombieDefinition definition = zombie.Definition;
            int reward = definition.KillPoints + (message.IsHeadshot ? definition.HeadshotBonusPoints : 0);
            _walletSystem.Earn(killer.Wallet, reward);
        }
    }
}
