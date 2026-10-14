// 플레이어가 체력 0이 되어 쓰러졌을 때 호스트에서 발행되는 메시지.

namespace ZD.Core.Messages
{
    public readonly struct PlayerDownedMessage
    {
        public readonly int PlayerId;

        public PlayerDownedMessage(int playerId)
        {
            PlayerId = playerId;
        }
    }
}
