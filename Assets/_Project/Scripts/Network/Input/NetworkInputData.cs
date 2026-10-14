// 한 틱의 플레이어 입력, 12바이트(3워드).
// Fusion이 필드마다 4바이트로 정렬하므로 작은 값은 직접 묶는다: 버튼 | 이동 x, 이동 y, 슬롯, 슬롯 요청 순번 | 요(0.01도), 피치(0.01도).

using Fusion;
using UnityEngine;

namespace ZD.Network.Input
{
    public struct NetworkInputData : INetworkInput
    {
        private const float MOVE_SCALE = 127f;
        private const float ANGLE_SCALE = 100f;
        private const int BYTE_MASK = 0xFF;
        private const int SHORT_MASK = 0xFFFF;
        private const int MOVE_Y_SHIFT = 8;
        private const int SLOT_SHIFT = 16;
        private const int SLOT_SEQUENCE_SHIFT = 24;
        private const int MOVE_MASK = 0xFFFF;
        private const int PITCH_SHIFT = 16;

        public NetworkButtons Buttons;
        private int _movePacked;
        private int _lookPacked;

        public Vector2 Move => new Vector2(
            (sbyte)(_movePacked & BYTE_MASK) / MOVE_SCALE,
            (sbyte)((_movePacked >> MOVE_Y_SHIFT) & BYTE_MASK) / MOVE_SCALE);

        public byte RequestedSlot
        {
            get => (byte)((_movePacked >> SLOT_SHIFT) & BYTE_MASK);
            set => _movePacked = (_movePacked & ~(BYTE_MASK << SLOT_SHIFT)) | (value << SLOT_SHIFT);
        }

        public byte SlotSequence
        {
            get => (byte)((_movePacked >> SLOT_SEQUENCE_SHIFT) & BYTE_MASK);
            set => _movePacked = (_movePacked & ~(BYTE_MASK << SLOT_SEQUENCE_SHIFT)) | (value << SLOT_SEQUENCE_SHIFT);
        }

        public float YawDegrees => (ushort)(_lookPacked & SHORT_MASK) / ANGLE_SCALE;
        public float PitchDegrees => (short)((_lookPacked >> PITCH_SHIFT) & SHORT_MASK) / ANGLE_SCALE;

        public void SetMove(Vector2 move)
        {
            int x = (byte)(sbyte)Mathf.RoundToInt(Mathf.Clamp(move.x, -1f, 1f) * MOVE_SCALE);
            int y = (byte)(sbyte)Mathf.RoundToInt(Mathf.Clamp(move.y, -1f, 1f) * MOVE_SCALE);
            _movePacked = (_movePacked & ~MOVE_MASK) | x | (y << MOVE_Y_SHIFT);
        }

        public void SetLook(float yawDegrees, float pitchDegrees)
        {
            int yaw = (ushort)Mathf.RoundToInt(Mathf.Repeat(yawDegrees, 360f) * ANGLE_SCALE);
            int pitch = (ushort)(short)Mathf.RoundToInt(Mathf.Clamp(pitchDegrees, -90f, 90f) * ANGLE_SCALE);
            _lookPacked = yaw | (pitch << PITCH_SHIFT);
        }
    }
}
