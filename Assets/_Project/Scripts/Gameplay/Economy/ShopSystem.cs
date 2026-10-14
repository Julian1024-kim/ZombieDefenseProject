// 호스트 전용 구매 검증.
// 클라이언트는 "N번 품목을 사고 싶다"만 보내고, 호스트가 거리, Point, 인벤토리를 확인한 뒤 Point를 뺀다.
// 모든 검사가 차감 전에 끝나므로 거절된 구매는 비용이 없다.

using UnityEngine;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Players;

namespace ZD.Gameplay.Economy
{
    public sealed class ShopSystem
    {
        private readonly ShopCatalog _catalog;
        private readonly ISupplyBoxLocator _supplyBoxes;
        private readonly WalletSystem _walletSystem;
        private readonly InventorySystem _inventorySystem;
        private readonly MatchStateModel _match;

        public ShopSystem(ShopCatalog catalog, ISupplyBoxLocator supplyBoxes, WalletSystem walletSystem, InventorySystem inventorySystem, MatchStateModel match)
        {
            _match = match;
            _catalog = catalog;
            _supplyBoxes = supplyBoxes;
            _walletSystem = walletSystem;
            _inventorySystem = inventorySystem;
        }

        public bool IsNearSupplyBox(Vector3 position)
        {
            float rangeSqr = _catalog.InteractRange * _catalog.InteractRange;
            for (int boxIndex = 0; boxIndex < _supplyBoxes.Count; boxIndex++)
            {
                Vector3 offset = _supplyBoxes.GetPosition(boxIndex) - position;
                offset.y = 0f;
                if (offset.sqrMagnitude <= rangeSqr)
                {
                    return true;
                }
            }

            return false;
        }

        public PurchaseResult TryPurchase(IPlayerModel buyer, int itemIndex)
        {
            PurchaseResult check = Validate(buyer, itemIndex);
            if (check != PurchaseResult.Success)
            {
                return check;
            }

            ShopItem item = _catalog.Get(itemIndex);
            _walletSystem.TrySpend(buyer.Wallet, item.Price);
            Apply(buyer.Inventory, item);
            return PurchaseResult.Success;
        }

        public PurchaseResult Validate(IPlayerModel buyer, int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= _catalog.Count)
            {
                return PurchaseResult.InvalidItem;
            }

            if (_match.IsGameOver)
            {
                return PurchaseResult.MatchOver;
            }

            if (buyer.Health <= 0)
            {
                return PurchaseResult.Downed;
            }

            if (!IsNearSupplyBox(buyer.Position))
            {
                return PurchaseResult.TooFarFromSupplyBox;
            }

            // 인벤토리 사유를 먼저 본다: "이미 보유"가 "Point 부족"보다 플레이어에게 더 많은 걸 알려준다.
            ShopItem item = _catalog.Get(itemIndex);
            PurchaseResult inventoryCheck = CheckInventory(buyer.Inventory, item);
            if (inventoryCheck != PurchaseResult.Success)
            {
                return inventoryCheck;
            }

            return buyer.Wallet.Points < item.Price ? PurchaseResult.NotEnoughPoints : PurchaseResult.Success;
        }

        private PurchaseResult CheckInventory(IInventoryModel inventory, ShopItem item)
        {
            switch (item.Kind)
            {
                case ShopItemKind.Weapon:
                    return _inventorySystem.FindWeapon(inventory, item.Weapon) != InventorySlots.NONE
                        ? PurchaseResult.AlreadyOwned
                        : PurchaseResult.Success;
                case ShopItemKind.Ammo:
                    return _inventorySystem.FindWeapon(inventory, item.Weapon) == InventorySlots.NONE
                        ? PurchaseResult.WeaponNotOwned
                        : PurchaseResult.Success;
                case ShopItemKind.HealKit:
                    return _inventorySystem.CanAddHealKit(inventory) ? PurchaseResult.Success : PurchaseResult.AtCapacity;
                default:
                    return PurchaseResult.InvalidItem;
            }
        }

        private void Apply(IInventoryModel inventory, ShopItem item)
        {
            switch (item.Kind)
            {
                case ShopItemKind.Weapon:
                    _inventorySystem.TryAddWeapon(inventory, item.Weapon);
                    break;
                case ShopItemKind.Ammo:
                    _inventorySystem.TryAddReserveAmmo(inventory, item.Weapon, item.Amount);
                    break;
                case ShopItemKind.HealKit:
                    _inventorySystem.TryAddHealKit(inventory);
                    break;
            }
        }
    }
}
