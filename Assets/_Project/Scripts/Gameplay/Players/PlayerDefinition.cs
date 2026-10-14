// 플레이어 기본 수치(체력, 이동 속도, 마우스 감도, 총구 위치)와 부활 설정.

using UnityEngine;

namespace ZD.Gameplay.Players
{
    [CreateAssetMenu(menuName = "ZD/Player Definition")]
    public sealed class PlayerDefinition : ScriptableObject
    {
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _lookSensitivity = 0.12f;
        [Tooltip("Muzzle position relative to the player's feet, in the player's facing space.")]
        [SerializeField] private Vector3 _muzzleOffset = new(0.3f, 1.35f, 0.5f);

        [Header("Revive")]
        [Tooltip("쓰러진 동료와 이 거리(m) 안에 있어야 부활시킬 수 있다.")]
        [SerializeField] private float _reviveRange = 2f;
        [Tooltip("F를 누르고 있어야 하는 시간(초).")]
        [SerializeField] private float _reviveSeconds = 5f;
        [Tooltip("부활한 플레이어의 체력.")]
        [SerializeField] private int _reviveHealth = 30;

        public int MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float LookSensitivity => _lookSensitivity;
        public Vector3 MuzzleOffset => _muzzleOffset;
        public float ReviveRange => _reviveRange;
        public float ReviveSeconds => _reviveSeconds;
        public int ReviveHealth => _reviveHealth;
    }
}
