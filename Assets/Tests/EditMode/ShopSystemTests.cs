// ShopSystem의 구매 검증 테스트(Point, 거리, 쓰러짐, 보유, 최대 수량).

using NUnit.Framework;
using UnityEngine;
using ZD.Core.Messages;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Economy;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Weapons;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class ShopSystemTests
    {
        private const int RIFLE_ITEM = 0;
        private const int RIFLE_AMMO_ITEM = 1;
        private const int HEAL_KIT_ITEM = 2;

        private WeaponDefinition _rifle;
        private InventorySystem _inventorySystem;
        private MatchStateModel _match;
        private ShopSystem _sut;
        private FakePlayer _buyer;

        [SetUp]
        public void SetUp()
        {
            var clock = new FakeClock();
            WeaponDefinition pistol = TestAssets.Pistol();
            WeaponDefinition knife = TestAssets.Knife();
            _rifle = TestAssets.Primary("Rifle");
            WeaponCatalog catalog = TestAssets.Catalog(pistol, knife, _rifle);
            var damage = new DamageSystem(new CapturingPublisher<PlayerDownedMessage>(), new CapturingPublisher<ZombieKilledMessage>());
            _inventorySystem = new InventorySystem(clock, catalog, new WeaponSystem(clock, new FakeHitQuery(), damage), TestAssets.Inventory(pistol, knife, maxHealKits: 1));
            ShopCatalog shop = TestAssets.Shop(2.5f,
                (ShopItemKind.Weapon, _rifle, 1500, 0),
                (ShopItemKind.Ammo, _rifle, 300, 60),
                (ShopItemKind.HealKit, null, 600, 0));
            var boxes = new FakeSupplyBoxes();
            boxes.Positions.Add(Vector3.zero);
            _match = new MatchStateModel();
            _sut = new ShopSystem(shop, boxes, new WalletSystem(), _inventorySystem, _match);

            _buyer = new FakePlayer(1, position: new Vector3(1f, 0f, 1f));
            _inventorySystem.ResetLoadout(_buyer.Inventory);
        }

        [Test]
        public void TryPurchase_GameOver_Rejected()
        {
            _buyer.FakeWallet.Points = 2000;
            _match.Flow = new FakeGameFlow { Phase = GamePhase.GameOver };

            Assert.That(_sut.TryPurchase(_buyer, RIFLE_ITEM), Is.EqualTo(PurchaseResult.MatchOver));
            Assert.That(_buyer.FakeWallet.Points, Is.EqualTo(2000));
        }

        [Test]
        public void TryPurchase_Affordable_ChargesAndGivesTheWeapon()
        {
            _buyer.FakeWallet.Points = 2000;

            PurchaseResult result = _sut.TryPurchase(_buyer, RIFLE_ITEM);

            Assert.That(result, Is.EqualTo(PurchaseResult.Success));
            Assert.That(_buyer.FakeWallet.Points, Is.EqualTo(500));
            Assert.That(_inventorySystem.FindWeapon(_buyer.Inventory, _rifle), Is.Not.EqualTo(InventorySlots.NONE));
        }

        [Test]
        public void TryPurchase_NotEnoughPoints_ChargesNothing()
        {
            _buyer.FakeWallet.Points = 1000;

            Assert.That(_sut.TryPurchase(_buyer, RIFLE_ITEM), Is.EqualTo(PurchaseResult.NotEnoughPoints));
            Assert.That(_buyer.FakeWallet.Points, Is.EqualTo(1000));
        }

        [Test]
        public void TryPurchase_AwayFromTheSupplyBox_IsRefused()
        {
            _buyer.FakeWallet.Points = 5000;
            _buyer.Position = new Vector3(10f, 0f, 0f);

            Assert.That(_sut.TryPurchase(_buyer, RIFLE_ITEM), Is.EqualTo(PurchaseResult.TooFarFromSupplyBox));
            Assert.That(_buyer.FakeWallet.Points, Is.EqualTo(5000));
        }

        [Test]
        public void TryPurchase_WhileDowned_IsRefused()
        {
            _buyer.FakeWallet.Points = 5000;
            _buyer.Health = 0;

            Assert.That(_sut.TryPurchase(_buyer, RIFLE_ITEM), Is.EqualTo(PurchaseResult.Downed));
        }

        [Test]
        public void TryPurchase_AmmoWithoutTheWeapon_IsRefusedAndFree()
        {
            _buyer.FakeWallet.Points = 5000;

            Assert.That(_sut.TryPurchase(_buyer, RIFLE_AMMO_ITEM), Is.EqualTo(PurchaseResult.WeaponNotOwned));
            Assert.That(_buyer.FakeWallet.Points, Is.EqualTo(5000));
        }

        [Test]
        public void TryPurchase_SameWeaponTwice_SecondIsRefused()
        {
            _buyer.FakeWallet.Points = 5000;
            _sut.TryPurchase(_buyer, RIFLE_ITEM);

            Assert.That(_sut.TryPurchase(_buyer, RIFLE_ITEM), Is.EqualTo(PurchaseResult.AlreadyOwned));
            Assert.That(_buyer.FakeWallet.Points, Is.EqualTo(3500));
        }

        [Test]
        public void TryPurchase_HealKitAtCapacity_IsRefused()
        {
            _buyer.FakeWallet.Points = 5000;

            Assert.That(_sut.TryPurchase(_buyer, HEAL_KIT_ITEM), Is.EqualTo(PurchaseResult.Success));
            Assert.That(_sut.TryPurchase(_buyer, HEAL_KIT_ITEM), Is.EqualTo(PurchaseResult.AtCapacity));
            Assert.That(_buyer.FakeWallet.Points, Is.EqualTo(4400));
        }

        [Test]
        public void TryPurchase_InvalidIndex_IsRefused()
        {
            _buyer.FakeWallet.Points = 5000;

            Assert.That(_sut.TryPurchase(_buyer, 99), Is.EqualTo(PurchaseResult.InvalidItem));
        }
    }
}
