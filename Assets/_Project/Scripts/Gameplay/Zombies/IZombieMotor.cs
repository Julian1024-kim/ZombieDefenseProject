// 좀비 하나의 이동 계약.
// 호스트에서 NavMeshAgent로 구현한다.

using UnityEngine;

namespace ZD.Gameplay.Zombies
{
    public interface IZombieMotor
    {
        void MoveTowards(Vector3 destination, float speed);
        void Stop();
        void FaceTowards(Vector3 point);

        bool IsPathBlocked { get; }
    }
}
