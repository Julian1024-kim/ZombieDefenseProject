// 현재 세션에 있는 플레이어 명단.
// 네트워크 계층이 플레이어 오브젝트가 스폰될 때 등록한다.

using System.Collections.Generic;

namespace ZD.Gameplay.Players
{
    public sealed class PlayerRosterModel
    {
        private readonly List<IPlayerModel> _players = new(4);

        public int Count => _players.Count;

        public IPlayerModel Get(int index) => _players[index];

        public IPlayerModel FindById(int entityId)
        {
            for (int playerIndex = 0; playerIndex < _players.Count; playerIndex++)
            {
                if (_players[playerIndex].EntityId == entityId)
                {
                    return _players[playerIndex];
                }
            }

            return null;
        }

        public void Register(IPlayerModel player)
        {
            if (!_players.Contains(player))
            {
                _players.Add(player);
            }
        }

        public void Unregister(IPlayerModel player)
        {
            _players.Remove(player);
        }
    }
}
