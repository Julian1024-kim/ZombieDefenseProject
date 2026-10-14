// ZombieAISystem의 추적, 공격, 죽음 테스트.

using NUnit.Framework;
using UnityEngine;
using ZD.Core.Messages;
using ZD.Gameplay.Building;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Players;
using ZD.Gameplay.Zombies;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class ZombieAISystemTests
    {
        private FakeClock _clock;
        private ZombieRosterModel _zombies;
        private PlayerRosterModel _players;
        private BarricadeRosterModel _barricades;
        private NearestPlayerTargetSelector _selector;
        private ZombieAISystem _sut;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
            _zombies = new ZombieRosterModel();
            _players = new PlayerRosterModel();
            _barricades = new BarricadeRosterModel();
            var damage = new DamageSystem(new CapturingPublisher<PlayerDownedMessage>(), new CapturingPublisher<ZombieKilledMessage>());
            _selector = new NearestPlayerTargetSelector(_players);
            _sut = new ZombieAISystem(_clock, _zombies, _barricades, damage);
        }

        [Test]
        public void Tick_PlayerFarAway_ChasesNearestStandingPlayer()
        {
            var near = new FakePlayer(1, position: new Vector3(10f, 0f, 0f));
            var far = new FakePlayer(2, position: new Vector3(30f, 0f, 0f));
            var downedNearest = new FakePlayer(3, health: 0, position: new Vector3(2f, 0f, 0f));
            _players.Register(far);
            _players.Register(near);
            _players.Register(downedNearest);
            (FakeZombie zombie, FakeZombieMotor motor) = AddZombie(Vector3.zero);

            _sut.Tick(_selector);

            Assert.That(zombie.State, Is.EqualTo(ZombieStateId.Chase));
            Assert.That(motor.Destination, Is.EqualTo(near.Position));
        }

        [Test]
        public void StopAll_AttackingZombie_StopsAndDealsNoMoreDamage()
        {
            var player = new FakePlayer(1, health: 100, position: new Vector3(1f, 0f, 0f));
            _players.Register(player);
            (FakeZombie zombie, FakeZombieMotor motor) = AddZombie(Vector3.zero);
            _sut.Tick(_selector);
            Assert.That(zombie.State, Is.EqualTo(ZombieStateId.Attack));

            _sut.StopAll();
            _clock.Advance(FakeClock.TICK_RATE * 3);

            Assert.That(zombie.State, Is.EqualTo(ZombieStateId.Chase));
            Assert.That(motor.IsStopped, Is.True);
            Assert.That(player.Health, Is.EqualTo(100));
        }

        [Test]
        public void Tick_PlayerInRange_AttacksAfterWindUpThenOncePerInterval()
        {
            var player = new FakePlayer(1, health: 100, position: new Vector3(1f, 0f, 0f));
            _players.Register(player);
            (FakeZombie zombie, FakeZombieMotor motor) = AddZombie(Vector3.zero);

            _sut.Tick(_selector);
            Assert.That(zombie.State, Is.EqualTo(ZombieStateId.Attack));
            Assert.That(motor.IsStopped, Is.True);
            Assert.That(player.Health, Is.EqualTo(100));

            _clock.Advance(FakeClock.TICK_RATE / 2);
            _sut.Tick(_selector);
            Assert.That(player.Health, Is.EqualTo(90));

            _clock.Advance(1);
            _sut.Tick(_selector);
            Assert.That(player.Health, Is.EqualTo(90));

            _clock.Advance(FakeClock.TICK_RATE);
            _sut.Tick(_selector);
            Assert.That(player.Health, Is.EqualTo(80));
        }

        [Test]
        public void Tick_ZombieHealthZero_EntersDeadAndStops()
        {
            _players.Register(new FakePlayer(1, position: new Vector3(10f, 0f, 0f)));
            (FakeZombie zombie, FakeZombieMotor motor) = AddZombie(Vector3.zero);
            zombie.Health = 0;

            _sut.Tick(_selector);

            Assert.That(zombie.State, Is.EqualTo(ZombieStateId.Dead));
            Assert.That(motor.IsStopped, Is.True);
        }

        [Test]
        public void Tick_PathBlockedByWall_AttacksWallInsteadOfPlayer()
        {
            var player = new FakePlayer(1, health: 100, position: new Vector3(0f, 0f, 1.2f));
            _players.Register(player);
            FakeBarricade wall = AddActiveWall(new Vector3(0f, 0f, 0.6f), health: 300);
            (FakeZombie zombie, FakeZombieMotor motor) = AddZombie(Vector3.zero);
            motor.IsPathBlocked = true;

            _sut.Tick(_selector);
            Assert.That(zombie.State, Is.EqualTo(ZombieStateId.AttackBarricade));

            _clock.Advance(FakeClock.TICK_RATE / 2);
            _sut.Tick(_selector);
            Assert.That(wall.Health, Is.EqualTo(290));
            Assert.That(player.Health, Is.EqualTo(100));
        }

        [Test]
        public void Tick_PlayerBehindWallUpClose_AttacksWallNotThroughIt()
        {
            var player = new FakePlayer(1, health: 100, position: new Vector3(0f, 0f, 1.2f));
            _players.Register(player);
            AddActiveWall(new Vector3(0f, 0f, 0.6f), health: 300);
            (FakeZombie zombie, _) = AddZombie(Vector3.zero);

            _sut.Tick(_selector);

            Assert.That(zombie.State, Is.EqualTo(ZombieStateId.AttackBarricade));
        }

        [Test]
        public void Tick_PathOpen_IgnoresWall()
        {
            var player = new FakePlayer(1, health: 100, position: new Vector3(1f, 0f, 0f));
            _players.Register(player);
            AddActiveWall(new Vector3(0f, 0f, 0.6f), health: 300);
            (FakeZombie zombie, _) = AddZombie(Vector3.zero);

            _sut.Tick(_selector);

            Assert.That(zombie.State, Is.EqualTo(ZombieStateId.Attack));
        }

        [Test]
        public void Tick_HeldByTrap_StaysStillUntilReleased()
        {
            var player = new FakePlayer(1, position: new Vector3(10f, 0f, 0f));
            _players.Register(player);
            (FakeZombie zombie, FakeZombieMotor motor) = AddZombie(Vector3.zero);
            zombie.HeldUntilTick = _clock.Tick + 10;

            _sut.Tick(_selector);
            Assert.That(motor.IsStopped, Is.True);
            Assert.That(motor.Destination, Is.Null);

            _clock.Advance(10);
            _sut.Tick(_selector);
            Assert.That(motor.Destination, Is.EqualTo(player.Position));
        }

        private FakeBarricade AddActiveWall(Vector3 position, int health)
        {
            var wall = new FakeBarricade(500 + _barricades.Count, TestAssets.Barricade(health: health), position, Quaternion.identity)
            {
                Health = health,
                State = BarricadeStateId.Active,
            };
            _barricades.Register(wall);
            return wall;
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
