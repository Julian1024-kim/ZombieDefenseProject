// 플레이어가 모두 쓰러지면 충족되는 패배 조건.

using ZD.Gameplay.Players;

namespace ZD.Gameplay.GameFlow
{
    public sealed class AllPlayersDownedCondition : IDefeatCondition
    {
        private readonly PlayerRosterModel _players;

        public AllPlayersDownedCondition(PlayerRosterModel players)
        {
            _players = players;
        }

        public bool IsMet()
        {
            if (_players.Count == 0)
            {
                return false;
            }

            for (int playerIndex = 0; playerIndex < _players.Count; playerIndex++)
            {
                if (_players.Get(playerIndex).Health > 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
