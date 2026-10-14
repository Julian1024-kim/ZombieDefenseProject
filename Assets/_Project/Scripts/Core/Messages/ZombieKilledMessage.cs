// 좀비가 처치됐을 때 호스트에서 발행되는 메시지.
// 처치자와 헤드샷 여부를 담는다.

namespace ZD.Core.Messages
{
    public readonly struct ZombieKilledMessage
    {
        public readonly int ZombieId;
        public readonly int KillerId;
        public readonly bool IsHeadshot;

        public ZombieKilledMessage(int zombieId, int killerId, bool isHeadshot)
        {
            ZombieId = zombieId;
            KillerId = killerId;
            IsHeadshot = isHeadshot;
        }
    }
}
