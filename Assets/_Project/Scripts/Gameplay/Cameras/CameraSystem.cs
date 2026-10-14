// 어깨 시점 카메라.
// 로컬 시선 각도를 바로 써서 조준에 네트워크 지연이 없고, 기준점과 어깨 위치 사이에 벽이 있으면 카메라를 당겨온다.

using UnityEngine;
using VContainer.Unity;
using ZD.Gameplay.Players;

namespace ZD.Gameplay.Cameras
{
    public sealed class CameraSystem : ILateTickable
    {
        private readonly CameraModel _model;
        private readonly LocalInputModel _input;
        private readonly CameraSettings _settings;

        public CameraSystem(CameraModel model, LocalInputModel input, CameraSettings settings)
        {
            _model = model;
            _input = input;
            _settings = settings;
        }

        public void LateTick()
        {
            if (!_model.HasTarget)
            {
                return;
            }

            Pose desired = ThirdPersonAim.GetCameraPose(_settings, _model.TargetPosition, _input.Pitch, _input.Yaw);
            Vector3 pivot = _model.TargetPosition + _settings.PivotOffset;
            Vector3 offset = desired.position - pivot;
            float distance = offset.magnitude;
            Vector3 direction = offset / Mathf.Max(distance, 0.0001f);

            if (Physics.SphereCast(pivot, _settings.CollisionRadius, direction, out RaycastHit hit, distance, _settings.CollisionMask, QueryTriggerInteraction.Ignore))
            {
                distance = hit.distance;
            }

            _model.Pose.Value = new Pose(pivot + direction * distance, desired.rotation);
        }
    }
}
