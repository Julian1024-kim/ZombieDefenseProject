// 완성 상태.
// 종류별 행동(벽, 지뢰, 곰덫)을 돌리고, 체력이 0이 되면 부서진다.
// 어떤 행동을 쓸지는 BarricadeBehaviors에서 한 번만 고른다.

namespace ZD.Gameplay.Building
{
    public sealed class ActiveBarricadeState : IBarricadeState
    {
        private readonly IBarricadeBehavior[] _behaviors;

        public ActiveBarricadeState(IBarricadeBehavior[] behaviors)
        {
            _behaviors = behaviors;
        }

        public BarricadeStateId Tick(IBarricadeModel barricade, in BarricadeTickContext context)
        {
            if (barricade.Health > 0)
            {
                _behaviors[(int)barricade.Definition.Kind].Tick(barricade, context);
            }

            return barricade.Health > 0 ? BarricadeStateId.Active : BarricadeStateId.Destroyed;
        }
    }
}
