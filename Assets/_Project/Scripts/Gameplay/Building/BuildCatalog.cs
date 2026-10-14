// 설치할 수 있는 방어막 목록(숫자키 순서)과 설치 규칙 수치: 설치 거리, 최대 경사, 설치 시간, 취소되는 이동 거리, 판정용 레이어.

using UnityEngine;

namespace ZD.Gameplay.Building
{
    [CreateAssetMenu(menuName = "ZD/Build Catalog")]
    public sealed class BuildCatalog : ScriptableObject
    {
        [SerializeField] private BarricadeDefinition[] _items;
        [SerializeField] private float _maxPlaceDistance = 4f;
        [SerializeField] private float _maxSlopeDegrees = 20f;
        [SerializeField] private float _installSeconds = 3f;
        [SerializeField] private float _cancelMoveDistance = 0.3f;
        [SerializeField] private LayerMask _groundMask = 1;
        [SerializeField] private LayerMask _blockingMask = 1;

        public int Count => _items.Length;
        public float MaxPlaceDistance => _maxPlaceDistance;
        public float MaxSlopeDegrees => _maxSlopeDegrees;
        public float InstallSeconds => _installSeconds;
        public float CancelMoveDistance => _cancelMoveDistance;
        public LayerMask GroundMask => _groundMask;
        public LayerMask BlockingMask => _blockingMask;

        public BarricadeDefinition Get(int index)
        {
            return index >= 0 && index < _items.Length ? _items[index] : null;
        }
    }
}
