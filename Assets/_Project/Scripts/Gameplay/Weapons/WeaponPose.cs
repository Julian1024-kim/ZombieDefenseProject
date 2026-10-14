// 손에 든 무기에 따른 상체 자세.
// 애니메이션이 무기 종류를 직접 알지 않도록 이 값만 본다.

namespace ZD.Gameplay.Weapons
{
    public enum WeaponPose : byte
    {
        None = 0,
        Rifle = 1,
        Pistol = 2,
        Knife = 3,
    }
}
