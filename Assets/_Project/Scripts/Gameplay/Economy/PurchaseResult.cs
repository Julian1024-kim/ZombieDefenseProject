// 호스트가 구매 요청에 내린 결과.

namespace ZD.Gameplay.Economy
{
    public enum PurchaseResult : byte
    {
        Success = 0,
        InvalidItem = 1,
        Downed = 2,
        TooFarFromSupplyBox = 3,
        NotEnoughPoints = 4,
        AlreadyOwned = 5,
        WeaponNotOwned = 6,
        AtCapacity = 7,
        MatchOver = 8,
    }
}
