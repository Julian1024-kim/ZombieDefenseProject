// 방어막 하나의 네트워크 어댑터.
// 상태는 모두 호스트가 [Networked] 값으로 갖고, 상태 머신은 BarricadeSystem이 돌린다.
// 완성되면 벽은 NavMeshObstacle로 길을 막는다.
// 끝나면(부서짐, 취소) 잠시 뒤 호스트가 제거해서 클라이언트도 마지막 모습을 본다.
// 설치 중이면 설치한 플레이어의 화면에 진행도를 보여준다.

using Fusion;
using R3;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using ZD.Gameplay.Building;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Hud;

namespace ZD.Network.Building
{
    public sealed class BarricadeNetwork : NetworkBehaviour, IBarricadeModel, IBarricadeDisplay
    {
        private const float REMOVE_DELAY_SECONDS = 0.8f;
        private const float NO_PROGRESS = -1f;

        [SerializeField] private BarricadeDefinition _definition;

        private readonly ReactiveProperty<BarricadeStateId> _displayState = new(BarricadeStateId.Installing);
        private readonly ReactiveProperty<int> _displayHealth = new(0);
        private NavMeshObstacle _obstacle;
        private Collider _collider;
        private BarricadeRosterModel _roster;
        private BuildCatalog _catalog;
        private LocalPlayerHudModel _hud;
        private bool _isShowingProgress;

        [Networked] public int Health { get; set; }
        [Networked] public BarricadeStateId State { get; set; }
        [Networked] public int InstallEndTick { get; set; }
        [Networked] public int BuilderId { get; set; }
        [Networked] public Vector3 BuilderStartPosition { get; set; }
        [Networked] public int BuilderStartHealth { get; set; }
        [Networked] public int UsesLeft { get; set; }
        [Networked] public int BusyUntilTick { get; set; }
        [Networked] private TickTimer RemoveTimer { get; set; }

        public int EntityId => (int)Object.Id.Raw;
        public DamageTargetKind Kind => DamageTargetKind.Barricade;
        public int MaxHealth => _definition.MaxHealth;
        public BarricadeDefinition Definition => _definition;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
        public ReadOnlyReactiveProperty<BarricadeStateId> DisplayState => _displayState;
        public ReadOnlyReactiveProperty<int> DisplayHealth => _displayHealth;

        private void Awake()
        {
            _obstacle = GetComponent<NavMeshObstacle>();
            _collider = GetComponent<Collider>();
            if (_obstacle != null)
            {
                _obstacle.enabled = false;
            }
        }

        [Inject]
        public void Construct(BarricadeRosterModel roster, BuildCatalog catalog, LocalPlayerHudModel hud)
        {
            _roster = roster;
            _catalog = catalog;
            _hud = hud;
        }

        public override void Spawned()
        {
            _roster.Register(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _roster.Unregister(this);
            HideProgress();
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || (State != BarricadeStateId.Destroyed && State != BarricadeStateId.Cancelled))
            {
                return;
            }

            if (!RemoveTimer.IsRunning)
            {
                RemoveTimer = TickTimer.CreateFromSeconds(Runner, REMOVE_DELAY_SECONDS);
            }
            else if (RemoveTimer.Expired(Runner))
            {
                Runner.Despawn(Object);
            }
        }

        public override void Render()
        {
            BarricadeStateId state = State;
            _displayState.Value = state;
            _displayHealth.Value = Health;

            bool isStanding = state == BarricadeStateId.Installing || state == BarricadeStateId.Active;
            if (_collider != null && _collider.enabled != isStanding)
            {
                _collider.enabled = isStanding;
            }

            // 좀비 길찾기는 호스트만 하므로, 비싼 길 깎기는 호스트에서만 켠다.
            bool blocksPath = state == BarricadeStateId.Active && HasStateAuthority;
            if (_obstacle != null && _obstacle.enabled != blocksPath)
            {
                _obstacle.enabled = blocksPath;
            }

            RefreshProgress(state);
        }

        private void OnDestroy()
        {
            _displayState.Dispose();
            _displayHealth.Dispose();
        }

        private void RefreshProgress(BarricadeStateId state)
        {
            if (state != BarricadeStateId.Installing || BuilderId != Runner.LocalPlayer.AsIndex)
            {
                HideProgress();
                return;
            }

            float totalTicks = Mathf.Max(1f, _catalog.InstallSeconds / Runner.DeltaTime);
            float remainingTicks = InstallEndTick - Runner.Tick.Raw;
            _hud.BuildProgress.Value = Mathf.Clamp01(1f - remainingTicks / totalTicks);
            _isShowingProgress = true;
        }

        private void HideProgress()
        {
            if (_isShowingProgress)
            {
                _hud.BuildProgress.Value = NO_PROGRESS;
                _isShowingProgress = false;
            }
        }
    }
}
