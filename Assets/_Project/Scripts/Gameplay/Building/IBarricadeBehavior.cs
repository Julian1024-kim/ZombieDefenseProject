// 완성된 방어막이 매 틱 하는 일(Strategy).
// 벽은 아무것도 안 하고, 지뢰는 폭발, 곰덫은 묶는다.
// 인스턴스를 모든 방어막이 공유하므로 방어막별 데이터는 모두 모델에 있다.

namespace ZD.Gameplay.Building
{
    public interface IBarricadeBehavior
    {
        void Tick(IBarricadeModel barricade, in BarricadeTickContext context);
    }
}
