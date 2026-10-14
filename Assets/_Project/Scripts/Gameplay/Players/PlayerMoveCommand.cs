// 한 틱의 이동 입력(이동 방향, 요, 피치).

using UnityEngine;

namespace ZD.Gameplay.Players
{
    public readonly struct PlayerMoveCommand
    {
        public readonly Vector2 Move;
        public readonly float Yaw;
        public readonly float Pitch;

        public PlayerMoveCommand(Vector2 move, float yaw, float pitch)
        {
            Move = move;
            Yaw = yaw;
            Pitch = pitch;
        }
    }
}
