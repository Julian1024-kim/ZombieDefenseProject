// 겹침 규칙: 방어막이 차지할 상자 영역에 벽, 다른 방어막, 플레이어, 좀비가 없어야 한다.
// 바닥 바로 위만 본다.

using UnityEngine;

namespace ZD.Gameplay.Building
{
    public sealed class OverlapRule : IPlacementRule
    {
        // 바닥 자체와 닿아서 막혔다고 판정되지 않도록 상자를 바닥에서 살짝 띄운다.
        private const float GROUND_CLEARANCE = 0.05f;

        public PlacementResult Check(in PlacementRequest request, IPlacementWorld world)
        {
            Vector3 size = request.Definition.Size;
            Vector3 halfExtents = new(size.x * 0.5f, Mathf.Max(0.05f, size.y * 0.5f - GROUND_CLEARANCE), size.z * 0.5f);
            Vector3 center = request.Position + Vector3.up * (size.y * 0.5f + GROUND_CLEARANCE);
            return world.IsAreaBlocked(center, halfExtents, request.Rotation) ? PlacementResult.Blocked : PlacementResult.Valid;
        }
    }
}
