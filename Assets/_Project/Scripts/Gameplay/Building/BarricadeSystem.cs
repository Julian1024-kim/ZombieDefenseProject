// 호스트에서 모든 방어막의 상태 머신(설치 중 → 완성 → 부서짐, 설치 중 → 취소)을 루프 하나로 돌린다.
// 상태와 행동 객체를 미리 만들어 두어 틱마다 할당이 없다.
// 재시작하면 모든 방어막을 지운다.

using System;
using MessagePipe;
using ZD.Core;
using ZD.Core.Messages;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Economy;
using ZD.Gameplay.Players;
using ZD.Gameplay.Zombies;

namespace ZD.Gameplay.Building
{
    public sealed class BarricadeSystem : IDisposable
    {
        private readonly INetworkClock _clock;
        private readonly BarricadeRosterModel _barricades;
        private readonly ZombieRosterModel _zombies;
        private readonly IBarricadeSpawner _spawner;
        private readonly IPublisher<BarricadeDestroyedMessage> _destroyedPublisher;
        private readonly IBarricadeState[] _states;
        private readonly IDisposable _restartSubscription;

        public BarricadeSystem(
            INetworkClock clock,
            BarricadeRosterModel barricades,
            PlayerRosterModel players,
            ZombieRosterModel zombies,
            WalletSystem walletSystem,
            DamageSystem damageSystem,
            BuildCatalog catalog,
            IBarricadeSpawner spawner,
            IPublisher<BarricadeDestroyedMessage> destroyedPublisher,
            ISubscriber<MatchRestartedMessage> restartedSubscriber)
        {
            _clock = clock;
            _barricades = barricades;
            _zombies = zombies;
            _spawner = spawner;
            _destroyedPublisher = destroyedPublisher;

            var finished = new FinishedBarricadeState();
            _states = new IBarricadeState[]
            {
                new InstallingBarricadeState(players, walletSystem, catalog.CancelMoveDistance),
                new ActiveBarricadeState(BarricadeBehaviors.Create(clock, damageSystem)),
                finished,
                finished,
            };
            _restartSubscription = restartedSubscriber.Subscribe(_ => _spawner.DespawnAll());
        }

        public void Tick()
        {
            var context = new BarricadeTickContext(_clock.Tick, _zombies);
            for (int barricadeIndex = 0; barricadeIndex < _barricades.Count; barricadeIndex++)
            {
                IBarricadeModel barricade = _barricades.Get(barricadeIndex);
                BarricadeStateId next = _states[(int)barricade.State].Tick(barricade, context);
                if (next != barricade.State)
                {
                    ChangeState(barricade, next);
                }
            }
        }

        public void Dispose() => _restartSubscription.Dispose();

        private void ChangeState(IBarricadeModel barricade, BarricadeStateId next)
        {
            barricade.State = next;
            if (next == BarricadeStateId.Destroyed)
            {
                _destroyedPublisher.Publish(new BarricadeDestroyedMessage(barricade.EntityId));
            }

            if (next == BarricadeStateId.Destroyed || next == BarricadeStateId.Cancelled)
            {
                barricade.Health = 0;
            }
        }
    }
}
