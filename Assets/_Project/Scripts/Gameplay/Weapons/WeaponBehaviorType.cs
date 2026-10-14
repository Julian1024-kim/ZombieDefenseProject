// 무기 발사 방식의 종류.
// WeaponDefinition이 이 값으로 전략을 만든다.

namespace ZD.Gameplay.Weapons
{
    public enum WeaponBehaviorType
    {
        Hitscan = 0,
        Melee = 1,
        Spread = 2,
    }
}
