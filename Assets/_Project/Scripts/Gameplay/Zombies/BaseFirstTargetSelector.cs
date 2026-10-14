// 거점을 먼저 노린다(거점 방어 모드).
// 단, 정해진 거리(기본 6m) 안에 서 있는 플레이어가 있으면 그 플레이어를 공격한다.
// 거점이 없거나 부서졌으면 가장 가까운 플레이어를 노린다.

using ZD.Gameplay.Defense;
using ZD.Gameplay.Players;

namespace ZD.Gameplay.Zombies
{
    public sealed class BaseFirstTargetSelector : ITargetSelector
    {
        // 좀비가 거점 바닥에서 이만큼 떨어진 곳까지 걸어간다(거점이 NavMesh를 깎은 테두리 바깥, 공격 범위 안).
        private const float APPROACH_MARGIN = 0.6f;

        private readonly NearestPlayerTargetSelector _nearestPlayer;
        private readonly BaseRosterModel _bases;

        public BaseFirstTargetSelector(NearestPlayerTargetSelector nearestPlayer, BaseRosterModel bases)
        {
            _nearestPlayer = nearestPlayer;
            _bases = bases;
        }

        public ZombieTarget Select(IZombieModel zombie)
        {
            IPlayerModel player = _nearestPlayer.FindNearestStanding(zombie, out float sqrDistanceToPlayer);
            IBaseModel baseModel = _bases.Current;
            bool isBaseAlive = baseModel != null && baseModel.Health > 0;
            if (player != null && (!isBaseAlive || sqrDistanceToPlayer <= Square(baseModel.Definition.PlayerAggroRadius)))
            {
                return NearestPlayerTargetSelector.ToTarget(player, sqrDistanceToPlayer);
            }

            if (!isBaseAlive)
            {
                return ZombieTarget.None;
            }

            return new ZombieTarget(
                ZombieTargetKind.Base,
                baseModel,
                BaseRosterModel.ApproachPoint(baseModel, zombie.Position, APPROACH_MARGIN),
                baseModel.Position,
                BaseRosterModel.SqrDistanceToFootprint(baseModel, zombie.Position));
        }

        private static float Square(float value) => value * value;
    }
}
