// 방어막 설치 상태(3초).
// 손이 바빠서 무기와 부활은 쓰지 않는다.
// 움직이면 방어막 시스템이 설치를 취소한다(그래서 이동은 막지 않음).
// 슬롯은 바꿀 수 있다.

using ZD.Gameplay.Inventory;

namespace ZD.Gameplay.Players
{
    public sealed class PlayerBuildingState : IPlayerState
    {
        private readonly PlayerMotorSystem _motor;
        private readonly InventorySystem _inventory;

        public PlayerBuildingState(PlayerMotorSystem motor, InventorySystem inventory)
        {
            _motor = motor;
            _inventory = inventory;
        }

        public void Enter(in PlayerTickContext context) { }

        public bool Tick(in PlayerTickContext context)
        {
            PlayerMovement.Move(_motor, context);
            if (context.Input.HasSlotRequest)
            {
                _inventory.SelectSlot(context.Player.Inventory, context.Player, context.Input.RequestedSlot);
            }

            return false;
        }

        public void Exit(in PlayerTickContext context) { }
    }
}
