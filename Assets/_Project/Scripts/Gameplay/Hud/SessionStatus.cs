// 세션 접속 상태(접속 중, 접속됨, 실패, 끊김).

namespace ZD.Gameplay.Hud
{
    public enum SessionStatus : byte
    {
        Connecting = 0,
        Connected = 1,
        Failed = 2,
        Disconnected = 3,
    }
}
