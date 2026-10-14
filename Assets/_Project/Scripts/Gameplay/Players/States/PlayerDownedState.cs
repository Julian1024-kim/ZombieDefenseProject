// 쓰러짐 상태: 체력 0.
// 둘러보기만 하고 아무것도 못 한다.
// 동료가 부활시키면(체력이 오르면) 활동 상태로 돌아간다.

namespace ZD.Gameplay.Players
{
    public sealed class PlayerDownedState : IPlayerState
    {
        private readonly PlayerMotorSystem _motor;

        public PlayerDownedState(PlayerMotorSystem motor)
        {
            _motor = motor;
        }

        public void Enter(in PlayerTickContext context) { }

        public bool Tick(in PlayerTickContext context)
        {
            PlayerMovement.LookOnly(_motor, context);
            return false;
        }

        public void Exit(in PlayerTickContext context) { }
    }
}
