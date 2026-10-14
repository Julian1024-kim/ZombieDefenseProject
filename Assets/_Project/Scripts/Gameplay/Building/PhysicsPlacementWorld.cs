// 실제 게임의 배치 판정기.
// 바닥은 레이캐스트로, 겹침은 상자 겹침 검사로 찾는다.
// 결과 버퍼를 미리 만들어 할당이 없다.
// 바닥은 목표 지점 0.5m 위에서 아래로 찾으므로, 엄폐물(1.5m) 위에는 설치되지 않는다.

using UnityEngine;

namespace ZD.Gameplay.Building
{
    public sealed class PhysicsPlacementWorld : IPlacementWorld
    {
        private const float GROUND_PROBE_HEIGHT = 0.5f;
        private const float GROUND_PROBE_DEPTH = 2f;

        private readonly BuildCatalog _catalog;
        private readonly Collider[] _overlapBuffer = new Collider[1];

        public PhysicsPlacementWorld(BuildCatalog catalog)
        {
            _catalog = catalog;
        }

        public bool TryFindGround(Vector3 point, out Vector3 ground, out Vector3 normal)
        {
            Vector3 origin = point + Vector3.up * GROUND_PROBE_HEIGHT;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, GROUND_PROBE_HEIGHT + GROUND_PROBE_DEPTH, _catalog.GroundMask, QueryTriggerInteraction.Ignore))
            {
                ground = hit.point;
                normal = hit.normal;
                return true;
            }

            ground = point;
            normal = Vector3.up;
            return false;
        }

        public bool IsAreaBlocked(Vector3 center, Vector3 halfExtents, Quaternion rotation)
        {
            return Physics.OverlapBoxNonAlloc(center, halfExtents, _overlapBuffer, rotation, _catalog.BlockingMask, QueryTriggerInteraction.Collide) > 0;
        }
    }
}
