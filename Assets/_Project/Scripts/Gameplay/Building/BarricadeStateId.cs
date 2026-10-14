// 방어막 상태.
// 클라이언트가 겉모습을 정할 수 있게 byte로 동기화된다.
// 설치 중 → 완성, 완성 → 부서짐, 설치 중 → 취소(Point 환불).

namespace ZD.Gameplay.Building
{
    public enum BarricadeStateId : byte
    {
        Installing = 0,
        Active = 1,
        Destroyed = 2,
        Cancelled = 3,
    }
}
