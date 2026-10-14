// 어깨 시점 카메라의 위치 오프셋과 벽 충돌 설정.

using UnityEngine;

namespace ZD.Gameplay.Cameras
{
    [CreateAssetMenu(menuName = "ZD/Camera Settings")]
    public sealed class CameraSettings : ScriptableObject
    {
        [SerializeField] private Vector3 _pivotOffset = new(0f, 1.65f, 0f);
        [SerializeField] private Vector3 _shoulderOffset = new(0.75f, 0.15f, -3.4f);
        [SerializeField] private float _collisionRadius = 0.2f;
        [SerializeField] private LayerMask _collisionMask = 1;

        public Vector3 PivotOffset => _pivotOffset;
        public Vector3 ShoulderOffset => _shoulderOffset;
        public float CollisionRadius => _collisionRadius;
        public LayerMask CollisionMask => _collisionMask;
    }
}
