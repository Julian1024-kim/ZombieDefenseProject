// 호스트에서 쓰러진 플레이어가 동료에게 부활했을 때 발행되는 메시지.
// 게임 오버 통계(부활시킨 횟수)에 쓴다.

namespace ZD.Core.Messages
{
    public readonly struct PlayerRevivedMessage
    {
        public readonly int ReviverId;
        public readonly int RevivedId;

        public PlayerRevivedMessage(int reviverId, int revivedId)
        {
            ReviverId = reviverId;
            RevivedId = revivedId;
        }
    }
}
