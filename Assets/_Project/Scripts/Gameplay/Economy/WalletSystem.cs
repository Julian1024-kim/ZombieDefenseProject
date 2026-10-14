// 호스트 전용.
// Point가 바뀌는 유일한 곳이다.

namespace ZD.Gameplay.Economy
{
    public sealed class WalletSystem
    {
        public void Earn(IWalletModel wallet, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            wallet.Points += amount;
            wallet.PointsEarned += amount;
        }

        public bool TrySpend(IWalletModel wallet, int price)
        {
            if (price < 0 || wallet.Points < price)
            {
                return false;
            }

            wallet.Points -= price;
            return true;
        }

        public void Refund(IWalletModel wallet, int amount)
        {
            if (amount > 0)
            {
                wallet.Points += amount;
            }
        }

        public void Reset(IWalletModel wallet)
        {
            wallet.Points = 0;
            wallet.PointsEarned = 0;
        }
    }
}
