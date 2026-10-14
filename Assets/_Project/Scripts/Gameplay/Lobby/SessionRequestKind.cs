// Game 씬이 세션에 어떻게 들어갈지: 방 만들기(호스트), 방 참가, 개발용 자동(명령줄·Game 씬 단독 실행).

namespace ZD.Gameplay.Lobby
{
    public enum SessionRequestKind : byte
    {
        DevAuto = 0,
        Host = 1,
        Join = 2,
    }
}
