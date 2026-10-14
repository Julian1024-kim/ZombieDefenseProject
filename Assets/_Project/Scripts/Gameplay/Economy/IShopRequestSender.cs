// 로컬 플레이어의 구매 요청을 호스트로 보낸다.
// 로컬 플레이어 오브젝트가 구현한다.

namespace ZD.Gameplay.Economy
{
    public interface IShopRequestSender
    {
        void SendPurchaseRequest(int itemIndex);
    }
}
