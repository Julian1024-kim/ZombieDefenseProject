// 한 틱의 무기 입력.
// FirePressed는 버튼이 눌린 틱에만 true다.

using UnityEngine;

namespace ZD.Gameplay.Weapons
{
    public readonly struct WeaponCommand
    {
        public readonly bool FireHeld;
        public readonly bool FirePressed;
        public readonly bool ReloadPressed;
        public readonly Vector3 AimOrigin;
        public readonly Vector3 AimDirection;

        public WeaponCommand(bool fireHeld, bool firePressed, bool reloadPressed, Vector3 aimOrigin, Vector3 aimDirection)
        {
            FireHeld = fireHeld;
            FirePressed = firePressed;
            ReloadPressed = reloadPressed;
            AimOrigin = aimOrigin;
            AimDirection = aimDirection;
        }
    }
}
