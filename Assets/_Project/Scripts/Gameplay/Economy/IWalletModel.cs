// 플레이어 Point의 모델 계약.
// PointsEarned는 오르기만 하고, 점수 계산에 쓰여서 Point를 써도 손해가 없다.

namespace ZD.Gameplay.Economy
{
    public interface IWalletModel
    {
        int Points { get; set; }
        int PointsEarned { get; set; }
    }
}
