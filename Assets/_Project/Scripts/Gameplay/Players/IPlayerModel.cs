// 플레이어의 모델 계약.
// 체력 0은 쓰러진 상태이고 동료만 부활시킬 수 있다.

using UnityEngine;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Economy;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Stats;

namespace ZD.Gameplay.Players
{
    public interface IPlayerModel : IDamageable
    {
        Vector3 Position { get; }
        IWalletModel Wallet { get; }
        IInventoryModel Inventory { get; }
        IReviveModel Revive { get; }
        IPlayerStatsModel Stats { get; }

        PlayerStateId ActionState { get; set; }

        string Nickname { get; }
    }
}
