// 좀비의 네트워크 어댑터.
// 모든 상태는 호스트가 갖고, 클라이언트는 보간(NetworkTransform)만 하며 상태를 View용 표시 속성에 옮긴다.
// 어떤 모델을 보여줄지(Variant)는 호스트가 스폰 때 정해 동기화한다.

using Fusion;
using R3;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Zombies;

namespace ZD.Network.Zombies
{
    [RequireComponent(typeof(NavMeshAgent), typeof(HitboxRoot), typeof(CapsuleCollider))]
    public sealed class ZombieNetwork : NetworkBehaviour, IZombieModel, IZombieDisplay
    {
        private const float CORPSE_SECONDS = 3f;
        private const float SPEED_SMOOTHING = 10f;

        [SerializeField] private ZombieDefinition _definition;
        [SerializeField] private int _visualVariantCount = 1;

        private readonly ReactiveProperty<ZombieStateId> _displayState = new(ZombieStateId.Chase);
        private readonly ReactiveProperty<int> _displayHealth = new(0);
        private readonly ReactiveProperty<float> _displaySpeed = new(0f);
        private readonly ReactiveProperty<int> _displayVariant = new(-1);
        private Vector3 _lastRenderPosition;
        private NavMeshAgent _agent;
        private HitboxRoot _hitboxRoot;
        private CapsuleCollider _collider;
        private ZombieRosterModel _roster;

        [Networked] public int Health { get; set; }
        [Networked] public ZombieStateId State { get; set; }
        [Networked] public int NextAttackTick { get; set; }
        [Networked] public int HeldUntilTick { get; set; }
        [Networked] private TickTimer DespawnTimer { get; set; }
        [Networked] private byte Variant { get; set; }

        public int EntityId => (int)Object.Id.Raw;
        public DamageTargetKind Kind => DamageTargetKind.Zombie;
        public int MaxHealth => _definition.MaxHealth;
        public ZombieDefinition Definition => _definition;
        public Vector3 Position => transform.position;
        public ReadOnlyReactiveProperty<ZombieStateId> DisplayState => _displayState;
        public ReadOnlyReactiveProperty<int> DisplayHealth => _displayHealth;
        public ReadOnlyReactiveProperty<float> DisplaySpeed => _displaySpeed;
        public ReadOnlyReactiveProperty<int> DisplayVariant => _displayVariant;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _hitboxRoot = GetComponent<HitboxRoot>();
            _collider = GetComponent<CapsuleCollider>();
            _agent.enabled = false;
        }

        [Inject]
        public void Construct(ZombieRosterModel roster)
        {
            _roster = roster;
        }

        public override void Spawned()
        {
            SetBodyActive(true);
            _lastRenderPosition = transform.position;
            if (!HasStateAuthority)
            {
                return;
            }

            Variant = (byte)Random.Range(0, Mathf.Max(1, _visualVariantCount));
            Health = _definition.MaxHealth;
            State = ZombieStateId.Chase;
            NextAttackTick = 0;
            HeldUntilTick = 0;
            DespawnTimer = TickTimer.None;
            _agent.enabled = true;
            _agent.Warp(transform.position);
            _roster.Register(this, new NavMeshZombieMotor(_agent, transform, Runner));
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (HasStateAuthority)
            {
                _roster.Unregister(this);
            }

            _agent.enabled = false;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || Health > 0)
            {
                return;
            }

            if (!DespawnTimer.IsRunning)
            {
                // 시체는 앞으로 3초 동안 총알을 막거나 플레이어를 가로막으면 안 된다.
                SetBodyActive(false);
                DespawnTimer = TickTimer.CreateFromSeconds(Runner, CORPSE_SECONDS);
            }
            else if (DespawnTimer.Expired(Runner))
            {
                Runner.Despawn(Object);
            }
        }

        public override void Render()
        {
            _displayState.Value = State;
            _displayHealth.Value = Health;
            _displayVariant.Value = Variant;
            UpdateDisplaySpeed();

            if (!HasStateAuthority && Health <= 0 && _collider.enabled)
            {
                SetBodyActive(false);
            }
        }

        private void OnDestroy()
        {
            _displayState.Dispose();
            _displayHealth.Dispose();
            _displaySpeed.Dispose();
            _displayVariant.Dispose();
        }

        private void UpdateDisplaySpeed()
        {
            Vector3 position = transform.position;
            Vector3 delta = position - _lastRenderPosition;
            _lastRenderPosition = position;
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            delta.y = 0f;
            float speed = delta.magnitude / deltaTime;
            _displaySpeed.Value = Mathf.Lerp(_displaySpeed.Value, speed, Mathf.Clamp01(SPEED_SMOOTHING * deltaTime));
        }

        private void SetBodyActive(bool isActive)
        {
            _hitboxRoot.HitboxRootActive = isActive;
            _collider.enabled = isActive;
        }
    }
}
