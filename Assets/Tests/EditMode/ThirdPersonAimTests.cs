// ThirdPersonAim의 조준 레이 테스트.

using NUnit.Framework;
using UnityEngine;
using ZD.Gameplay.Cameras;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class ThirdPersonAimTests
    {
        private const float TOLERANCE = 0.001f;

        private CameraSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = TestAssets.Create<CameraSettings>(so =>
            {
                so.FindProperty("_pivotOffset").vector3Value = new Vector3(0f, 1.6f, 0f);
                so.FindProperty("_shoulderOffset").vector3Value = new Vector3(0.5f, 0.1f, -2.6f);
            });
        }

        [Test]
        public void GetAimRay_LiesOnTheCameraCenterLine_SoShotsLandOnTheCrosshair()
        {
            var player = new Vector3(3f, 0f, -2f);

            ThirdPersonAim.GetAimRay(_settings, player, 10f, 35f, out Vector3 origin, out Vector3 direction);
            Pose camera = ThirdPersonAim.GetCameraPose(_settings, player, 10f, 35f);

            Vector3 cameraForward = camera.rotation * Vector3.forward;
            Assert.That(Vector3.Angle(direction, cameraForward), Is.LessThan(TOLERANCE));
            Assert.That(Vector3.Cross(origin - camera.position, cameraForward).magnitude, Is.LessThan(TOLERANCE));
        }

        [Test]
        public void GetAimRay_StartsAtThePlayersEyePlane_NotBehindThePlayer()
        {
            var player = new Vector3(0f, 0f, 0f);

            ThirdPersonAim.GetAimRay(_settings, player, 0f, 0f, out Vector3 origin, out Vector3 direction);

            Vector3 eye = player + _settings.PivotOffset;
            Assert.That(Vector3.Dot(origin - eye, direction), Is.EqualTo(0f).Within(TOLERANCE));
            Assert.That(direction, Is.EqualTo(Vector3.forward));
        }
    }
}
