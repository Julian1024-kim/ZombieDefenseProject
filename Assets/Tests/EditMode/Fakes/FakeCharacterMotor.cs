// 마지막으로 받은 시선과 이동(방향, 속도)만 기억하는 테스트용 캐릭터 이동기.

using UnityEngine;
using ZD.Core;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeCharacterMotor : ICharacterMotor
    {
        public Vector3 Position { get; set; }
        public float LastSpeed { get; private set; }
        public Vector3 LastDirection { get; private set; }
        public float LastYaw { get; private set; }

        public void SetLookRotation(float pitch, float yaw) => LastYaw = yaw;

        public void SetInputDirection(Vector3 worldDirection, float speed)
        {
            LastDirection = worldDirection;
            LastSpeed = speed;
        }
    }
}
