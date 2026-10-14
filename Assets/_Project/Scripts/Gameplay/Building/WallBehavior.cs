// 벽(바리케이드, 콘크리트).
// 길을 막는 것은 NavMeshObstacle이 하고, 부서지는 것은 좀비 공격이 하므로 틱마다 할 일은 없다.

namespace ZD.Gameplay.Building
{
    public sealed class WallBehavior : IBarricadeBehavior
    {
        public void Tick(IBarricadeModel barricade, in BarricadeTickContext context) { }
    }
}
