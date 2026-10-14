// Simple KCC를 ICharacterMotor로 감싸서, Gameplay가 Fusion 타입 없이 플레이어를 움직이게 한다.

using Fusion.Addons.SimpleKCC;
using UnityEngine;
using ZD.Core;

namespace ZD.Network.Players
{
    public sealed class KccCharacterMotor : ICharacterMotor
    {
        private const float MIN_PITCH = -70f;
        private const float MAX_PITCH = 70f;

        private readonly SimpleKCC _kcc;

        public KccCharacterMotor(SimpleKCC kcc)
        {
            _kcc = kcc;
        }

        public Vector3 Position => _kcc.Position;

        public void SetLookRotation(float pitch, float yaw)
        {
            _kcc.SetLookRotation(pitch, yaw, MIN_PITCH, MAX_PITCH);
        }

        public void SetInputDirection(Vector3 worldDirection, float speed)
        {
            _kcc.Move(worldDirection * speed, 0f);
        }
    }
}
