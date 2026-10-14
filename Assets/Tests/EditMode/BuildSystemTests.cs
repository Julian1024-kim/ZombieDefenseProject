// 호스트 설치 처리 테스트: 다시 검증하고, Point를 내고, 설치 중 방어막을 만든다.

using NUnit.Framework;
using UnityEngine;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Building;
using ZD.Gameplay.Economy;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class BuildSystemTests
    {
        private FakeClock _clock;
        private FakePlacementWorld _world;
        private BarricadeRosterModel _roster;
        private FakeBarricadeSpawner _spawner;
        private MatchStateModel _match;
        private BuildSystem _sut;
        private FakePlayer _builder;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
            _world = new FakePlacementWorld();
            _roster = new BarricadeRosterModel();
            _spawner = new FakeBarricadeSpawner(_roster);
            BuildCatalog catalog = TestAssets.Build(TestAssets.Barricade(price: 300, health: 300));
            _match = new MatchStateModel();
            _sut = new BuildSystem(_clock, catalog, new PlacementSystem(catalog, _world), new WalletSystem(), _spawner, _roster, _match);
            _builder = new FakePlayer(1, position: Vector3.zero);
            _builder.FakeWallet.Points = 500;
        }

        [Test]
        public void TryBuild_GameOver_RejectedWithoutSpending()
        {
            _match.Flow = new FakeGameFlow { Phase = GamePhase.GameOver };

            PlacementResult result = _sut.TryBuild(_builder, 0, new Vector3(0f, 0f, 2f), 0f);

            Assert.That(result, Is.EqualTo(PlacementResult.Unavailable));
            Assert.That(_builder.FakeWallet.Points, Is.EqualTo(500));
            Assert.That(_spawner.Spawned, Is.Empty);
        }

        [Test]
        public void TryBuild_Valid_SpendsPointsAndSpawnsInstalling()
        {
            PlacementResult result = _sut.TryBuild(_builder, 0, new Vector3(0f, 0f, 2f), 90f);

            Assert.That(result, Is.EqualTo(PlacementResult.Valid));
            Assert.That(_builder.FakeWallet.Points, Is.EqualTo(200));
            FakeBarricade barricade = _spawner.Spawned[0];
            Assert.That(barricade.State, Is.EqualTo(BarricadeStateId.Installing));
            Assert.That(barricade.Health, Is.EqualTo(300));
            Assert.That(barricade.BuilderId, Is.EqualTo(1));
            Assert.That(barricade.InstallEndTick, Is.EqualTo(_clock.Tick + FakeClock.TICK_RATE * 3));
            Assert.That(barricade.Rotation.eulerAngles.y, Is.EqualTo(90f).Within(0.01f));
        }

        [Test]
        public void TryBuild_AreaTakenMeanwhile_RejectedWithoutSpending()
        {
            _world.IsBlocked = true;

            PlacementResult result = _sut.TryBuild(_builder, 0, new Vector3(0f, 0f, 2f), 0f);

            Assert.That(result, Is.EqualTo(PlacementResult.Blocked));
            Assert.That(_builder.FakeWallet.Points, Is.EqualTo(500));
            Assert.That(_spawner.Spawned, Is.Empty);
        }

        [Test]
        public void TryBuild_AlreadyInstalling_Unavailable()
        {
            _sut.TryBuild(_builder, 0, new Vector3(0f, 0f, 2f), 0f);

            PlacementResult second = _sut.TryBuild(_builder, 0, new Vector3(2f, 0f, 0f), 0f);

            Assert.That(second, Is.EqualTo(PlacementResult.Unavailable));
            Assert.That(_builder.FakeWallet.Points, Is.EqualTo(200));
            Assert.That(_spawner.Spawned.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryBuild_DownedBuilderOrUnknownItem_Unavailable()
        {
            Assert.That(_sut.TryBuild(_builder, 5, new Vector3(0f, 0f, 2f), 0f), Is.EqualTo(PlacementResult.Unavailable));

            _builder.Health = 0;
            Assert.That(_sut.TryBuild(_builder, 0, new Vector3(0f, 0f, 2f), 0f), Is.EqualTo(PlacementResult.Unavailable));
            Assert.That(_builder.FakeWallet.Points, Is.EqualTo(500));
        }
    }
}
