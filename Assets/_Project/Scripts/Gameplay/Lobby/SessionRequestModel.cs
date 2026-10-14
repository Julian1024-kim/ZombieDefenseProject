// 로비에서 정한 '어느 방에 어떻게 들어갈지'.
// 앱 전체(Root)에 하나 있고, Game 씬의 SessionEntryPoint가 읽어 접속한다.
// 호스트의 비밀번호는 참가 요청을 확인하는 데만 쓰고 방 목록에는 싣지 않는다.

namespace ZD.Gameplay.Lobby
{
    public sealed class SessionRequestModel
    {
        public SessionRequestKind Kind { get; set; } = SessionRequestKind.DevAuto;
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
    }
}
