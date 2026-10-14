// 방어막 종류에서 행동을 고르는 유일한 곳.
// BarricadeKind 값 순서대로 행동 배열을 만든다.

using ZD.Core;
using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Building
{
    public static class BarricadeBehaviors
    {
        public static IBarricadeBehavior[] Create(INetworkClock clock, DamageSystem damageSystem)
        {
            var behaviors = new IBarricadeBehavior[3];
            behaviors[(int)BarricadeKind.Wall] = new WallBehavior();
            behaviors[(int)BarricadeKind.Mine] = new MineBehavior(damageSystem);
            behaviors[(int)BarricadeKind.BearTrap] = new BearTrapBehavior(clock, damageSystem);
            return behaviors;
        }
    }
}
