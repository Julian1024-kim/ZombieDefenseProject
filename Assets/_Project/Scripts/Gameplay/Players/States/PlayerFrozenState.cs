// 정지 상태: 게임 오버.
// 결과 화면을 보는 동안 둘러보기만 하고 이동·사격·회복·부활을 하지 않는다.
// 재시작하면 활동 상태로 돌아간다.

namespace ZD.Gameplay.Players
{
    public sealed class PlayerFrozenState : IPlayerState
    {
        private readonly PlayerMotorSystem _motor;

        public PlayerFrozenState(PlayerMotorSystem motor)
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
