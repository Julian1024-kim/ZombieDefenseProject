// 웨이브가 시작될 때 호스트에서 발행되는 메시지.
// 웨이브 번호와 좀비 수를 담는다.

namespace ZD.Core.Messages
{
    public readonly struct WaveStartedMessage
    {
        public readonly int WaveNumber;
        public readonly int ZombieCount;

        public WaveStartedMessage(int waveNumber, int zombieCount)
        {
            WaveNumber = waveNumber;
            ZombieCount = zombieCount;
        }
    }
}
