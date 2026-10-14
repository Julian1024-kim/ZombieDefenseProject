// 호스트 전용.
// 처치(헤드샷 포함)와 부활 메시지를 보고 플레이어별 통계를 올리고, 재시작하면 모두 0으로 되돌린다.
// 함정 처치는 설치한 플레이어의 처치로 센다(처치 메시지의 처치자가 설치자다).

using System;
using MessagePipe;
using ZD.Core.Messages;
using ZD.Gameplay.Players;

namespace ZD.Gameplay.Stats
{
    public sealed class StatsSystem : IDisposable
    {
        private readonly PlayerRosterModel _players;
        private readonly IDisposable _subscriptions;

        public StatsSystem(
            PlayerRosterModel players,
            ISubscriber<ZombieKilledMessage> zombieKilledSubscriber,
            ISubscriber<PlayerRevivedMessage> revivedSubscriber,
            ISubscriber<MatchRestartedMessage> restartedSubscriber)
        {
            _players = players;
            _subscriptions = DisposableBag.Create(
                zombieKilledSubscriber.Subscribe(OnZombieKilled),
                revivedSubscriber.Subscribe(OnRevived),
                restartedSubscriber.Subscribe(_ => ResetAll()));
        }

        public void Dispose() => _subscriptions.Dispose();

        private void OnZombieKilled(ZombieKilledMessage message)
        {
            IPlayerModel killer = _players.FindById(message.KillerId);
            if (killer == null)
            {
                return;
            }

            killer.Stats.Kills++;
            if (message.IsHeadshot)
            {
                killer.Stats.Headshots++;
            }
        }

        private void OnRevived(PlayerRevivedMessage message)
        {
            IPlayerModel reviver = _players.FindById(message.ReviverId);
            if (reviver != null)
            {
                reviver.Stats.Revives++;
            }
        }

        private void ResetAll()
        {
            for (int playerIndex = 0; playerIndex < _players.Count; playerIndex++)
            {
                IPlayerStatsModel stats = _players.Get(playerIndex).Stats;
                stats.Kills = 0;
                stats.Headshots = 0;
                stats.Revives = 0;
            }
        }
    }
}
