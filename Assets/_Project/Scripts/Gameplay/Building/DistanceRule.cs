// 설치 거리 규칙: 설치자에서 설치 위치까지 수평 거리가 최대 거리 이내여야 한다.

using UnityEngine;

namespace ZD.Gameplay.Building
{
    public sealed class DistanceRule : IPlacementRule
    {
        private readonly float _maxSqrDistance;

        public DistanceRule(float maxDistance)
        {
            _maxSqrDistance = maxDistance * maxDistance;
        }

        public PlacementResult Check(in PlacementRequest request, IPlacementWorld world)
        {
            Vector3 offset = request.Position - request.BuilderPosition;
            offset.y = 0f;
            return offset.sqrMagnitude <= _maxSqrDistance ? PlacementResult.Valid : PlacementResult.TooFar;
        }
    }
}
