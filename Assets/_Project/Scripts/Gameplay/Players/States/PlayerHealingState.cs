// 회복 상태: 회복 키트를 쓰는 동안(3초).
// 걸을 수 있고 슬롯을 바꾸면 회복이 취소된다.
// 무기와 부활은 쓰지 않는다.
// 다 끝나기 전에 다른 상태로 넘어가면(쓰러짐, 게임 오버) 회복을 취소한다(키트는 남음).

using ZD.Gameplay.Inventory;

namespace ZD.Gameplay.Players
{
    public sealed class PlayerHealingState : IPlayerState
    {
        private readonly PlayerMotorSystem _motor;
        private readonly InventorySystem _inventory;

        public PlayerHealingState(PlayerMotorSystem motor, InventorySystem inventory)
        {
            _motor = motor;
            _inventory = inventory;
        }

        public void Enter(in PlayerTickContext context) { }

        public bool Tick(in PlayerTickContext context)
        {
            IPlayerModel player = context.Player;
            PlayerMovement.Move(_motor, context);
            if (context.Input.HasSlotRequest)
            {
                _inventory.SelectSlot(player.Inventory, player, context.Input.RequestedSlot);
            }

            _inventory.TickHealing(player.Inventory, player);
            return false;
        }

        public void Exit(in PlayerTickContext context)
        {
            if (context.Player.Inventory.HealEndTick != 0)
            {
                InventorySystem.CancelHealing(context.Player.Inventory);
            }
        }
    }
}
