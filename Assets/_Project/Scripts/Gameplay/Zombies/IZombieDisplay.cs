// 좀비 하나의 표시용 상태.
// 복제된 값으로 로컬에서 갱신되고, View가 구독한다.

using R3;

namespace ZD.Gameplay.Zombies
{
    public interface IZombieDisplay
    {
        ReadOnlyReactiveProperty<ZombieStateId> DisplayState { get; }
        ReadOnlyReactiveProperty<int> DisplayHealth { get; }

        ReadOnlyReactiveProperty<float> DisplaySpeed { get; }

        ReadOnlyReactiveProperty<int> DisplayVariant { get; }
    }
}
