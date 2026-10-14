// 무기 발사 방식 전략(즉시 판정, 산탄, 근접 등).
// 무기 종류마다 구현을 바꿔 끼운다.

using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Weapons
{
    public interface IWeaponBehavior
    {
        void Fire(WeaponDefinition definition, int shooterId, in WeaponCommand command, IHitQuery hitQuery, DamageSystem damageSystem);
    }
}
