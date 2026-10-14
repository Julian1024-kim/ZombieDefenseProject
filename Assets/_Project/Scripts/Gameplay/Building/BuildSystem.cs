// 호스트에서 설치 요청을 처리한다: 배치 규칙으로 다시 검증하고, Point를 차감하고, 설치 중 상태의 방어막을 스폰한다.
// 미리보기가 초록이어도 그사이 다른 사람이 자리를 차지했으면 여기서 거절된다.
// 한 사람은 한 번에 하나만 설치한다.

using UnityEngine;
using ZD.Core;
using ZD.Gameplay.Economy;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Players;

namespace ZD.Gameplay.Building
{
    public sealed class BuildSystem
    {
        private readonly INetworkClock _clock;
        private readonly BuildCatalog _catalog;
        private readonly PlacementSystem _placement;
        private readonly WalletSystem _walletSystem;
        private readonly IBarricadeSpawner _spawner;
        private readonly BarricadeRosterModel _barricades;
        private readonly MatchStateModel _match;

        public BuildSystem(INetworkClock clock, BuildCatalog catalog, PlacementSystem placement, WalletSystem walletSystem, IBarricadeSpawner spawner, BarricadeRosterModel barricades, MatchStateModel match)
        {
            _match = match;
            _clock = clock;
            _catalog = catalog;
            _placement = placement;
            _walletSystem = walletSystem;
            _spawner = spawner;
            _barricades = barricades;
        }

        public PlacementResult TryBuild(IPlayerModel builder, int itemIndex, Vector3 target, float yaw)
        {
            if (_match.IsGameOver || builder.Health <= 0 || _barricades.IsInstalling(builder.EntityId))
            {
                return PlacementResult.Unavailable;
            }

            BarricadeDefinition definition = _catalog.Get(itemIndex);
            PlacementResult result = _placement.Evaluate(builder.Position, target, yaw, definition, builder.Wallet.Points, out Vector3 position, out Quaternion rotation);
            if (result != PlacementResult.Valid)
            {
                return result;
            }

            if (!_walletSystem.TrySpend(builder.Wallet, definition.Price))
            {
                return PlacementResult.NotEnoughPoints;
            }

            IBarricadeModel barricade = _spawner.Spawn(definition, position, rotation);
            if (barricade == null)
            {
                _walletSystem.Refund(builder.Wallet, definition.Price);
                return PlacementResult.Unavailable;
            }

            barricade.Health = definition.MaxHealth;
            barricade.State = BarricadeStateId.Installing;
            barricade.InstallEndTick = _clock.Tick + Mathf.Max(1, _clock.SecondsToTicks(_catalog.InstallSeconds));
            barricade.BuilderId = builder.EntityId;
            barricade.BuilderStartPosition = builder.Position;
            barricade.BuilderStartHealth = builder.Health;
            barricade.UsesLeft = definition.Uses;
            barricade.BusyUntilTick = 0;
            return PlacementResult.Valid;
        }
    }
}
