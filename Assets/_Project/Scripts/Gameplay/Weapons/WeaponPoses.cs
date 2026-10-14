// 무기 정의에서 상체 자세를 고르는 유일한 곳.
// 주무기는 소총 자세, 보조무기는 권총 자세, 근접 무기(칼, 도끼, 마체테)는 칼 자세다.

namespace ZD.Gameplay.Weapons
{
    public static class WeaponPoses
    {
        public static WeaponPose From(WeaponDefinition weapon)
        {
            if (weapon == null)
            {
                return WeaponPose.None;
            }

            return weapon.Category switch
            {
                WeaponCategory.Primary => WeaponPose.Rifle,
                WeaponCategory.Secondary => WeaponPose.Pistol,
                WeaponCategory.Melee => WeaponPose.Knife,
                _ => WeaponPose.None,
            };
        }
    }
}
