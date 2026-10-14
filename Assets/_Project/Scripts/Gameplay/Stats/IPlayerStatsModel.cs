// 플레이어 하나의 매치 통계(처치, 헤드샷, 부활시킨 횟수).
// 네트워크 계층이 [Networked] 값으로 구현하고, 재시작하면 0이 된다.
// 획득 Point는 지갑(IWalletModel.PointsEarned)에 있다.

namespace ZD.Gameplay.Stats
{
    public interface IPlayerStatsModel
    {
        int Kills { get; set; }
        int Headshots { get; set; }
        int Revives { get; set; }
    }
}
