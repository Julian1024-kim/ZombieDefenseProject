// 로컬 설치 모드.
// 켜져 있으면 매 프레임 카메라 조준 방향으로 설치 위치를 고르고 배치 규칙으로 미리 판정한다.
// 확정하면 호스트에 요청하고 설치 모드를 끈다.
// 여기 판정은 미리보기용이고, 호스트가 같은 규칙으로 다시 검증한다.

using UnityEngine;
using VContainer.Unity;
using ZD.Gameplay.Cameras;
using ZD.Gameplay.Hud;

namespace ZD.Gameplay.Building
{
    public sealed class BuildModeSystem : ITickable
    {
        private readonly BuildModeModel _model;
        private readonly BuildCatalog _catalog;
        private readonly PlacementSystem _placement;
        private readonly CameraModel _cameraModel;
        private readonly LocalPlayerHudModel _localPlayer;

        public BuildModeSystem(BuildModeModel model, BuildCatalog catalog, PlacementSystem placement, CameraModel cameraModel, LocalPlayerHudModel localPlayer)
        {
            _model = model;
            _catalog = catalog;
            _placement = placement;
            _cameraModel = cameraModel;
            _localPlayer = localPlayer;
        }

        public void Tick()
        {
            if (!_model.IsActive.Value)
            {
                return;
            }

            if (!_cameraModel.HasTarget || _localPlayer.Health.Value <= 0)
            {
                Exit();
                return;
            }

            Pose camera = _cameraModel.Pose.Value;
            Vector3 builder = _cameraModel.TargetPosition;
            Vector3 aimDirection = camera.rotation * Vector3.forward;
            Vector3 target = _placement.ChooseTarget(builder, camera.position, aimDirection);
            float yaw = camera.rotation.eulerAngles.y;
            BarricadeDefinition definition = _catalog.Get(_model.SelectedIndex.Value);
            PlacementResult result = _placement.Evaluate(builder, target, yaw, definition, _localPlayer.Points.Value, out Vector3 position, out Quaternion rotation);

            // 이미 설치 중이면(진행 막대가 보이면) 호스트가 거절하므로 미리보기도 막는다.
            _model.PreviewResult.Value = _localPlayer.BuildProgress.Value >= 0f ? PlacementResult.Unavailable : result;
            _model.PreviewPose.Value = new Pose(position, rotation);
        }

        public void Toggle()
        {
            if (_model.IsActive.Value)
            {
                Exit();
            }
            else if (_localPlayer.Health.Value > 0)
            {
                _model.IsActive.Value = true;
            }
        }

        public void Select(int itemIndex)
        {
            if (_model.IsActive.Value && _catalog.Get(itemIndex) != null)
            {
                _model.SelectedIndex.Value = itemIndex;
            }
        }

        public void Confirm()
        {
            if (!_model.IsActive.Value || _model.PreviewResult.Value != PlacementResult.Valid || _model.RequestSender == null)
            {
                return;
            }

            Pose pose = _model.PreviewPose.Value;
            _model.RequestSender.SendBuildRequest(_model.SelectedIndex.Value, pose.position, pose.rotation.eulerAngles.y);
            Exit();
        }

        public void Exit()
        {
            _model.IsActive.Value = false;
        }
    }
}
