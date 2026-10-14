// 쓰러지지 않은 플레이어 중 가장 가까운 사람을 노린다(생존 모드).

using ZD.Gameplay.Players;

namespace ZD.Gameplay.Zombies
{
    public sealed class NearestPlayerTargetSelector : ITargetSelector
    {
        private readonly PlayerRosterModel _players;

        public NearestPlayerTargetSelector(PlayerRosterModel players)
        {
            _players = players;
        }

        public ZombieTarget Select(IZombieModel zombie)
        {
            IPlayerModel player = FindNearestStanding(zombie, out float sqrDistance);
            return player == null ? ZombieTarget.None : ToTarget(player, sqrDistance);
        }

        public IPlayerModel FindNearestStanding(IZombieModel zombie, out float nearestSqrDistance)
        {
            IPlayerModel nearest = null;
            nearestSqrDistance = float.MaxValue;
            for (int playerIndex = 0; playerIndex < _players.Count; playerIndex++)
            {
                IPlayerModel player = _players.Get(playerIndex);
                if (player.Health <= 0)
                {
                    continue;
                }

                float sqrDistance = (player.Position - zombie.Position).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearest = player;
                }
            }

            return nearest;
        }

        public static ZombieTarget ToTarget(IPlayerModel player, float sqrDistance)
        {
            return new ZombieTarget(ZombieTargetKind.Player, player, player.Position, player.Position, sqrDistance);
        }
    }
}
