// 경사 규칙: 바닥이 최대 경사보다 기울어 있으면 설치할 수 없다.

using UnityEngine;

namespace ZD.Gameplay.Building
{
    public sealed class SlopeRule : IPlacementRule
    {
        private readonly float _maxSlopeDegrees;

        public SlopeRule(float maxSlopeDegrees)
        {
            _maxSlopeDegrees = maxSlopeDegrees;
        }

        public PlacementResult Check(in PlacementRequest request, IPlacementWorld world)
        {
            return Vector3.Angle(request.GroundNormal, Vector3.up) <= _maxSlopeDegrees ? PlacementResult.Valid : PlacementResult.TooSteep;
        }
    }
}
