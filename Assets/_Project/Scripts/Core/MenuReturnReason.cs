// 게임에서 시작 화면(로비)으로 돌아온 이유.
// 로비가 안내 문구를 고르는 데 쓴다.

namespace ZD.Core
{
    public enum MenuReturnReason : byte
    {
        Left = 0,
        HostLeft = 1,
        RoomNotFound = 2,
        RoomFull = 3,
        WrongPassword = 4,
        ConnectFailed = 5,
    }
}
