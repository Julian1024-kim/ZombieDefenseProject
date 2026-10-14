// 결과 화면 한 줄: 게임 오버 순간에 확정한 플레이어 하나의 닉네임과 통계.
// PlayerNumber는 접속 순번(정렬용)이다.

namespace ZD.Gameplay.Stats
{
    public readonly struct PlayerResult
    {
        public readonly int PlayerNumber;
        public readonly string Name;
        public readonly int Kills;
        public readonly int Headshots;
        public readonly int Revives;
        public readonly int PointsEarned;

        public PlayerResult(int playerNumber, string name, int kills, int headshots, int revives, int pointsEarned)
        {
            PlayerNumber = playerNumber;
            Name = name ?? string.Empty;
            Kills = kills;
            Headshots = headshots;
            Revives = revives;
            PointsEarned = pointsEarned;
        }
    }
}
