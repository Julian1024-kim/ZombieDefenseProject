// 시선의 요 방향 기준으로 플레이어를 움직인다.
// 쓰러진 플레이어는 둘러볼 수만 있고 움직이지 못한다.
// 자기 클라이언트의 예측 틱과 호스트에서 실행된다.

using UnityEngine;
using ZD.Core;

namespace ZD.Gameplay.Players
{
    public sealed class PlayerMotorSystem
    {
        private readonly PlayerDefinition _definition;

        public PlayerMotorSystem(PlayerDefinition definition)
        {
            _definition = definition;
        }

        public void Tick(IPlayerModel player, ICharacterMotor motor, in PlayerMoveCommand command)
        {
            motor.SetLookRotation(command.Pitch, command.Yaw);

            if (player.Health <= 0)
            {
                motor.SetInputDirection(Vector3.zero, 0f);
                return;
            }

            Vector3 localDirection = new Vector3(command.Move.x, 0f, command.Move.y);
            Vector3 worldDirection = Quaternion.Euler(0f, command.Yaw, 0f) * Vector3.ClampMagnitude(localDirection, 1f);
            motor.SetInputDirection(worldDirection, _definition.MoveSpeed);
        }
    }
}
