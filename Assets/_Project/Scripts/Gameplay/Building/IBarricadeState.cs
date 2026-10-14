// 방어막 상태 하나.
// 인스턴스를 모든 방어막이 공유하고, Tick은 다음 상태를 돌려준다.
// 전환은 방어막 시스템이 한다.

namespace ZD.Gameplay.Building
{
    public interface IBarricadeState
    {
        BarricadeStateId Tick(IBarricadeModel barricade, in BarricadeTickContext context);
    }
}
