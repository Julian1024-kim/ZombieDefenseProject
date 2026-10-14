// 완성된 방어막이 부서졌다(벽은 좀비가 부숨, 지뢰는 폭발, 곰덫은 횟수 소진).
// 호스트에서 발행한다.

namespace ZD.Core.Messages
{
    public readonly struct BarricadeDestroyedMessage
    {
        public readonly int BarricadeId;

        public BarricadeDestroyedMessage(int barricadeId)
        {
            BarricadeId = barricadeId;
        }
    }
}
