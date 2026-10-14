// 테스트용 지갑 모델.

using ZD.Gameplay.Economy;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeWallet : IWalletModel
    {
        public int Points { get; set; }
        public int PointsEarned { get; set; }
    }
}
