// 무기 한 칸의 복제 상태(탄창, 예비 탄약, 다음 발사 틱, 재장전 종료 틱).

using Fusion;

namespace ZD.Network.Players
{
    public struct WeaponSlotState : INetworkStruct
    {
        public int AmmoInMagazine;
        public int ReserveAmmo;
        public int NextFireTick;
        public int ReloadEndTick;
    }
}
