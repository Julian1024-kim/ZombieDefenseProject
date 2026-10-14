// 매치 단계(준비, 웨이브, 게임 오버, 대기).
// 대기는 방을 만든 뒤 호스트가 시작을 누르기 전이다(값은 상태 배열 순서).

namespace ZD.Gameplay.GameFlow
{
    public enum GamePhase : byte
    {
        Preparation = 0,
        Wave = 1,
        GameOver = 2,
        Waiting = 3,
    }
}
