// 방 목록 한 줄: 방 코드(세션 이름), 방 이름, 방장 닉네임, 모드, 인원, 잠금(비밀번호) 여부, 들어갈 수 있는지.

using ZD.Gameplay.GameFlow;

namespace ZD.Gameplay.Lobby
{
    public readonly struct RoomInfo
    {
        public readonly string Code;
        public readonly string Name;
        public readonly string HostName;
        public readonly GameModeId Mode;
        public readonly int PlayerCount;
        public readonly int MaxPlayers;
        public readonly bool IsLocked;
        public readonly bool IsOpen;

        public RoomInfo(string code, string name, string hostName, GameModeId mode, int playerCount, int maxPlayers, bool isLocked, bool isOpen)
        {
            Code = code;
            Name = name;
            HostName = hostName;
            Mode = mode;
            PlayerCount = playerCount;
            MaxPlayers = maxPlayers;
            IsLocked = isLocked;
            IsOpen = isOpen;
        }

        public bool CanJoin => IsOpen && PlayerCount < MaxPlayers;
    }
}
