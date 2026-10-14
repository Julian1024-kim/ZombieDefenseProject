// 부활 상태: 쓰러진 동료 옆에서 부활 키를 누르고 있는 동안(5초).
// 제자리에서 둘러보기만 하고, 키를 떼거나 멀어지면 부활 시스템이 취소한다.
// 다른 상태로 넘어가면(내가 쓰러짐, 게임 오버) 부활을 취소한다.

namespace ZD.Gameplay.Players
{
    public sealed class PlayerRevivingState : IPlayerState
    {
        private readonly PlayerMotorSystem _motor;
        private readonly ReviveSystem _revive;

        public PlayerRevivingState(PlayerMotorSystem motor, ReviveSystem revive)
        {
            _motor = motor;
            _revive = revive;
        }

        public void Enter(in PlayerTickContext context) { }

        public bool Tick(in PlayerTickContext context)
        {
            _revive.Tick(context.Player, context.Input.IsReviveHeld, context.HasStateAuthority);
            PlayerMovement.LookOnly(_motor, context);
            return false;
        }

        public void Exit(in PlayerTickContext context) => _revive.Cancel(context.Player.Revive);
    }
}
