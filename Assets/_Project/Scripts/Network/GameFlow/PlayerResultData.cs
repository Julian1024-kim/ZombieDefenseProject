// 결과 한 줄의 네트워크 표현(PlayerResult와 같은 값, 닉네임은 16자 고정 문자열).
// 게임 오버 때 한 번 써서 변경분만 나간다.

using Fusion;
using ZD.Gameplay.Stats;

namespace ZD.Network.GameFlow
{
    public struct PlayerResultData : INetworkStruct
    {
        public int PlayerNumber;
        public NetworkString<_16> Name;
        public int Kills;
        public int Headshots;
        public int Revives;
        public int PointsEarned;

        public static PlayerResultData From(in PlayerResult result)
        {
            return new PlayerResultData
            {
                PlayerNumber = result.PlayerNumber,
                Name = result.Name,
                Kills = result.Kills,
                Headshots = result.Headshots,
                Revives = result.Revives,
                PointsEarned = result.PointsEarned,
            };
        }

        public PlayerResult ToResult() => new(PlayerNumber, Name.ToString(), Kills, Headshots, Revives, PointsEarned);
    }
}
