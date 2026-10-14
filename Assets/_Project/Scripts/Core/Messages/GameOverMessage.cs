// 게임 오버 시 호스트에서 발행되는 메시지.
// 클리어한 웨이브 수를 담는다.

namespace ZD.Core.Messages
{
    public readonly struct GameOverMessage
    {
        public readonly int WavesCleared;

        public GameOverMessage(int wavesCleared)
        {
            WavesCleared = wavesCleared;
        }
    }
}
