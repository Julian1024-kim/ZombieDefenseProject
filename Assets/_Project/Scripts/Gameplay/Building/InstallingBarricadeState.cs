// 설치 중 상태.
// 설치 시간이 지나면 완성된다.
// 그사이 설치자가 움직이거나, 맞거나, 쓰러지거나, 나가면 취소하고 Point를 돌려준다.

using ZD.Gameplay.Economy;
using ZD.Gameplay.Players;

namespace ZD.Gameplay.Building
{
    public sealed class InstallingBarricadeState : IBarricadeState
    {
        private readonly PlayerRosterModel _players;
        private readonly WalletSystem _walletSystem;
        private readonly float _sqrCancelMoveDistance;

        public InstallingBarricadeState(PlayerRosterModel players, WalletSystem walletSystem, float cancelMoveDistance)
        {
            _players = players;
            _walletSystem = walletSystem;
            _sqrCancelMoveDistance = cancelMoveDistance * cancelMoveDistance;
        }

        public BarricadeStateId Tick(IBarricadeModel barricade, in BarricadeTickContext context)
        {
            IPlayerModel builder = _players.FindById(barricade.BuilderId);
            if (builder == null)
            {
                return BarricadeStateId.Cancelled;
            }

            // 설치 중 회복으로 체력이 올랐으면 기준도 올린다. 그래야 그 뒤에 맞아도 취소된다.
            if (builder.Health > barricade.BuilderStartHealth)
            {
                barricade.BuilderStartHealth = builder.Health;
            }

            bool tookDamage = builder.Health < barricade.BuilderStartHealth;
            bool moved = (builder.Position - barricade.BuilderStartPosition).sqrMagnitude > _sqrCancelMoveDistance;
            if (tookDamage || moved)
            {
                _walletSystem.Refund(builder.Wallet, barricade.Definition.Price);
                return BarricadeStateId.Cancelled;
            }

            return context.Tick >= barricade.InstallEndTick ? BarricadeStateId.Active : BarricadeStateId.Installing;
        }
    }
}
