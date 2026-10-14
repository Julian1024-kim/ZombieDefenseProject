// 거점 설정: 체력, 웨이브 클리어 때 회복 비율, 크기, 좀비가 거점 대신 플레이어를 노리는 거리.

using UnityEngine;

namespace ZD.Gameplay.Defense
{
    [CreateAssetMenu(menuName = "ZD/Base Definition")]
    public sealed class BaseDefinition : ScriptableObject
    {
        [SerializeField] private int _maxHealth = 3000;
        [SerializeField, Range(0f, 1f)] private float _waveClearHealFraction = 0.2f;
        [SerializeField] private Vector3 _size = new(3f, 2f, 3f);
        [Tooltip("이 거리 안에 서 있는 플레이어가 있으면 좀비가 거점 대신 플레이어를 공격한다.")]
        [SerializeField] private float _playerAggroRadius = 6f;

        public int MaxHealth => _maxHealth;
        public float WaveClearHealFraction => _waveClearHealFraction;
        public Vector3 Size => _size;
        public float PlayerAggroRadius => _playerAggroRadius;
    }
}
