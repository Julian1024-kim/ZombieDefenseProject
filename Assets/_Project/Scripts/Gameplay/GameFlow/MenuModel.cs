// 시작 화면 상태: 보이는지(게임 중이면 숨김), 지금 화면, 불러오는 중인지, 안내 문구, 비밀번호를 물어볼 방.

using R3;
using ZD.Gameplay.Lobby;

namespace ZD.Gameplay.GameFlow
{
    public sealed class MenuModel
    {
        public ReactiveProperty<bool> IsVisible { get; } = new(true);
        public ReactiveProperty<bool> IsLoading { get; } = new(false);
        public ReactiveProperty<MenuScreen> Screen { get; } = new(MenuScreen.Main);
        public ReactiveProperty<MenuNotice> Notice { get; } = new(MenuNotice.None);
        public RoomInfo PendingRoom { get; set; }
    }
}
