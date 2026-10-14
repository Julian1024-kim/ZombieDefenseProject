// 로컬 접속 상태와 실패 이유(방 없음, 꽉 참, 비밀번호 틀림 등), 접속한 방의 코드와 내가 호스트인지.
// 대기 화면과 접속 안내에 쓴다.

using R3;
using ZD.Core;

namespace ZD.Gameplay.Hud
{
    public sealed class SessionStatusModel
    {
        public ReactiveProperty<SessionStatus> Status { get; } = new(SessionStatus.Connecting);
        public SessionStartResult FailureReason { get; set; } = SessionStartResult.Ok;
        public string RoomCode { get; set; } = string.Empty;
        public bool IsHost { get; set; }
    }
}
