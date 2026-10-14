// 끝난 상태(부서짐, 취소).
// 네트워크 계층이 잠시 뒤 오브젝트를 제거하므로 할 일이 없다.

namespace ZD.Gameplay.Building
{
    public sealed class FinishedBarricadeState : IBarricadeState
    {
        public BarricadeStateId Tick(IBarricadeModel barricade, in BarricadeTickContext context) => barricade.State;
    }
}
