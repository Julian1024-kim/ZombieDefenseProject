// 거점 체력 막대의 로컬 표시용 값.
// 거점 네트워크 오브젝트가 Render에서 쓰고, View가 구독한다.
// 거점이 없으면 숨긴다.

using R3;

namespace ZD.Gameplay.Hud
{
    public sealed class BaseHudModel
    {
        public ReactiveProperty<bool> IsVisible { get; } = new(false);
        public ReactiveProperty<int> Health { get; } = new(0);
        public ReactiveProperty<int> MaxHealth { get; } = new(1);
    }
}
