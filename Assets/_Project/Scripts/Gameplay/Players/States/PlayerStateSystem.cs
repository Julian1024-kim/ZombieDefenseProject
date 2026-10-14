// 플레이어 행동 상태 머신.
// 매 틱 복제된 값에서 다음 상태를 정하고(우선순위: 정지 → 쓰러짐 → 부활 중 → 설치 중 → 회복 중 → 활동), 바뀌었으면 이전 상태의 Exit와 새 상태의 Enter를 부른 뒤 현재 상태를 진행시킨다.
// 상태 객체는 미리 만들어 다시 쓴다.
// 상태 번호는 플레이어 모델(복제 값)에 저장되므로 클라이언트 예측·롤백 재실행에서도 같은 순서로 돈다.

using ZD.Gameplay.Building;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Weapons;

namespace ZD.Gameplay.Players
{
    public sealed class PlayerStateSystem
    {
        private readonly BarricadeRosterModel _barricades;
        private readonly MatchStateModel _match;
        private readonly IPlayerState[] _states;

        public PlayerStateSystem(
            PlayerMotorSystem motor,
            ReviveSystem revive,
            InventorySystem inventory,
            WeaponSystem weapons,
            BarricadeRosterModel barricades,
            MatchStateModel match)
        {
            _barricades = barricades;
            _match = match;
            _states = new IPlayerState[]
            {
                new PlayerActiveState(motor, revive, inventory, weapons),
                new PlayerHealingState(motor, inventory),
                new PlayerRevivingState(motor, revive),
                new PlayerBuildingState(motor, inventory),
                new PlayerDownedState(motor),
                new PlayerFrozenState(motor),
            };
        }

        public bool Tick(in PlayerTickContext context)
        {
            IPlayerModel player = context.Player;
            PlayerStateId next = Resolve(player);
            if (next != player.ActionState)
            {
                _states[(int)player.ActionState].Exit(context);
                player.ActionState = next;
                _states[(int)next].Enter(context);
            }

            return _states[(int)next].Tick(context);
        }

        private PlayerStateId Resolve(IPlayerModel player)
        {
            if (_match.IsGameOver)
            {
                return PlayerStateId.Frozen;
            }

            if (player.Health <= 0)
            {
                return PlayerStateId.Downed;
            }

            if (ReviveSystem.IsReviving(player.Revive))
            {
                return PlayerStateId.Reviving;
            }

            if (_barricades.IsInstalling(player.EntityId))
            {
                return PlayerStateId.Building;
            }

            IInventoryModel inventory = player.Inventory;
            if (inventory.SelectedSlot == WeaponSlot.Healing && inventory.HealEndTick != 0)
            {
                return PlayerStateId.Healing;
            }

            return PlayerStateId.Active;
        }
    }
}
