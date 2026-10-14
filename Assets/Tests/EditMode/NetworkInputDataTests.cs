// NetworkInputData의 크기 예산과 패킹 정확도 테스트.

using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEngine;
using ZD.Network.Input;

namespace ZD.Tests.EditMode
{
    public sealed class NetworkInputDataTests
    {
        private const int INPUT_BUDGET_BYTES = 16;
        private const float ANGLE_TOLERANCE = 0.01f;
        private const float MOVE_TOLERANCE = 1f / 127f;

        [Test]
        public void Size_StaysWithinTheInputBudget()
        {
            Assert.That(Marshal.SizeOf(typeof(NetworkInputData)), Is.LessThanOrEqualTo(INPUT_BUDGET_BYTES));
        }

        [TestCase(0f, 0f)]
        [TestCase(359.99f, -70f)]
        [TestCase(123.45f, 70f)]
        public void SetLook_RoundTripsToHundredthsOfADegree(float yaw, float pitch)
        {
            var data = new NetworkInputData();

            data.SetLook(yaw, pitch);

            Assert.That(Mathf.DeltaAngle(data.YawDegrees, yaw), Is.EqualTo(0f).Within(ANGLE_TOLERANCE));
            Assert.That(data.PitchDegrees, Is.EqualTo(pitch).Within(ANGLE_TOLERANCE));
        }

        [Test]
        public void SetMoveAndSlot_DoNotOverwriteEachOther()
        {
            var data = new NetworkInputData();

            data.RequestedSlot = 3;
            data.SetMove(new Vector2(-1f, 0.5f));

            Assert.That(data.RequestedSlot, Is.EqualTo(3));
            Assert.That(data.Move.x, Is.EqualTo(-1f).Within(MOVE_TOLERANCE));
            Assert.That(data.Move.y, Is.EqualTo(0.5f).Within(MOVE_TOLERANCE));

            data.RequestedSlot = 0;
            Assert.That(data.Move.x, Is.EqualTo(-1f).Within(MOVE_TOLERANCE));
        }

        [Test]
        public void SlotSequence_SurvivesMoveAndSlotWrites()
        {
            var data = new NetworkInputData();

            data.SlotSequence = 255;
            data.RequestedSlot = 1;
            data.SetMove(new Vector2(1f, -1f));

            Assert.That(data.SlotSequence, Is.EqualTo(255));
            Assert.That(data.RequestedSlot, Is.EqualTo(1));
            Assert.That(data.Move.y, Is.EqualTo(-1f).Within(MOVE_TOLERANCE));
        }
    }
}
