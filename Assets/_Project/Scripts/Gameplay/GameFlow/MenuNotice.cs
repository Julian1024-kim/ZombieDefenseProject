// 로비에 띄우는 안내(참가 실패 이유, 게임에서 돌아온 이유 등).
// None이면 안내 없음.

namespace ZD.Gameplay.GameFlow
{
    public enum MenuNotice : byte
    {
        None = 0,
        RoomNotFound = 1,
        RoomFull = 2,
        WrongPassword = 3,
        ConnectFailed = 4,
        HostLeft = 5,
        InvalidCode = 6,
        LobbyFailed = 7,
    }
}
