// 호스트 전용 거점 관리: 거점 방어 모드면 매치 시작 때 거점을 스폰하고, 웨이브를 클리어할 때마다 최대 체력의 일부를 회복하고(최대치를 넘지 않음), 재시작하면 체력을 다시 채운다.

using System;
using MessagePipe;
using UnityEngine;
using ZD.Core.Messages;
using ZD.Gameplay.GameFlow;

namespace ZD.Gameplay.Defense
{
    public sealed class BaseSystem : IDisposable
    {
        private readonly BaseRosterModel _bases;
        private readonly IBaseSpawner _spawner;
        private readonly IDisposable _subscriptions;

        public BaseSystem(
            BaseRosterModel bases,
            IBaseSpawner spawner,
            ISubscriber<WaveClearedMessage> waveClearedSubscriber,
            ISubscriber<MatchRestartedMessage> restartedSubscriber)
        {
            _bases = bases;
            _spawner = spawner;
            _subscriptions = DisposableBag.Create(
                waveClearedSubscriber.Subscribe(_ => HealAfterWave()),
                restartedSubscriber.Subscribe(_ => RestoreFull()));
        }

        public void Setup(GameModeId mode)
        {
            if (mode == GameModeId.BaseDefense && _bases.Current == null)
            {
                _spawner.Spawn();
            }
        }

        public void Dispose() => _subscriptions.Dispose();

        private void HealAfterWave()
        {
            IBaseModel current = _bases.Current;
            if (current == null || current.Health <= 0)
            {
                return;
            }

            int heal = Mathf.RoundToInt(current.MaxHealth * current.Definition.WaveClearHealFraction);
            current.Health = Mathf.Min(current.MaxHealth, current.Health + heal);
        }

        private void RestoreFull()
        {
            IBaseModel current = _bases.Current;
            if (current != null)
            {
                current.Health = current.MaxHealth;
            }
        }
    }
}
