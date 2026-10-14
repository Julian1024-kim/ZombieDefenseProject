// InventorySystem의 슬롯 규칙, 무기 추가, 탄약, 회복 테스트.

using NUnit.Framework;
using ZD.Core.Messages;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Players;
using ZD.Gameplay.Weapons;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class InventorySystemTests
    {
        private FakeClock _clock;
        private WeaponDefinition _pistol;
        private WeaponDefinition _knife;
        private WeaponDefinition _rifle;
        private WeaponDefinition _shotgun;
        private WeaponDefinition _sniper;
        private WeaponCatalog _catalog;
        private InventorySystem _sut;
        private FakePlayer _player;
        private FakeInventory _inventory;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
            _pistol = TestAssets.Pistol();
            _knife = TestAssets.Knife();
            _rifle = TestAssets.Primary("Rifle");
            _shotgun = TestAssets.Primary("Shotgun");
            _sniper = TestAssets.Primary("Sniper");
            _catalog = TestAssets.Catalog(_pistol, _knife, _rifle, _shotgun, _sniper);
            var damage = new DamageSystem(new CapturingPublisher<PlayerDownedMessage>(), new CapturingPublisher<ZombieKilledMessage>());
            var weapons = new WeaponSystem(_clock, new FakeHitQuery(), damage);
            _sut = new InventorySystem(_clock, _catalog, weapons, TestAssets.Inventory(_pistol, _knife));
            _player = new FakePlayer(1);
            _inventory = _player.FakeInventory;
            _sut.ResetLoadout(_inventory);
        }

        [Test]
        public void ResetLoadout_StartsWithPistolInHandAndKnifeNoPrimaries()
        {
            Assert.That(_sut.GetWeapon(_inventory, _sut.GetActiveWeaponIndex(_inventory)), Is.SameAs(_pistol));
            Assert.That(_sut.GetWeapon(_inventory, InventorySlots.MELEE), Is.SameAs(_knife));
            Assert.That(_inventory.GetWeaponId(InventorySlots.PRIMARY_A), Is.EqualTo(WeaponCatalog.NO_WEAPON));
            Assert.That(_inventory.GetWeaponState(InventorySlots.SECONDARY).AmmoInMagazine, Is.EqualTo(_pistol.MagazineSize));
        }

        [Test]
        public void SelectSlot_PrimaryWithNoPrimaries_KeepsCurrentWeapon()
        {
            _sut.SelectSlot(_inventory, _player, WeaponSlot.Primary);

            Assert.That(_inventory.SelectedSlot, Is.EqualTo(WeaponSlot.Secondary));
        }

        [Test]
        public void TryAddWeapon_TwoPrimaries_FillBothHoldersAndDrawTheNewest()
        {
            _sut.TryAddWeapon(_inventory, _rifle);
            _sut.TryAddWeapon(_inventory, _shotgun);

            Assert.That(_inventory.SelectedSlot, Is.EqualTo(WeaponSlot.Primary));
            Assert.That(_sut.GetWeapon(_inventory, _sut.GetActiveWeaponIndex(_inventory)), Is.SameAs(_shotgun));
            Assert.That(_sut.FindWeapon(_inventory, _rifle), Is.Not.EqualTo(InventorySlots.NONE));
        }

        [Test]
        public void SelectSlot_PrimaryAgainWithTwoPrimaries_SwapsToTheOtherOne()
        {
            _sut.TryAddWeapon(_inventory, _rifle);
            _sut.TryAddWeapon(_inventory, _shotgun);

            _sut.SelectSlot(_inventory, _player, WeaponSlot.Primary);
            Assert.That(_sut.GetWeapon(_inventory, _sut.GetActiveWeaponIndex(_inventory)), Is.SameAs(_rifle));

            _sut.SelectSlot(_inventory, _player, WeaponSlot.Primary);
            Assert.That(_sut.GetWeapon(_inventory, _sut.GetActiveWeaponIndex(_inventory)), Is.SameAs(_shotgun));
        }

        [Test]
        public void SelectSlot_PrimaryFromAnotherSlot_DrawsTheLastUsedPrimary()
        {
            _sut.TryAddWeapon(_inventory, _rifle);
            _sut.TryAddWeapon(_inventory, _shotgun);
            _sut.SelectSlot(_inventory, _player, WeaponSlot.Primary);
            _sut.SelectSlot(_inventory, _player, WeaponSlot.Melee);

            _sut.SelectSlot(_inventory, _player, WeaponSlot.Primary);

            Assert.That(_sut.GetWeapon(_inventory, _sut.GetActiveWeaponIndex(_inventory)), Is.SameAs(_rifle));
        }

        [Test]
        public void SelectSlot_PrimaryAgainWithOnePrimary_DoesNothing()
        {
            _sut.TryAddWeapon(_inventory, _rifle);

            _sut.SelectSlot(_inventory, _player, WeaponSlot.Primary);

            Assert.That(_sut.GetWeapon(_inventory, _sut.GetActiveWeaponIndex(_inventory)), Is.SameAs(_rifle));
        }

        [Test]
        public void TryAddWeapon_ThirdPrimary_ReplacesThePrimaryInHand()
        {
            _sut.TryAddWeapon(_inventory, _rifle);
            _sut.TryAddWeapon(_inventory, _shotgun);

            _sut.TryAddWeapon(_inventory, _sniper);

            Assert.That(_sut.FindWeapon(_inventory, _shotgun), Is.EqualTo(InventorySlots.NONE));
            Assert.That(_sut.FindWeapon(_inventory, _rifle), Is.Not.EqualTo(InventorySlots.NONE));
            Assert.That(_sut.GetWeapon(_inventory, _sut.GetActiveWeaponIndex(_inventory)), Is.SameAs(_sniper));
        }

        [Test]
        public void TryAddWeapon_AlreadyOwned_IsRefused()
        {
            _sut.TryAddWeapon(_inventory, _rifle);

            Assert.That(_sut.TryAddWeapon(_inventory, _rifle), Is.False);
        }

        [Test]
        public void TryAddReserveAmmo_OnlyForOwnedWeapons()
        {
            Assert.That(_sut.TryAddReserveAmmo(_inventory, _rifle, 60), Is.False);
            int before = _inventory.GetWeaponState(InventorySlots.SECONDARY).ReserveAmmo;

            Assert.That(_sut.TryAddReserveAmmo(_inventory, _pistol, 24), Is.True);
            Assert.That(_inventory.GetWeaponState(InventorySlots.SECONDARY).ReserveAmmo, Is.EqualTo(before + 24));
        }

        [Test]
        public void Healing_TakesThreeSecondsHealsSeventyAndReturnsToTheWeapon()
        {
            _player.Health = 20;
            _sut.TryAddHealKit(_inventory);

            _sut.SelectSlot(_inventory, _player, WeaponSlot.Healing);
            Assert.That(_sut.GetActiveWeaponIndex(_inventory), Is.EqualTo(InventorySlots.NONE));

            _clock.Advance(FakeClock.TICK_RATE * 3 - 1);
            _sut.TickHealing(_inventory, _player);
            Assert.That(_player.Health, Is.EqualTo(20));

            _clock.Advance(1);
            _sut.TickHealing(_inventory, _player);
            Assert.That(_player.Health, Is.EqualTo(90));
            Assert.That(_inventory.HealKits, Is.EqualTo(0));
            Assert.That(_inventory.SelectedSlot, Is.EqualTo(WeaponSlot.Secondary));
        }

        [Test]
        public void Healing_SwitchingWeaponCancelsAndKeepsTheKit()
        {
            _player.Health = 20;
            _sut.TryAddHealKit(_inventory);
            _sut.SelectSlot(_inventory, _player, WeaponSlot.Healing);

            _sut.SelectSlot(_inventory, _player, WeaponSlot.Melee);
            _clock.Advance(FakeClock.TICK_RATE * 5);
            _sut.TickHealing(_inventory, _player);

            Assert.That(_player.Health, Is.EqualTo(20));
            Assert.That(_inventory.HealKits, Is.EqualTo(1));
            Assert.That(_inventory.SelectedSlot, Is.EqualTo(WeaponSlot.Melee));
        }

        [Test]
        public void Healing_AtFullHealthOrWithoutKits_DoesNotStart()
        {
            _sut.TryAddHealKit(_inventory);
            _sut.SelectSlot(_inventory, _player, WeaponSlot.Healing);
            Assert.That(_inventory.SelectedSlot, Is.EqualTo(WeaponSlot.Secondary));

            _player.Health = 50;
            _inventory.HealKits = 0;
            _sut.SelectSlot(_inventory, _player, WeaponSlot.Healing);
            Assert.That(_inventory.SelectedSlot, Is.EqualTo(WeaponSlot.Secondary));
        }

        [Test]
        public void TryAddHealKit_StopsAtTheMaximum()
        {
            Assert.That(_sut.TryAddHealKit(_inventory), Is.True);
            Assert.That(_sut.TryAddHealKit(_inventory), Is.True);
            Assert.That(_sut.TryAddHealKit(_inventory), Is.False);
            Assert.That(_inventory.HealKits, Is.EqualTo(2));
        }
    }
}
