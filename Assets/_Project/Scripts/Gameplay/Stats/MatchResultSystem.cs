// 호스트 전용.
// 게임 오버 순간의 통계로 결과(팀 점수, 클리어한 웨이브, 진 이유, 플레이어별 줄)를 확정한다.
// 확정한 뒤에는 통계가 더 바뀌어도(남은 함정 처치 등) 결과는 그대로다.

using UnityEngine;
using ZD.Gameplay.Defense;
using ZD.Gameplay.Players;

namespace ZD.Gameplay.Stats
{
    public sealed class MatchResultSystem
    {
        public const int MAX_PLAYERS = 4;

        private readonly PlayerRosterModel _players;
        private readonly BaseRosterModel _bases;

        public MatchResultSystem(PlayerRosterModel players, BaseRosterModel bases)
        {
            _players = players;
            _bases = bases;
        }

        public void Record(IMatchResultModel result, int wavesCleared)
        {
            int playerCount = Mathf.Min(_players.Count, MAX_PLAYERS);
            int totalPoints = 0;
            for (int playerIndex = 0; playerIndex < playerCount; playerIndex++)
            {
                IPlayerModel player = _players.Get(playerIndex);
                int earned = player.Wallet.PointsEarned;
                totalPoints += earned;
                result.SetPlayer(playerIndex, new PlayerResult(player.EntityId, player.Nickname, player.Stats.Kills, player.Stats.Headshots, player.Stats.Revives, earned));
            }

            SortByPlayerNumber(result, playerCount);
            result.PlayerCount = playerCount;
            result.WavesCleared = wavesCleared;
            result.IsBaseDestroyed = _bases.Current != null && _bases.Current.Health <= 0;
            result.TeamScore = ScoreRules.TeamScore(wavesCleared, totalPoints);
            result.Version++;
        }

        private static void SortByPlayerNumber(IMatchResultModel result, int count)
        {
            for (int index = 1; index < count; index++)
            {
                PlayerResult current = result.GetPlayer(index);
                int insertAt = index;
                while (insertAt > 0 && result.GetPlayer(insertAt - 1).PlayerNumber > current.PlayerNumber)
                {
                    result.SetPlayer(insertAt, result.GetPlayer(insertAt - 1));
                    insertAt--;
                }

                result.SetPlayer(insertAt, current);
            }
        }
    }
}
