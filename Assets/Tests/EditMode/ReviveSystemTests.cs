// ReviveSystem의 부활 시작, 완료, 취소 조건 테스트.

using NUnit.Framework;
using UnityEngine;
using ZD.Core.Messages;
using ZD.Gameplay.Players;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class ReviveSystemTests
    {
        private const int REVIVE_TICKS = FakeClock.TICK_RATE * 5;

        private FakeClock _clock;
        private PlayerRosterModel _players;
        private CapturingPublisher<PlayerRevivedMessage> _revived;
        private ReviveSystem _sut;
        private FakePlayer _reviver;
        private FakePlayer _downed;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
            _players = new PlayerRosterModel();
            _revived = new CapturingPublisher<PlayerRevivedMessage>();
            PlayerDefinition definition = TestAssets.Create<PlayerDefinition>(so =>
            {
                so.FindProperty("_reviveRange").floatValue = 2f;
                so.FindProperty("_reviveSeconds").floatValue = 5f;
                so.FindProperty("_reviveHealth").intValue = 30;
            });
            _sut = new ReviveSystem(_clock, _players, definition, _revived);

            _reviver = new FakePlayer(1, position: Vector3.zero);
            _downed = new FakePlayer(2, health: 0, position: new Vector3(1f, 0f, 0f));
            _players.Register(_reviver);
            _players.Register(_downed);
        }

        [Test]
        public void Tick_HeldForFiveSeconds_RevivesWithThirtyHealth()
        {
            _sut.Tick(_reviver, true, true);
            Assert.That(_reviver.ReviveTargetId, Is.EqualTo(2));

            HoldFor(REVIVE_TICKS - 1);
            Assert.That(_downed.Health, Is.EqualTo(0));

            HoldFor(1);
            Assert.That(_downed.Health, Is.EqualTo(30));
            Assert.That(_reviver.ReviveTargetId, Is.EqualTo(ReviveSystem.NO_TARGET));
            Assert.That(_revived.Messages.Count, Is.EqualTo(1));
            Assert.That(_revived.Messages[0].ReviverId, Is.EqualTo(1));
        }

        [Test]
        public void Tick_ReleasedEarly_CancelsAndStartsOverNextTime()
        {
            _sut.Tick(_reviver, true, true);
            HoldFor(REVIVE_TICKS / 2);

            _sut.Tick(_reviver, false, true);
            Assert.That(_reviver.ReviveTargetId, Is.EqualTo(ReviveSystem.NO_TARGET));

            _sut.Tick(_reviver, true, true);
            HoldFor(REVIVE_TICKS - 1);
            Assert.That(_downed.Health, Is.EqualTo(0));
        }

        [Test]
        public void Tick_TargetOutOfRange_Cancels()
        {
            _sut.Tick(_reviver, true, true);

            _downed.Position = new Vector3(5f, 0f, 0f);
            _sut.Tick(_reviver, true, true);

            Assert.That(_reviver.ReviveTargetId, Is.EqualTo(ReviveSystem.NO_TARGET));
        }

        [Test]
        public void Tick_ReviverGoesDown_Cancels()
        {
            _sut.Tick(_reviver, true, true);

            _reviver.Health = 0;
            HoldFor(REVIVE_TICKS);

            Assert.That(_downed.Health, Is.EqualTo(0));
            Assert.That(_reviver.ReviveTargetId, Is.EqualTo(ReviveSystem.NO_TARGET));
        }

        [Test]
        public void Tick_WithoutAuthority_DoesNotChangeHealth()
        {
            _sut.Tick(_reviver, true, false);
            for (int tickIndex = 0; tickIndex < REVIVE_TICKS; tickIndex++)
            {
                _clock.Advance(1);
                _sut.Tick(_reviver, true, false);
            }

            Assert.That(_downed.Health, Is.EqualTo(0));
            Assert.That(_revived.Messages, Is.Empty);
        }

        [Test]
        public void Tick_NobodyDownedNearby_DoesNothing()
        {
            _downed.Position = new Vector3(10f, 0f, 0f);

            _sut.Tick(_reviver, true, true);

            Assert.That(_reviver.ReviveTargetId, Is.EqualTo(ReviveSystem.NO_TARGET));
        }

        [Test]
        public void Progress_RisesAndTheDownedPlayerCanFindTheReviver()
        {
            Assert.That(_sut.GetProgress(_reviver.Revive), Is.EqualTo(-1f));

            _sut.Tick(_reviver, true, true);
            HoldFor(REVIVE_TICKS / 2);

            Assert.That(_sut.GetProgress(_reviver.Revive), Is.EqualTo(0.5f).Within(0.02f));
            Assert.That(_sut.FindReviverOf(_downed), Is.SameAs(_reviver));
        }

        private void HoldFor(int ticks)
        {
            for (int tickIndex = 0; tickIndex < ticks; tickIndex++)
            {
                _clock.Advance(1);
                _sut.Tick(_reviver, true, true);
            }
        }
    }
}
