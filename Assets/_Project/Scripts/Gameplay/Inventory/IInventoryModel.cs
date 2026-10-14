// 플레이어 인벤토리의 모델 계약.
// 무기는 WeaponCatalog ID(0 = 비어 있음)로 저장해서 네트워크 계층이 바이트로 담을 수 있다.

using ZD.Gameplay.Players;
using ZD.Gameplay.Weapons;

namespace ZD.Gameplay.Inventory
{
    public interface IInventoryModel
    {
        WeaponSlot SelectedSlot { get; set; }

        WeaponSlot PreviousSlot { get; set; }

        int ActivePrimary { get; set; }

        int HealKits { get; set; }

        int HealEndTick { get; set; }

        byte GetWeaponId(int weaponIndex);
        void SetWeaponId(int weaponIndex, byte weaponId);
        IWeaponModel GetWeaponState(int weaponIndex);
    }
}
