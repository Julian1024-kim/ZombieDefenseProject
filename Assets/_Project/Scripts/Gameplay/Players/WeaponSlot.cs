// 숫자 키 1~5에 대응하는 슬롯(주무기, 보조무기, 근접, 회복, 투척).

namespace ZD.Gameplay.Players
{
    public enum WeaponSlot : byte
    {
        None = 0,
        Primary = 1,
        Secondary = 2,
        Melee = 3,
        Healing = 4,
        Throwable = 5,
    }
}
