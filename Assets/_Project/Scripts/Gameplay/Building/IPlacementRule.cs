// 배치 규칙 하나(Specification).
// 미리보기와 호스트 검증이 같은 규칙 객체를 써서 결과가 어긋나지 않는다.

namespace ZD.Gameplay.Building
{
    public interface IPlacementRule
    {
        PlacementResult Check(in PlacementRequest request, IPlacementWorld world);
    }
}
