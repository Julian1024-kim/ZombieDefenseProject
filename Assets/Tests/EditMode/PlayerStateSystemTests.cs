// 플레이어 행동 상태 머신 테스트: 활동(사격), 쓰러짐, 회복 중 쓰러지면 회복 취소, 부활 중 정지, 설치 중 사격 금지, 게임 오버 정지.

using NUnit.Framework;
using UnityEngine;
using ZD.Core.Messages;
using ZD.Gameplay.Building;
using ZD.Gameplay.Combat;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Players;
using ZD.Gameplay.Weapons;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class PlayerStateSystemTests
    {
        private FakeClock _clock;
        private PlayerRosterModel _players;
        private BarricadeRosterModel _barricades;
        private MatchStateModel _match;
        private InventorySystem _inventory;
        private FakePlayer _player;
        private FakeCharacterMotor _motor;
        private PlayerStateSystem _sut;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
            _players = new PlayerRosterModel();
            _barricades = new BarricadeRosterModel();
            _match = new MatchStateModel { Flow = new FakeGameFlow { Phase = GamePhase.Wave } };
            PlayerDefinition definition = TestAssets.Create<PlayerDefinition>(so =>
            {
                so.FindProperty("_moveSpeed").floatValue = 5f;
                so.FindProperty("_reviveRange").floatValue = 2f;
                so.FindProperty("_reviveSeconds").floatValue = 5f;
                so.FindProperty("_reviveHealth").intValue = 30;
            });
            WeaponDefinition pistol = TestAssets.Pistol();
            WeaponDefinition knife = TestAssets.Knife();
            var damage = new DamageSystem(new CapturingPublisher<PlayerDownedMessage>(), new CapturingPublisher<ZombieKilledMessage>());
            var weapons = new WeaponSystem(_clock, new FakeHitQuery(), damage);
            _inventory = new InventorySystem(_clock, TestAssets.Catalog(pistol, knife), weapons, TestAssets.Inventory(pistol, knife));
            var revive = new ReviveSystem(_clock, _players, definition, new CapturingPublisher<PlayerRevivedMessage>());
            _sut = new PlayerStateSystem(new PlayerMotorSystem(definition), revive, _inventory, weapons, _barricades, _match);

            _player = new FakePlayer(1, health: 50, position: Vector3.zero);
            _inventory.ResetLoadout(_player.FakeInventory);
            _players.Register(_player);
            _motor = new FakeCharacterMotor();
        }

        [Test]
        public void Tick_Active_MovesAndFires()
        {
            bool fired = Tick(move: Vector2.up, firePressed: true);

            Assert.That(_player.ActionState, Is.EqualTo(PlayerStateId.Active));
            Assert.That(fired, Is.True);
            Assert.That(_motor.LastSpeed, Is.GreaterThan(0f));
        }

        [Test]
        public void Tick_HealthZero_DownedCannotMoveOrFire()
        {
            _player.Health = 0;

            bool fired = Tick(move: Vector2.up, firePressed: true);

            Assert.That(_player.ActionState, Is.EqualTo(PlayerStateId.Downed));
            Assert.That(fired, Is.False);
            Assert.That(_motor.LastDirection, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Tick_DownedWhileHealing_CancelsHealing()
        {
            _player.FakeInventory.HealKits = 1;
            Tick(slot: WeaponSlot.Healing);
            Tick();
            Assert.That(_player.ActionState, Is.EqualTo(PlayerStateId.Healing));

            _player.Health = 0;
            Tick();

            Assert.That(_player.ActionState, Is.EqualTo(PlayerStateId.Downed));
            Assert.That(_player.FakeInventory.HealEndTick, Is.EqualTo(0));
            Assert.That(_player.FakeInventory.SelectedSlot, Is.Not.EqualTo(WeaponSlot.Healing));
            Assert.That(_player.FakeInventory.HealKits, Is.EqualTo(1));
        }

        [Test]
        public void Tick_ReviveHeld_StopsAndRevivesThenReturns()
        {
            _players.Register(new FakePlayer(2, health: 0, position: new Vector3(1f, 0f, 0f)));

            bool fired = Tick(move: Vector2.up, firePressed: true, reviveHeld: true);
            Assert.That(fired, Is.False);
            Assert.That(_motor.LastDirection, Is.EqualTo(Vector3.zero));

            Tick(move: Vector2.up, reviveHeld: true);
            Assert.That(_player.ActionState, Is.EqualTo(PlayerStateId.Reviving));
            Assert.That(_motor.LastDirection, Is.EqualTo(Vector3.zero));

            Tick(move: Vector2.up);
            Tick(move: Vector2.up);
            Assert.That(_player.ActionState, Is.EqualTo(PlayerStateId.Active));
            Assert.That(ReviveSystem.IsReviving(_player.Revive), Is.False);
        }

        [Test]
        public void Tick_Installing_BuildingCannotFire()
        {
            _barricades.Register(new FakeBarricade(500, TestAssets.Barricade(), Vector3.zero, Quaternion.identity)
            {
                State = BarricadeStateId.Installing,
                BuilderId = _player.EntityId,
            });

            bool fired = Tick(firePressed: true);

            Assert.That(_player.ActionState, Is.EqualTo(PlayerStateId.Building));
            Assert.That(fired, Is.False);
        }

        [Test]
        public void Tick_GameOver_FrozenAndCancelsRevive()
        {
            _players.Register(new FakePlayer(2, health: 0, position: new Vector3(1f, 0f, 0f)));
            Tick(reviveHeld: true);
            Tick(reviveHeld: true);
            Assert.That(_player.ActionState, Is.EqualTo(PlayerStateId.Reviving));

            _match.Flow = new FakeGameFlow { Phase = GamePhase.GameOver };
            bool fired = Tick(move: Vector2.up, firePressed: true, reviveHeld: true);

            Assert.That(_player.ActionState, Is.EqualTo(PlayerStateId.Frozen));
            Assert.That(fired, Is.False);
            Assert.That(_motor.LastDirection, Is.EqualTo(Vector3.zero));
            Assert.That(ReviveSystem.IsReviving(_player.Revive), Is.False);
        }

        private bool Tick(Vector2 move = default, bool firePressed = false, bool reviveHeld = false, WeaponSlot? slot = null)
        {
            var input = new PlayerTickInput(
                move, 0f, 0f, firePressed, firePressed, false, reviveHeld,
                slot.HasValue, slot ?? WeaponSlot.Secondary, Vector3.up, Vector3.forward);
            bool fired = _sut.Tick(new PlayerTickContext(_player, _motor, input, hasStateAuthority: true));
            _clock.Advance(1);
            return fired;
        }
    }
}
