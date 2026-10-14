// 방 목록 서버 접속 상태와 공개된 방 목록(모든 모드).
// 로비 서비스가 쓰고, View는 고른 모드의 방만 보인다.
// 목록이 바뀌면 RoomsVersion이 오른다.

using System.Collections.Generic;
using R3;

namespace ZD.Gameplay.Lobby
{
    public sealed class LobbyModel
    {
        public ReactiveProperty<LobbyStatus> Status { get; } = new(LobbyStatus.Disconnected);
        public ReactiveProperty<int> RoomsVersion { get; } = new(0);
        public List<RoomInfo> Rooms { get; } = new();

        public bool TryFind(string code, out RoomInfo room)
        {
            for (int roomIndex = 0; roomIndex < Rooms.Count; roomIndex++)
            {
                if (Rooms[roomIndex].Code == code)
                {
                    room = Rooms[roomIndex];
                    return true;
                }
            }

            room = default;
            return false;
        }
    }
}
