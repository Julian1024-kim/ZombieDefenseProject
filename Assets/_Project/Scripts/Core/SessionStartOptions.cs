// 세션을 시작하는 데 필요한 값.
// 방 이름·방장 닉네임·모드·잠금 여부는 방 목록에 보이는 정보이고, 비밀번호는 목록에 싣지 않는다(호스트는 확인용으로 들고 있고, 참가자는 접속할 때 보낸다).

namespace ZD.Core
{
    public readonly struct SessionStartOptions
    {
        public readonly SessionRole Role;
        public readonly string SessionName;
        public readonly bool IsVisible;
        public readonly string RoomName;
        public readonly string HostName;
        public readonly int ModeIndex;
        public readonly string Password;

        public SessionStartOptions(SessionRole role, string sessionName, bool isVisible, string roomName, string hostName, int modeIndex, string password)
        {
            Role = role;
            SessionName = sessionName;
            IsVisible = isVisible;
            RoomName = roomName;
            HostName = hostName;
            ModeIndex = modeIndex;
            Password = password ?? string.Empty;
        }

        public bool IsLocked => Password.Length > 0;
    }
}
