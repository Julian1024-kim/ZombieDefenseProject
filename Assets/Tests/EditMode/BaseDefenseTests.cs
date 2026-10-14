// 거점 방어 모드 테스트: 좀비 목표 선정(거점 우선, 6m 안 플레이어), 모드별 패배 조건, 거점 회복·재시작·스폰.

using NUnit.Framework;
using UnityEngine;
using ZD.Core.Messages;
using ZD.Gameplay.Building;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Defense;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Players;
using ZD.Gameplay.Zombies;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class BaseDefenseTests
    {
        private FakeClock _clock;
        private ZombieRosterModel _zombies;
        private PlayerRosterModel _players;
        private BaseRosterModel _bases;
        private BaseDefinition _baseDefinition;
        private ZombieAISystem _ai;
        private GameModeSystem _modes;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
            _zombies = new ZombieRosterModel();
            _players = new PlayerRosterModel();
            _bases = new BaseRosterModel();
            _baseDefinition = TestAssets.Base();
            var damage = new DamageSystem(new CapturingPublisher<PlayerDownedMessage>(), new CapturingPublisher<ZombieKilledMessage>());
            _ai = new ZombieAISystem(_clock, _zombies, new BarricadeRosterModel(), damage);
            var nearest = new NearestPlayerTargetSelector(_players);
            _modes = new GameModeSystem(
                new AllPlayersDownedCondition(_players),
                new BaseDestroyedCondition(_bases),
                nearest,
                new BaseFirstTargetSelector(nearest, _bases));
        }

        [Test]
        public void Tick_BaseMode_PlayerFar_WalksToBaseEdge()
        {
            _players.Register(new FakePlayer(1, position: new Vector3(-20f, 0f, 0f)));
            _bases.Register(new FakeBase(_baseDefinition, Vector3.zero));
            (FakeZombie zombie, FakeZombieMotor motor) = AddZombie(new Vector3(10f, 0f, 0f));

            _ai.Tick(_modes.GetTargetSelector(GameModeId.BaseDefense));

            Assert.That(zombie.State, Is.EqualTo(ZombieStateId.Chase));
            // 바닥 반폭 1.5m + 다가갈 여유 0.6m.
            Assert.That(motor.Destination.Value.x, Is.EqualTo(2.1f).Within(0.001f));
            Assert.That(motor.Destination.Value.z, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void Tick_BaseMode_NextToBase_AttacksBase()
        {
            _players.Register(new FakePlayer(1, position: new Vector3(-20f, 0f, 0f)));
            var baseModel = new FakeBase(_baseDefinition, Vector3.zero);
            _bases.Register(baseModel);
            (FakeZombie zombie, _) = AddZombie(new Vector3(2.2f, 0f, 0f));

            _ai.Tick(_modes.GetTargetSelector(GameModeId.BaseDefense));
            Assert.That(zombie.State, Is.EqualTo(ZombieStateId.Attack));
            Assert.That(baseModel.Health, Is.EqualTo(_baseDefinition.MaxHealth));

            _clock.Advance(FakeClock.TICK_RATE / 2);
            _ai.Tick(_modes.GetTargetSelector(GameModeId.BaseDefense));

            Assert.That(baseModel.Health, Is.EqualTo(_baseDefinition.MaxHealth - 10));
        }

        [Test]
        public void Tick_BaseMode_PlayerWithinAggroRadius_ChasesPlayer()
        {
            var player = new FakePlayer(1, position: new Vector3(15f, 0f, 0f));
            _players.Register(player);
            _bases.Register(new FakeBase(_baseDefinition, Vector3.zero));
            (_, FakeZombieMotor motor) = AddZombie(new Vector3(10f, 0f, 0f));

            _ai.Tick(_modes.GetTargetSelector(GameModeId.BaseDefense));

            Assert.That(motor.Destination, Is.EqualTo(player.Position));
        }

        [Test]
        public void Tick_BaseMode_DownedPlayerNearby_IgnoresPlayer()
        {
            _players.Register(new FakePlayer(1, health: 0, position: new Vector3(12f, 0f, 0f)));
            _players.Register(new FakePlayer(2, position: new Vector3(-30f, 0f, 0f)));
            _bases.Register(new FakeBase(_baseDefinition, Vector3.zero));
            (_, FakeZombieMotor motor) = AddZombie(new Vector3(10f, 0f, 0f));

            _ai.Tick(_modes.GetTargetSelector(GameModeId.BaseDefense));

            Assert.That(motor.Destination.Value.x, Is.EqualTo(2.1f).Within(0.001f));
        }

        [Test]
        public void Tick_SurvivalMode_IgnoresBase()
        {
            var player = new FakePlayer(1, position: new Vector3(-20f, 0f, 0f));
            _players.Register(player);
            _bases.Register(new FakeBase(_baseDefinition, Vector3.zero));
            (_, FakeZombieMotor motor) = AddZombie(new Vector3(10f, 0f, 0f));

            _ai.Tick(_modes.GetTargetSelector(GameModeId.Survival));

            Assert.That(motor.Destination, Is.EqualTo(player.Position));
        }

        [Test]
        public void IsDefeated_BaseDestroyed_OnlyBaseModeLoses()
        {
            _players.Register(new FakePlayer(1));
            _bases.Register(new FakeBase(_baseDefinition, Vector3.zero) { Health = 0 });

            Assert.That(_modes.IsDefeated(GameModeId.BaseDefense), Is.True);
            Assert.That(_modes.IsDefeated(GameModeId.Survival), Is.False);
        }

        [Test]
        public void IsDefeated_AllPlayersDown_BothModesLose()
        {
            _players.Register(new FakePlayer(1, health: 0));
            _bases.Register(new FakeBase(_baseDefinition, Vector3.zero));

            Assert.That(_modes.IsDefeated(GameModeId.BaseDefense), Is.True);
            Assert.That(_modes.IsDefeated(GameModeId.Survival), Is.True);
        }

        [Test]
        public void IsDefeated_NoBaseYet_NotDefeated()
        {
            _players.Register(new FakePlayer(1));

            Assert.That(_modes.IsDefeated(GameModeId.BaseDefense), Is.False);
        }

        [Test]
        public void WaveCleared_HealsTwentyPercentUpToMax()
        {
            var waveCleared = new CapturingSubscriber<WaveClearedMessage>();
            CreateBaseSystem(waveCleared, new CapturingSubscriber<MatchRestartedMessage>(), out _);
            var baseModel = new FakeBase(_baseDefinition, Vector3.zero) { Health = 1000 };
            _bases.Register(baseModel);

            waveCleared.Raise(new WaveClearedMessage(1));
            Assert.That(baseModel.Health, Is.EqualTo(1600));

            baseModel.Health = 2900;
            waveCleared.Raise(new WaveClearedMessage(2));
            Assert.That(baseModel.Health, Is.EqualTo(3000));
        }

        [Test]
        public void WaveCleared_DestroyedBase_StaysDestroyed()
        {
            var waveCleared = new CapturingSubscriber<WaveClearedMessage>();
            CreateBaseSystem(waveCleared, new CapturingSubscriber<MatchRestartedMessage>(), out _);
            var baseModel = new FakeBase(_baseDefinition, Vector3.zero) { Health = 0 };
            _bases.Register(baseModel);

            waveCleared.Raise(new WaveClearedMessage(1));

            Assert.That(baseModel.Health, Is.EqualTo(0));
        }

        [Test]
        public void MatchRestarted_RestoresFullHealth()
        {
            var restarted = new CapturingSubscriber<MatchRestartedMessage>();
            CreateBaseSystem(new CapturingSubscriber<WaveClearedMessage>(), restarted, out _);
            var baseModel = new FakeBase(_baseDefinition, Vector3.zero) { Health = 0 };
            _bases.Register(baseModel);

            restarted.Raise(new MatchRestartedMessage());

            Assert.That(baseModel.Health, Is.EqualTo(3000));
        }

        [Test]
        public void Setup_SpawnsBaseOnlyInBaseModeAndOnce()
        {
            BaseSystem sut = CreateBaseSystem(new CapturingSubscriber<WaveClearedMessage>(), new CapturingSubscriber<MatchRestartedMessage>(), out FakeBaseSpawner spawner);

            sut.Setup(GameModeId.Survival);
            Assert.That(spawner.SpawnCount, Is.EqualTo(0));

            sut.Setup(GameModeId.BaseDefense);
            sut.Setup(GameModeId.BaseDefense);
            Assert.That(spawner.SpawnCount, Is.EqualTo(1));
            Assert.That(_bases.Current, Is.Not.Null);
        }

        private BaseSystem CreateBaseSystem(
            CapturingSubscriber<WaveClearedMessage> waveCleared,
            CapturingSubscriber<MatchRestartedMessage> restarted,
            out FakeBaseSpawner spawner)
        {
            spawner = new FakeBaseSpawner(_bases, _baseDefinition);
            return new BaseSystem(_bases, spawner, waveCleared, restarted);
        }

        private (FakeZombie, FakeZombieMotor) AddZombie(Vector3 position)
        {
            var zombie = new FakeZombie(100 + _zombies.Count, TestAssets.Zombie(damage: 10), position);
            var motor = new FakeZombieMotor();
            _zombies.Register(zombie, motor);
            return (zombie, motor);
        }
    }
}
