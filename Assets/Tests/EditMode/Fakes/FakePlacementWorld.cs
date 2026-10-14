// 테스트용 배치 월드.
// 바닥은 높이 0의 평면(법선은 바꿀 수 있음)이고, 막힘 여부를 직접 정한다.

using UnityEngine;
using ZD.Gameplay.Building;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakePlacementWorld : IPlacementWorld
    {
        public bool HasGround { get; set; } = true;
        public Vector3 GroundNormal { get; set; } = Vector3.up;
        public bool IsBlocked { get; set; }

        public bool TryFindGround(Vector3 point, out Vector3 ground, out Vector3 normal)
        {
            ground = new Vector3(point.x, 0f, point.z);
            normal = GroundNormal;
            return HasGround;
        }

        public bool IsAreaBlocked(Vector3 center, Vector3 halfExtents, Quaternion rotation) => IsBlocked;
    }
}
