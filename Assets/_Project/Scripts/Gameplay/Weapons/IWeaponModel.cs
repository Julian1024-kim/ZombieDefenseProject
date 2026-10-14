// 장착한 무기 하나의 모델 계약.
// 틱 값은 절대 시뮬레이션 틱이고 0은 미설정이다.

namespace ZD.Gameplay.Weapons
{
    public interface IWeaponModel
    {
        int AmmoInMagazine { get; set; }
        int ReserveAmmo { get; set; }
        int NextFireTick { get; set; }
        int ReloadEndTick { get; set; }
    }
}
