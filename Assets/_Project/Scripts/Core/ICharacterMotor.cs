// 캐릭터 이동 계약.
// 네트워크 계층이 Simple KCC 위에 구현한다.

using UnityEngine;

namespace ZD.Core
{
    public interface ICharacterMotor
    {
        Vector3 Position { get; }
        void SetLookRotation(float pitch, float yaw);
        void SetInputDirection(Vector3 worldDirection, float speed);
    }
}
