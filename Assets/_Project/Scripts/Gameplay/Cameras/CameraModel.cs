// 로컬 전용 카메라 상태.
// 네트워크로 동기화하지 않는다.

using R3;
using UnityEngine;

namespace ZD.Gameplay.Cameras
{
    public sealed class CameraModel
    {
        public bool HasTarget { get; set; }
        public Vector3 TargetPosition { get; set; }
        public ReactiveProperty<Pose> Pose { get; } = new(UnityEngine.Pose.identity);
    }
}
