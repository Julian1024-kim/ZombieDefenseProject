// 이 클라이언트가 조종하는 플레이어의 표시용 상태.

using R3;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Players;

namespace ZD.Gameplay.Hud
{
    public sealed class LocalPlayerHudModel
    {
        public ReactiveProperty<int> Health { get; } = new(100);
        public ReactiveProperty<int> MaxHealth { get; } = new(100);
        public ReactiveProperty<int> Points { get; } = new(0);
        public ReactiveProperty<WeaponSlot> SelectedSlot { get; } = new(WeaponSlot.Secondary);
        public ReactiveProperty<int> ActivePrimary { get; } = new(InventorySlots.PRIMARY_A);
        public ReactiveProperty<byte> PrimaryAId { get; } = new(0);
        public ReactiveProperty<byte> PrimaryBId { get; } = new(0);
        public ReactiveProperty<byte> SecondaryId { get; } = new(0);
        public ReactiveProperty<byte> MeleeId { get; } = new(0);
        public ReactiveProperty<int> HealKits { get; } = new(0);
        public ReactiveProperty<bool> IsHealing { get; } = new(false);
        public ReactiveProperty<int> AmmoInMagazine { get; } = new(0);
        public ReactiveProperty<int> ReserveAmmo { get; } = new(0);
        public ReactiveProperty<bool> IsReloading { get; } = new(false);
        public ReactiveProperty<bool> UsesAmmo { get; } = new(true);

        public ReactiveProperty<float> ReviveProgress { get; } = new(-1f);

        public ReactiveProperty<float> BeingRevivedProgress { get; } = new(-1f);

        public ReactiveProperty<bool> CanReviveNearby { get; } = new(false);

        public ReactiveProperty<float> BuildProgress { get; } = new(-1f);
    }
}
