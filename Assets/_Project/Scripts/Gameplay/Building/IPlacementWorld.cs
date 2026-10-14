// 배치 판정이 묻는 월드 정보.
// 실제 게임은 물리 쿼리로, 테스트는 가짜로 구현한다.

using UnityEngine;

namespace ZD.Gameplay.Building
{
    public interface IPlacementWorld
    {
        bool TryFindGround(Vector3 point, out Vector3 ground, out Vector3 normal);

        bool IsAreaBlocked(Vector3 center, Vector3 halfExtents, Quaternion rotation);
    }
}
