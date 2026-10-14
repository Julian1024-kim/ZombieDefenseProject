// 배치 규칙 묶음(Point → 설치 거리 → 경사 → 겹침)과 설치 목표 고르기 테스트.

using NUnit.Framework;
using UnityEngine;
using ZD.Gameplay.Building;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class PlacementSystemTests
    {
        private BarricadeDefinition _barricade;
        private FakePlacementWorld _world;
        private PlacementSystem _sut;

        [SetUp]
        public void SetUp()
        {
            _barricade = TestAssets.Barricade(price: 300);
            _world = new FakePlacementWorld();
            _sut = new PlacementSystem(TestAssets.Build(_barricade), _world);
        }

        [Test]
        public void Evaluate_AllRulesPass_IsValidAndSnapsToGround()
        {
            PlacementResult result = Evaluate(new Vector3(0f, 0.4f, 2f), points: 300, out Vector3 position);

            Assert.That(result, Is.EqualTo(PlacementResult.Valid));
            Assert.That(position.y, Is.EqualTo(0f));
        }

        [Test]
        public void Evaluate_NotEnoughPoints_Rejected()
        {
            Assert.That(Evaluate(new Vector3(0f, 0f, 2f), points: 299, out _), Is.EqualTo(PlacementResult.NotEnoughPoints));
        }

        [Test]
        public void Evaluate_BeyondMaxDistance_TooFar()
        {
            Assert.That(Evaluate(new Vector3(0f, 0f, 4.5f), points: 300, out _), Is.EqualTo(PlacementResult.TooFar));
        }

        [Test]
        public void Evaluate_SteepGround_TooSteep()
        {
            _world.GroundNormal = Quaternion.Euler(30f, 0f, 0f) * Vector3.up;

            Assert.That(Evaluate(new Vector3(0f, 0f, 2f), points: 300, out _), Is.EqualTo(PlacementResult.TooSteep));
        }

        [Test]
        public void Evaluate_AreaOccupied_Blocked()
        {
            _world.IsBlocked = true;

            Assert.That(Evaluate(new Vector3(0f, 0f, 2f), points: 300, out _), Is.EqualTo(PlacementResult.Blocked));
        }

        [Test]
        public void Evaluate_NoGround_Rejected()
        {
            _world.HasGround = false;

            Assert.That(Evaluate(new Vector3(0f, 0f, 2f), points: 300, out _), Is.EqualTo(PlacementResult.NoGround));
        }

        [Test]
        public void Evaluate_SeveralFailures_ReportsFirstRule()
        {
            _world.IsBlocked = true;

            Assert.That(Evaluate(new Vector3(0f, 0f, 9f), points: 0, out _), Is.EqualTo(PlacementResult.NotEnoughPoints));
        }

        [Test]
        public void ChooseTarget_ClampsToMaxDistance()
        {
            Vector3 builder = Vector3.zero;
            Vector3 far = _sut.ChooseTarget(builder, new Vector3(0f, 2f, -3f), new Vector3(0f, -0.1f, 1f).normalized);
            Vector3 up = _sut.ChooseTarget(builder, new Vector3(0f, 2f, -3f), new Vector3(0f, 0.3f, 1f).normalized);

            Assert.That(new Vector2(far.x, far.z).magnitude, Is.EqualTo(4f).Within(0.001f));
            Assert.That(up.z, Is.EqualTo(4f).Within(0.001f));
        }

        private PlacementResult Evaluate(Vector3 target, int points, out Vector3 position)
        {
            return _sut.Evaluate(Vector3.zero, target, 0f, _barricade, points, out position, out _);
        }
    }
}
