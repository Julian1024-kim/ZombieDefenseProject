// 로컬 전용 상점 창 상태.

using R3;

namespace ZD.Gameplay.Economy
{
    public sealed class ShopUiModel
    {
        public ReactiveProperty<bool> IsNearSupplyBox { get; } = new(false);
        public ReactiveProperty<bool> IsOpen { get; } = new(false);

        public Subject<PurchaseResult> PurchaseResults { get; } = new();

        public IShopRequestSender RequestSender { get; set; }
    }
}
