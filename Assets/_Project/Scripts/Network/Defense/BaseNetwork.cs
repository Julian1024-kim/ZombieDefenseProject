// 거점의 네트워크 어댑터.
// 체력은 호스트가 [Networked] 값으로 갖고, 회복·재시작은 BaseSystem이 맡는다.
// 거점은 NavMeshObstacle로 길을 깎아(호스트만) 좀비가 뚫고 지나가지 않게 하고, 각 피어는 체력을 HUD 막대에 옮긴다.

using Fusion;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Defense;
using ZD.Gameplay.Hud;

namespace ZD.Network.Defense
{
    public sealed class BaseNetwork : NetworkBehaviour, IBaseModel
    {
        [SerializeField] private BaseDefinition _definition;

        private NavMeshObstacle _obstacle;
        private BaseRosterModel _roster;
        private BaseHudModel _hud;

        [Networked] public int Health { get; set; }

        public int EntityId => (int)Object.Id.Raw;
        public DamageTargetKind Kind => DamageTargetKind.Base;
        public int MaxHealth => _definition.MaxHealth;
        public BaseDefinition Definition => _definition;
        public Vector3 Position => transform.position;

        private void Awake()
        {
            _obstacle = GetComponent<NavMeshObstacle>();
            if (_obstacle != null)
            {
                _obstacle.enabled = false;
            }
        }

        [Inject]
        public void Construct(BaseRosterModel roster, BaseHudModel hud)
        {
            _roster = roster;
            _hud = hud;
        }

        public override void Spawned()
        {
            _roster.Register(this);
            _hud.MaxHealth.Value = MaxHealth;
            _hud.IsVisible.Value = true;
            if (HasStateAuthority)
            {
                Health = MaxHealth;
                if (_obstacle != null)
                {
                    _obstacle.enabled = true;
                }
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _roster.Unregister(this);
            _hud.IsVisible.Value = false;
        }

        public override void Render()
        {
            _hud.Health.Value = Health;
        }
    }
}
