// LocalInputSystem의 입력 유지, 슬롯 순번, 시선 계산 테스트.

using NUnit.Framework;
using UnityEngine;
using ZD.Gameplay.Players;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class LocalInputSystemTests
    {
        private const int HOLD_TICKS = 4;

        private LocalInputModel _model;
        private LocalInputSystem _sut;

        [SetUp]
        public void SetUp()
        {
            _model = new LocalInputModel();
            PlayerDefinition definition = TestAssets.Create<PlayerDefinition>(so => so.FindProperty("_lookSensitivity").floatValue = 1f);
            _sut = new LocalInputSystem(_model, definition);
        }

        [Test]
        public void IsFireDown_ClickReleasedBeforeTheTick_StaysDownForTheHoldTicksThenReleases()
        {
            _sut.SetFireHeld(true);
            _sut.SetFireHeld(false);

            for (int tickIndex = 0; tickIndex < HOLD_TICKS; tickIndex++)
            {
                Assert.That(_sut.IsFireDown(), Is.True, $"tick {tickIndex}");
                _sut.AdvanceTick();
            }

            Assert.That(_sut.IsFireDown(), Is.False);
        }

        [Test]
        public void IsFireDown_WhileHeld_StaysDownPastTheHoldTicks()
        {
            _sut.SetFireHeld(true);
            for (int tickIndex = 0; tickIndex < HOLD_TICKS * 3; tickIndex++)
            {
                _sut.AdvanceTick();
            }

            Assert.That(_sut.IsFireDown(), Is.True);
        }

        [Test]
        public void OneShotRequests_SurviveSeveralTicksSoALateTickCannotLoseThem()
        {
            _sut.RequestReload();
            _sut.RequestRestart();

            for (int tickIndex = 0; tickIndex < HOLD_TICKS - 1; tickIndex++)
            {
                _sut.AdvanceTick();
            }

            Assert.That(_sut.IsReloadDown(), Is.True);
            Assert.That(_sut.IsRestartDown(), Is.True);

            _sut.AdvanceTick();

            Assert.That(_sut.IsReloadDown(), Is.False);
            Assert.That(_sut.IsRestartDown(), Is.False);
        }

        [Test]
        public void RequestSlot_EveryPressBumpsTheSequenceEvenForTheSameKey()
        {
            byte start = _sut.SlotRequestSequence;

            _sut.RequestSlot(WeaponSlot.Primary);
            _sut.AdvanceTick();
            _sut.RequestSlot(WeaponSlot.Primary);

            Assert.That(_sut.RequestedSlot, Is.EqualTo(WeaponSlot.Primary));
            Assert.That(_sut.SlotRequestSequence, Is.EqualTo((byte)(start + 2)));
        }

        [Test]
        public void AddLookInput_WrapsYawAndClampsPitch()
        {
            _sut.ResetLook(350f, 0f);

            _sut.AddLookInput(new Vector2(20f, -200f));

            Assert.That(_model.Yaw, Is.EqualTo(10f).Within(0.001f));
            Assert.That(_model.Pitch, Is.EqualTo(70f).Within(0.001f));
        }
    }
}
