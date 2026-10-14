// 방어막 하나의 표시용 상태.
// 복제된 값으로 각 피어가 Render에서 갱신하고, View가 구독한다.

using R3;

namespace ZD.Gameplay.Building
{
    public interface IBarricadeDisplay
    {
        BarricadeDefinition Definition { get; }
        ReadOnlyReactiveProperty<BarricadeStateId> DisplayState { get; }
        ReadOnlyReactiveProperty<int> DisplayHealth { get; }
    }
}
