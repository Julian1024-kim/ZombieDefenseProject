// 플레이어 하나의 표시용 상태.
// 복제된 값으로 각 피어가 Render에서 갱신하고, 애니메이션 View가 구독한다.

using R3;
using UnityEngine;
using ZD.Gameplay.Weapons;

namespace ZD.Gameplay.Players
{
    public interface IPlayerDisplay
    {
        ReadOnlyReactiveProperty<Vector2> DisplayMove { get; }
        ReadOnlyReactiveProperty<bool> DisplayDowned { get; }
        ReadOnlyReactiveProperty<bool> DisplayReviving { get; }
        ReadOnlyReactiveProperty<bool> DisplayHealing { get; }
        ReadOnlyReactiveProperty<bool> DisplayReloading { get; }

        ReadOnlyReactiveProperty<bool> DisplayBuilding { get; }
        ReadOnlyReactiveProperty<WeaponPose> DisplayPose { get; }

        ReadOnlyReactiveProperty<byte> DisplayWeaponId { get; }

        ReadOnlyReactiveProperty<int> DisplayShotCount { get; }

        ReadOnlyReactiveProperty<float> DisplayAimPitch { get; }

        ReadOnlyReactiveProperty<int> DisplayVisualIndex { get; }
    }
}
