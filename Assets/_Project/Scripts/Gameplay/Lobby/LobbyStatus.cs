// 방 목록 서버 접속 상태.

namespace ZD.Gameplay.Lobby
{
    public enum LobbyStatus : byte
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Failed = 3,
    }
}
