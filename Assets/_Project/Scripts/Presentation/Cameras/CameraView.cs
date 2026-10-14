// CameraSystem이 계산한 위치와 회전을 씬 카메라에 적용한다.

using R3;
using UnityEngine;
using VContainer;
using ZD.Gameplay.Cameras;

namespace ZD.Presentation.Cameras
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraView : MonoBehaviour
    {
        private readonly CompositeDisposable _disposables = new();
        private CameraModel _model;
        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        [Inject]
        public void Construct(CameraModel model)
        {
            _model = model;
        }

        private void Start()
        {
            _model.Pose
                .Subscribe(pose => _transform.SetPositionAndRotation(pose.position, pose.rotation))
                .AddTo(_disposables);
        }

        private void OnDestroy() => _disposables.Dispose();
    }
}
