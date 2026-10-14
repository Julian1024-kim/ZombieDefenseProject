// 방어막 상태 머신 테스트: 설치 완성, 움직이거나 맞으면 취소와 환불, 지뢰 폭발, 곰덫 묶기, 재시작 정리.

using NUnit.Framework;
using UnityEngine;
using ZD.Core.Messages;
using ZD.Gameplay.Building;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Economy;
using ZD.Gameplay.Players;
using ZD.Gameplay.Zombies;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class BarricadeSystemTests
    {
        private const int INSTALL_TICKS = FakeClock.TICK_RATE * 3;

        private FakeClock _clock;
        private BarricadeRosterModel _barricades;
        private PlayerRosterModel _players;
        private ZombieRosterModel _zombies;
        private FakeBarricadeSpawner _spawner;
        private CapturingPublisher<BarricadeDestroyedMessage> _destroyed;
        private CapturingPublisher<ZombieKilledMessage> _killed;
        private CapturingSubscriber<MatchRestartedMessage> _restarted;
        private BarricadeSystem _sut;
        private FakePlayer _builder;
        private ZombieDefinition _zombieDefinition;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
            _barricades = new BarricadeRosterModel();
            _players = new PlayerRosterModel();
            _zombies = new ZombieRosterModel();
            _spawner = new FakeBarricadeSpawner(_barricades);
            _destroyed = new CapturingPublisher<BarricadeDestroyedMessage>();
            _killed = new CapturingPublisher<ZombieKilledMessage>();
            _restarted = new CapturingSubscriber<MatchRestartedMessage>();
            var damage = new DamageSystem(new CapturingPublisher<PlayerDownedMessage>(), _killed);
            _sut = new BarricadeSystem(_clock, _barricades, _players, _zombies, new WalletSystem(), damage,
                TestAssets.Build(), _spawner, _destroyed, _restarted);

            _builder = new FakePlayer(7, position: Vector3.zero);
            _players.Register(_builder);
            _zombieDefinition = TestAssets.Zombie(health: 100);
        }

        [TearDown]
        public void TearDown() => _sut.Dispose();

        [Test]
        public void Tick_InstallTimeElapsed_BecomesActive()
        {
            FakeBarricade barricade = Install(TestAssets.Barricade());

            AdvanceAndTick(INSTALL_TICKS - 1);
            Assert.That(barricade.State, Is.EqualTo(BarricadeStateId.Installing));

            AdvanceAndTick(1);
            Assert.That(barricade.State, Is.EqualTo(BarricadeStateId.Active));
        }

        [Test]
        public void Tick_BuilderMoves_CancelsAndRefunds()
        {
            FakeBarricade barricade = Install(TestAssets.Barricade(price: 300));

            _builder.Position = new Vector3(0.5f, 0f, 0f);
            AdvanceAndTick(1);

            Assert.That(barricade.State, Is.EqualTo(BarricadeStateId.Cancelled));
            Assert.That(barricade.Health, Is.EqualTo(0));
            Assert.That(_builder.FakeWallet.Points, Is.EqualTo(300));
            Assert.That(_destroyed.Messages, Is.Empty);
        }

        [Test]
        public void Tick_BuilderHit_CancelsAndRefunds()
        {
            FakeBarricade barricade = Install(TestAssets.Barricade(price: 300));

            _builder.Health -= 10;
            AdvanceAndTick(1);

            Assert.That(barricade.State, Is.EqualTo(BarricadeStateId.Cancelled));
            Assert.That(_builder.FakeWallet.Points, Is.EqualTo(300));
        }

        [Test]
        public void Tick_HealedThenHitWhileInstalling_StillCancels()
        {
            _builder.Health = 50;
            FakeBarricade barricade = Install(TestAssets.Barricade(price: 300));

            _builder.Health = 100;
            AdvanceAndTick(1);
            _builder.Health = 90;
            AdvanceAndTick(1);

            Assert.That(barricade.State, Is.EqualTo(BarricadeStateId.Cancelled));
        }

        [Test]
        public void Tick_ActiveWallHealthZero_DestroyedWithMessage()
        {
            FakeBarricade barricade = Install(TestAssets.Barricade());
            AdvanceAndTick(INSTALL_TICKS);

            barricade.Health = 0;
            AdvanceAndTick(1);

            Assert.That(barricade.State, Is.EqualTo(BarricadeStateId.Destroyed));
            Assert.That(_destroyed.Messages.Count, Is.EqualTo(1));
        }

        [Test]
        public void Tick_ZombieStepsOnMine_ExplodesAndCreditsBuilder()
        {
            FakeBarricade mine = Install(TestAssets.Barricade(BarricadeKind.Mine, trapDamage: 150, triggerRadius: 0.8f, effectRadius: 3f), new Vector3(0f, 0f, 2f));
            AdvanceAndTick(INSTALL_TICKS);
            FakeZombie onTop = AddZombie(new Vector3(0f, 0f, 2.5f));
            FakeZombie nearby = AddZombie(new Vector3(2f, 0f, 2f));
            FakeZombie far = AddZombie(new Vector3(10f, 0f, 2f));

            AdvanceAndTick(1);

            Assert.That(onTop.Health, Is.EqualTo(0));
            Assert.That(nearby.Health, Is.EqualTo(0));
            Assert.That(far.Health, Is.EqualTo(100));
            Assert.That(mine.State, Is.EqualTo(BarricadeStateId.Destroyed));
            Assert.That(_killed.Messages.Count, Is.EqualTo(2));
            Assert.That(_killed.Messages[0].KillerId, Is.EqualTo(7));
        }

        [Test]
        public void Tick_InstallingMine_DoesNotTrigger()
        {
            Install(TestAssets.Barricade(BarricadeKind.Mine), new Vector3(0f, 0f, 2f));
            FakeZombie zombie = AddZombie(new Vector3(0f, 0f, 2f));

            AdvanceAndTick(1);

            Assert.That(zombie.Health, Is.EqualTo(100));
        }

        [Test]
        public void Tick_BearTrap_HoldsOneZombieAtATimeAndBreaksAfterUses()
        {
            FakeBarricade trap = Install(TestAssets.Barricade(BarricadeKind.BearTrap, trapDamage: 40, holdSeconds: 3f, uses: 3), new Vector3(0f, 0f, 2f));
            AdvanceAndTick(INSTALL_TICKS);
            FakeZombie first = AddZombie(new Vector3(0f, 0f, 2.2f), health: 1000);
            FakeZombie second = AddZombie(new Vector3(0.3f, 0f, 2f), health: 1000);

            AdvanceAndTick(1);
            IZombieModel caught = first.HeldUntilTick > 0 ? first : second;
            IZombieModel other = caught == first ? second : first;
            Assert.That(caught.HeldUntilTick, Is.EqualTo(_clock.Tick + FakeClock.TICK_RATE * 3));
            Assert.That(caught.Health, Is.EqualTo(960));
            Assert.That(other.HeldUntilTick, Is.EqualTo(0));
            Assert.That(trap.UsesLeft, Is.EqualTo(2));

            for (int useIndex = 0; useIndex < 3; useIndex++)
            {
                AdvanceAndTick(FakeClock.TICK_RATE * 3);
            }

            Assert.That(trap.UsesLeft, Is.EqualTo(0));
            Assert.That(trap.State, Is.EqualTo(BarricadeStateId.Destroyed));
        }

        [Test]
        public void MatchRestarted_DespawnsAllBarricades()
        {
            Install(TestAssets.Barricade());

            _restarted.Raise(new MatchRestartedMessage());

            Assert.That(_spawner.DespawnAllCount, Is.EqualTo(1));
            Assert.That(_barricades.Count, Is.EqualTo(0));
        }

        private FakeBarricade Install(BarricadeDefinition definition, Vector3 position = default)
        {
            var barricade = (FakeBarricade)_spawner.Spawn(definition, position, Quaternion.identity);
            barricade.Health = definition.MaxHealth;
            barricade.State = BarricadeStateId.Installing;
            barricade.InstallEndTick = _clock.Tick + INSTALL_TICKS;
            barricade.BuilderId = _builder.EntityId;
            barricade.BuilderStartPosition = _builder.Position;
            barricade.BuilderStartHealth = _builder.Health;
            barricade.UsesLeft = definition.Uses;
            return barricade;
        }

        private FakeZombie AddZombie(Vector3 position, int health = 100)
        {
            var zombie = new FakeZombie(_zombies.Count + 100, _zombieDefinition, position) { Health = health };
            _zombies.Register(zombie, new FakeZombieMotor());
            return zombie;
        }

        private void AdvanceAndTick(int ticks)
        {
            for (int tickIndex = 0; tickIndex < ticks; tickIndex++)
            {
                _clock.Advance(1);
                _sut.Tick();
            }
        }
    }
}
