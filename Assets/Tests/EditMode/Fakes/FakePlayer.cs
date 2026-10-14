// 테스트용 플레이어 모델(지갑, 인벤토리, 통계 포함).

using UnityEngine;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Economy;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Players;
using ZD.Gameplay.Stats;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakePlayer : IPlayerModel, IReviveModel, IPlayerStatsModel
    {
        public FakePlayer(int id, int health = 100, Vector3 position = default)
        {
            EntityId = id;
            MaxHealth = 100;
            Health = health;
            Position = position;
        }

        public int EntityId { get; }
        public DamageTargetKind Kind => DamageTargetKind.Player;
        public int Health { get; set; }
        public int MaxHealth { get; }
        public Vector3 Position { get; set; }
        public FakeWallet FakeWallet { get; } = new();
        public FakeInventory FakeInventory { get; } = new();
        public IWalletModel Wallet => FakeWallet;
        public IInventoryModel Inventory => FakeInventory;
        public IReviveModel Revive => this;
        public IPlayerStatsModel Stats => this;
        public string Nickname { get; set; } = string.Empty;
        public PlayerStateId ActionState { get; set; }
        public int Kills { get; set; }
        public int Headshots { get; set; }
        public int Revives { get; set; }
        public int ReviveTargetId { get; set; } = -1;
        public int ReviveEndTick { get; set; }
    }
}
