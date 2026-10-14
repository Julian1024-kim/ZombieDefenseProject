// 배열로 만든 테스트용 인벤토리 모델.

using ZD.Gameplay.Inventory;
using ZD.Gameplay.Players;
using ZD.Gameplay.Weapons;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeInventory : IInventoryModel
    {
        private readonly byte[] _weaponIds = new byte[InventorySlots.WEAPON_COUNT];
        private readonly FakeWeapon[] _weapons =
        {
            new FakeWeapon(), new FakeWeapon(), new FakeWeapon(), new FakeWeapon(),
        };

        public WeaponSlot SelectedSlot { get; set; }
        public WeaponSlot PreviousSlot { get; set; }
        public int ActivePrimary { get; set; }
        public int HealKits { get; set; }
        public int HealEndTick { get; set; }

        public byte GetWeaponId(int weaponIndex) => _weaponIds[weaponIndex];

        public void SetWeaponId(int weaponIndex, byte weaponId) => _weaponIds[weaponIndex] = weaponId;

        public IWeaponModel GetWeaponState(int weaponIndex) => _weapons[weaponIndex];
    }
}
