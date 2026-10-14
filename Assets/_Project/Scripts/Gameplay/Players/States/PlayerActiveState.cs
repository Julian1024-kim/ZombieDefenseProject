// 활동 상태(기본): 부활 키를 누르면 동료 부활을 시작하고(시작한 틱부터 멈춤), 아니면 이동, 슬롯 바꾸기, 손에 든 무기 사격을 한다.
// 회복 키트를 꺼내면 다음 틱부터 회복 상태가 된다.

using ZD.Gameplay.Inventory;
using ZD.Gameplay.Weapons;

namespace ZD.Gameplay.Players
{
    public sealed class PlayerActiveState : IPlayerState
    {
        private readonly PlayerMotorSystem _motor;
        private readonly ReviveSystem _revive;
        private readonly InventorySystem _inventory;
        private readonly WeaponSystem _weapons;

        public PlayerActiveState(PlayerMotorSystem motor, ReviveSystem revive, InventorySystem inventory, WeaponSystem weapons)
        {
            _motor = motor;
            _revive = revive;
            _inventory = inventory;
            _weapons = weapons;
        }

        public void Enter(in PlayerTickContext context) { }

        public bool Tick(in PlayerTickContext context)
        {
            IPlayerModel player = context.Player;
            PlayerTickInput input = context.Input;
            _revive.Tick(player, input.IsReviveHeld, context.HasStateAuthority);
            if (ReviveSystem.IsReviving(player.Revive))
            {
                PlayerMovement.LookOnly(_motor, context);
                return false;
            }

            PlayerMovement.Move(_motor, context);
            if (input.HasSlotRequest)
            {
                _inventory.SelectSlot(player.Inventory, player, input.RequestedSlot);
            }

            int weaponIndex = _inventory.GetActiveWeaponIndex(player.Inventory);
            if (weaponIndex == InventorySlots.NONE)
            {
                return false;
            }

            var command = new WeaponCommand(input.IsFireHeld, input.IsFirePressed, input.IsReloadPressed, input.AimOrigin, input.AimDirection);
            WeaponDefinition weapon = _inventory.GetWeapon(player.Inventory, weaponIndex);
            return _weapons.Tick(player.Inventory.GetWeaponState(weaponIndex), weapon, player.EntityId, command);
        }

        public void Exit(in PlayerTickContext context) { }
    }
}
