// WeaponSystem의 연사 간격, 탄약, 재장전, 헤드샷, 근접 판정 테스트.

using NUnit.Framework;
using UnityEngine;
using ZD.Core.Messages;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Weapons;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class WeaponSystemTests
    {
        private static readonly WeaponCommand PressFire = new(true, true, false, Vector3.zero, Vector3.forward);
        private static readonly WeaponCommand HoldFire = new(true, false, false, Vector3.zero, Vector3.forward);
        private static readonly WeaponCommand PressReload = new(false, false, true, Vector3.zero, Vector3.forward);

        private FakeClock _clock;
        private FakeHitQuery _hitQuery;
        private WeaponSystem _sut;
        private FakeWeapon _weapon;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
            _hitQuery = new FakeHitQuery();
            var damage = new DamageSystem(new CapturingPublisher<PlayerDownedMessage>(), new CapturingPublisher<ZombieKilledMessage>());
            _sut = new WeaponSystem(_clock, _hitQuery, damage);
            _weapon = new FakeWeapon();
        }

        [Test]
        public void Equip_Pistol_FillsMagazineAndReserve()
        {
            WeaponDefinition pistol = TestAssets.Pistol(magazine: 12, reserve: 24);

            _sut.Equip(_weapon, pistol);

            Assert.That(_weapon.AmmoInMagazine, Is.EqualTo(12));
            Assert.That(_weapon.ReserveAmmo, Is.EqualTo(24));
        }

        [Test]
        public void Tick_PressFire_ConsumesAmmoAndBlocksUntilFireIntervalPasses()
        {
            WeaponDefinition pistol = TestAssets.Pistol(shotsPerSecond: 4f, magazine: 5);
            _sut.Equip(_weapon, pistol);

            Assert.That(_sut.Tick(_weapon, pistol, 1, PressFire), Is.True);
            _clock.Advance(1);
            Assert.That(_sut.Tick(_weapon, pistol, 1, PressFire), Is.False);
            _clock.Advance(FakeClock.TICK_RATE / 4);
            Assert.That(_sut.Tick(_weapon, pistol, 1, PressFire), Is.True);
            Assert.That(_weapon.AmmoInMagazine, Is.EqualTo(3));
        }

        [Test]
        public void Tick_HoldFireOnSemiAuto_DoesNotFire()
        {
            WeaponDefinition pistol = TestAssets.Pistol();
            _sut.Equip(_weapon, pistol);

            Assert.That(_sut.Tick(_weapon, pistol, 1, HoldFire), Is.False);
        }

        [Test]
        public void Tick_EmptyMagazine_StartsReloadAndRefillsFromReserve()
        {
            WeaponDefinition pistol = TestAssets.Pistol(shotsPerSecond: 60f, magazine: 2, reserve: 1);
            _sut.Equip(_weapon, pistol);
            _sut.Tick(_weapon, pistol, 1, PressFire);
            _clock.Advance(1);
            _sut.Tick(_weapon, pistol, 1, PressFire);
            _clock.Advance(1);

            Assert.That(_sut.Tick(_weapon, pistol, 1, PressFire), Is.False);
            Assert.That(_weapon.ReloadEndTick, Is.GreaterThan(0));

            _clock.Advance(FakeClock.TICK_RATE);
            _sut.Tick(_weapon, pistol, 1, default);

            Assert.That(_weapon.AmmoInMagazine, Is.EqualTo(1));
            Assert.That(_weapon.ReserveAmmo, Is.EqualTo(0));
        }

        [Test]
        public void Tick_ReloadWithFullMagazine_IsIgnored()
        {
            WeaponDefinition pistol = TestAssets.Pistol();
            _sut.Equip(_weapon, pistol);

            _sut.Tick(_weapon, pistol, 1, PressReload);

            Assert.That(_weapon.ReloadEndTick, Is.EqualTo(0));
        }

        [Test]
        public void Tick_HitscanHeadshot_DealsMultipliedDamage()
        {
            WeaponDefinition pistol = TestAssets.Pistol(damage: 20);
            var zombie = new FakeZombie(9, TestAssets.Zombie(health: 100));
            _hitQuery.RaycastHit = new HitResult(zombie, HitZone.Head, Vector3.zero);
            _sut.Equip(_weapon, pistol);

            _sut.Tick(_weapon, pistol, 1, PressFire);

            Assert.That(zombie.Health, Is.EqualTo(60));
        }

        [Test]
        public void Tick_KnifeOverlappingSameTargetTwice_HitsItOnce()
        {
            WeaponDefinition knife = TestAssets.Knife(damage: 30, maxTargets: 3);
            var zombie = new FakeZombie(9, TestAssets.Zombie(health: 100));
            _hitQuery.OverlapHits.Add(new HitResult(zombie, HitZone.Body, Vector3.zero));
            _hitQuery.OverlapHits.Add(new HitResult(zombie, HitZone.Head, Vector3.zero));
            _sut.Equip(_weapon, knife);

            bool fired = _sut.Tick(_weapon, knife, 1, PressFire);

            Assert.That(fired, Is.True);
            Assert.That(zombie.Health, Is.EqualTo(70));
            Assert.That(_hitQuery.RaycastCount, Is.EqualTo(0));
        }
    }
}
