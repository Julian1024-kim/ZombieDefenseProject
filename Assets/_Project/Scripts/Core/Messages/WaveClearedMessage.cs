// 웨이브의 좀비를 모두 처치했을 때 호스트에서 발행되는 메시지.

namespace ZD.Core.Messages
{
    public readonly struct WaveClearedMessage
    {
        public readonly int WaveNumber;

        public WaveClearedMessage(int waveNumber)
        {
            WaveNumber = waveNumber;
        }
    }
}
