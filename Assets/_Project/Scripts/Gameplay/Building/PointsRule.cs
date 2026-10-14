// Point 규칙: 설치자가 가격만큼 Point를 갖고 있어야 한다.

namespace ZD.Gameplay.Building
{
    public sealed class PointsRule : IPlacementRule
    {
        public PlacementResult Check(in PlacementRequest request, IPlacementWorld world)
        {
            return request.Points >= request.Definition.Price ? PlacementResult.Valid : PlacementResult.NotEnoughPoints;
        }
    }
}
