// 슬롯 규칙: 1번 키에 주무기 2개(다시 누르면 교체), 보조무기 1개, 근접 1개, 4번 키에 회복 키트.
// 예측 틱에서 실행되어 자기 화면에서는 무기 전환이 즉시 보인다.

using UnityEngine;
using ZD.Core;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Players;
using ZD.Gameplay.Weapons;

namespace ZD.Gameplay.Inventory
{
    public sealed class InventorySystem
    {
        private readonly INetworkClock _clock;
        private readonly WeaponCatalog _catalog;
        private readonly WeaponSystem _weaponSystem;
        private readonly InventorySettings _settings;

        public InventorySystem(INetworkClock clock, WeaponCatalog catalog, WeaponSystem weaponSystem, InventorySettings settings)
        {
            _clock = clock;
            _catalog = catalog;
            _weaponSystem = weaponSystem;
            _settings = settings;
        }

        public void ResetLoadout(IInventoryModel inventory)
        {
            for (int weaponIndex = 0; weaponIndex < InventorySlots.WEAPON_COUNT; weaponIndex++)
            {
                inventory.SetWeaponId(weaponIndex, WeaponCatalog.NO_WEAPON);
            }

            PutWeapon(inventory, InventorySlots.SECONDARY, _settings.StartingSecondary);
            PutWeapon(inventory, InventorySlots.MELEE, _settings.StartingMelee);
            inventory.ActivePrimary = InventorySlots.PRIMARY_A;
            inventory.SelectedSlot = WeaponSlot.Secondary;
            inventory.PreviousSlot = WeaponSlot.Secondary;
            inventory.HealKits = 0;
            inventory.HealEndTick = 0;
        }

        public int GetActiveWeaponIndex(IInventoryModel inventory)
        {
            int weaponIndex = inventory.SelectedSlot switch
            {
                WeaponSlot.Primary => inventory.ActivePrimary,
                WeaponSlot.Secondary => InventorySlots.SECONDARY,
                WeaponSlot.Melee => InventorySlots.MELEE,
                _ => InventorySlots.NONE,
            };

            if (weaponIndex == InventorySlots.NONE || inventory.GetWeaponId(weaponIndex) == WeaponCatalog.NO_WEAPON)
            {
                return InventorySlots.NONE;
            }

            return weaponIndex;
        }

        public WeaponDefinition GetWeapon(IInventoryModel inventory, int weaponIndex)
        {
            return weaponIndex == InventorySlots.NONE ? null : _catalog.Get(inventory.GetWeaponId(weaponIndex));
        }

        public void SelectSlot(IInventoryModel inventory, IDamageable owner, WeaponSlot requested)
        {
            switch (requested)
            {
                case WeaponSlot.Primary:
                    SelectPrimary(inventory);
                    break;
                case WeaponSlot.Secondary:
                    SwitchTo(inventory, WeaponSlot.Secondary, InventorySlots.SECONDARY);
                    break;
                case WeaponSlot.Melee:
                    SwitchTo(inventory, WeaponSlot.Melee, InventorySlots.MELEE);
                    break;
                case WeaponSlot.Healing:
                    StartHealing(inventory, owner);
                    break;
            }
        }

        public void TickHealing(IInventoryModel inventory, IDamageable owner)
        {
            if (inventory.SelectedSlot != WeaponSlot.Healing || inventory.HealEndTick == 0 || _clock.Tick < inventory.HealEndTick)
            {
                return;
            }

            owner.Health = Mathf.Min(owner.MaxHealth, owner.Health + _settings.HealAmount);
            inventory.HealKits--;
            inventory.HealEndTick = 0;
            inventory.SelectedSlot = inventory.PreviousSlot;
        }

        public bool TryAddWeapon(IInventoryModel inventory, WeaponDefinition weapon)
        {
            if (FindWeapon(inventory, weapon) != InventorySlots.NONE)
            {
                return false;
            }

            CancelHealing(inventory);
            CancelReloadInHand(inventory);

            switch (weapon.Category)
            {
                case WeaponCategory.Primary:
                    int target = ChooseHolderForNewPrimary(inventory);
                    PutWeapon(inventory, target, weapon);
                    inventory.ActivePrimary = target;
                    inventory.SelectedSlot = WeaponSlot.Primary;
                    break;
                case WeaponCategory.Secondary:
                    PutWeapon(inventory, InventorySlots.SECONDARY, weapon);
                    inventory.SelectedSlot = WeaponSlot.Secondary;
                    break;
                case WeaponCategory.Melee:
                    PutWeapon(inventory, InventorySlots.MELEE, weapon);
                    inventory.SelectedSlot = WeaponSlot.Melee;
                    break;
            }

            return true;
        }

        public bool TryAddReserveAmmo(IInventoryModel inventory, WeaponDefinition weapon, int rounds)
        {
            int weaponIndex = FindWeapon(inventory, weapon);
            if (weaponIndex == InventorySlots.NONE || !weapon.UsesAmmo)
            {
                return false;
            }

            inventory.GetWeaponState(weaponIndex).ReserveAmmo += rounds;
            return true;
        }

        public bool CanAddHealKit(IInventoryModel inventory) => inventory.HealKits < _settings.MaxHealKits;

        public bool TryAddHealKit(IInventoryModel inventory)
        {
            if (!CanAddHealKit(inventory))
            {
                return false;
            }

            inventory.HealKits++;
            return true;
        }

        public int FindWeapon(IInventoryModel inventory, WeaponDefinition weapon)
        {
            byte id = _catalog.GetId(weapon);
            if (id == WeaponCatalog.NO_WEAPON)
            {
                return InventorySlots.NONE;
            }

            for (int weaponIndex = 0; weaponIndex < InventorySlots.WEAPON_COUNT; weaponIndex++)
            {
                if (inventory.GetWeaponId(weaponIndex) == id)
                {
                    return weaponIndex;
                }
            }

            return InventorySlots.NONE;
        }

        private void SelectPrimary(IInventoryModel inventory)
        {
            bool hasA = inventory.GetWeaponId(InventorySlots.PRIMARY_A) != WeaponCatalog.NO_WEAPON;
            bool hasB = inventory.GetWeaponId(InventorySlots.PRIMARY_B) != WeaponCatalog.NO_WEAPON;
            if (!hasA && !hasB)
            {
                return;
            }

            if (inventory.SelectedSlot == WeaponSlot.Primary)
            {
                // 주무기를 든 채로 1을 누르면 다른 주무기가 있을 때 그것으로 바꾼다.
                if (hasA && hasB)
                {
                    CancelReloadInHand(inventory);
                    inventory.ActivePrimary = OtherPrimary(inventory.ActivePrimary);
                }

                return;
            }

            if (inventory.GetWeaponId(inventory.ActivePrimary) == WeaponCatalog.NO_WEAPON)
            {
                inventory.ActivePrimary = hasA ? InventorySlots.PRIMARY_A : InventorySlots.PRIMARY_B;
            }

            CancelHealing(inventory);
            CancelReloadInHand(inventory);
            inventory.SelectedSlot = WeaponSlot.Primary;
        }

        private void SwitchTo(IInventoryModel inventory, WeaponSlot slot, int weaponIndex)
        {
            if (inventory.SelectedSlot == slot || inventory.GetWeaponId(weaponIndex) == WeaponCatalog.NO_WEAPON)
            {
                return;
            }

            CancelHealing(inventory);
            CancelReloadInHand(inventory);
            inventory.SelectedSlot = slot;
        }

        private void StartHealing(IInventoryModel inventory, IDamageable owner)
        {
            if (inventory.SelectedSlot == WeaponSlot.Healing || inventory.HealKits <= 0 || owner.Health >= owner.MaxHealth)
            {
                return;
            }

            CancelReloadInHand(inventory);
            inventory.PreviousSlot = inventory.SelectedSlot;
            inventory.SelectedSlot = WeaponSlot.Healing;
            inventory.HealEndTick = _clock.Tick + Mathf.Max(1, _clock.SecondsToTicks(_settings.HealSeconds));
        }

        public static void CancelHealing(IInventoryModel inventory)
        {
            if (inventory.SelectedSlot == WeaponSlot.Healing)
            {
                inventory.HealEndTick = 0;
                inventory.SelectedSlot = inventory.PreviousSlot;
            }
        }

        private void CancelReloadInHand(IInventoryModel inventory)
        {
            int weaponIndex = GetActiveWeaponIndex(inventory);
            if (weaponIndex != InventorySlots.NONE)
            {
                _weaponSystem.CancelReload(inventory.GetWeaponState(weaponIndex));
            }
        }

        private static int ChooseHolderForNewPrimary(IInventoryModel inventory)
        {
            if (inventory.GetWeaponId(InventorySlots.PRIMARY_A) == WeaponCatalog.NO_WEAPON)
            {
                return InventorySlots.PRIMARY_A;
            }

            if (inventory.GetWeaponId(InventorySlots.PRIMARY_B) == WeaponCatalog.NO_WEAPON)
            {
                return InventorySlots.PRIMARY_B;
            }

            return inventory.ActivePrimary;
        }

        private static int OtherPrimary(int primary) => primary == InventorySlots.PRIMARY_A ? InventorySlots.PRIMARY_B : InventorySlots.PRIMARY_A;

        private void PutWeapon(IInventoryModel inventory, int weaponIndex, WeaponDefinition weapon)
        {
            inventory.SetWeaponId(weaponIndex, _catalog.GetId(weapon));
            _weaponSystem.Equip(inventory.GetWeaponState(weaponIndex), weapon);
        }
    }
}
