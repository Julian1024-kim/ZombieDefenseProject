// 설치 위치를 고르고 배치 규칙으로 판정한다.
// 로컬 미리보기와 호스트 검증이 이 클래스 하나를 같이 쓴다.
// 규칙 순서: Point → 설치 거리 → 경사 → 겹침(물리 검사라 가장 비싸서 마지막).

using UnityEngine;

namespace ZD.Gameplay.Building
{
    public sealed class PlacementSystem
    {
        private readonly BuildCatalog _catalog;
        private readonly IPlacementWorld _world;
        private readonly IPlacementRule _rules;

        public PlacementSystem(BuildCatalog catalog, IPlacementWorld world)
        {
            _catalog = catalog;
            _world = world;
            _rules = new CompositePlacementRule(
                new PointsRule(),
                new DistanceRule(catalog.MaxPlaceDistance),
                new SlopeRule(catalog.MaxSlopeDegrees),
                new OverlapRule());
        }

        public Vector3 ChooseTarget(Vector3 builderPosition, Vector3 aimOrigin, Vector3 aimDirection)
        {
            Vector3 flatForward = new Vector3(aimDirection.x, 0f, aimDirection.z).normalized;
            float maxDistance = _catalog.MaxPlaceDistance;
            Vector3 target = builderPosition + flatForward * maxDistance;
            if (aimDirection.y < -0.01f)
            {
                float t = (builderPosition.y - aimOrigin.y) / aimDirection.y;
                Vector3 hit = aimOrigin + aimDirection * t;
                Vector3 offset = hit - builderPosition;
                offset.y = 0f;
                target = builderPosition + Vector3.ClampMagnitude(offset, maxDistance);
            }

            return target;
        }

        public PlacementResult Evaluate(Vector3 builderPosition, Vector3 target, float yaw, BarricadeDefinition definition, int points, out Vector3 position, out Quaternion rotation)
        {
            position = target;
            rotation = Quaternion.Euler(0f, yaw, 0f);
            if (definition == null)
            {
                return PlacementResult.Unavailable;
            }

            if (!_world.TryFindGround(target, out Vector3 ground, out Vector3 normal))
            {
                return PlacementResult.NoGround;
            }

            position = ground;
            var request = new PlacementRequest(builderPosition, ground, rotation, normal, definition, points);
            return _rules.Check(request, _world);
        }
    }
}
