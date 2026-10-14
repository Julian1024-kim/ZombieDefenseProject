// 상태들이 같이 쓰는 이동 처리: 입력대로 움직이기, 또는 제자리에서 둘러보기만.

using UnityEngine;

namespace ZD.Gameplay.Players
{
    internal static class PlayerMovement
    {
        public static void Move(PlayerMotorSystem motor, in PlayerTickContext context)
        {
            PlayerTickInput input = context.Input;
            motor.Tick(context.Player, context.Motor, new PlayerMoveCommand(input.Move, input.Yaw, input.Pitch));
        }

        public static void LookOnly(PlayerMotorSystem motor, in PlayerTickContext context)
        {
            PlayerTickInput input = context.Input;
            motor.Tick(context.Player, context.Motor, new PlayerMoveCommand(Vector2.zero, input.Yaw, input.Pitch));
        }
    }
}
