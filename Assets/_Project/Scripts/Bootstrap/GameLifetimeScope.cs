// Game 씬의 의존성은 전부 여기서 연결한다.
// Bootstrap 씬에서 들어오면 RootLifetimeScope를 부모로 써서 고른 모드와 씬 전환기를 받는다.
// Game 씬만 바로 플레이하면 -zd-mode나 생존 모드로 시작하고, 돌아갈 메뉴 없는 전환기를 쓴다.

using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZD.Core;
using ZD.Core.Messages;
using ZD.Gameplay.Building;
using ZD.Gameplay.Cameras;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Defense;
using ZD.Gameplay.Economy;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Hud;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Lobby;
using ZD.Gameplay.Players;
using ZD.Gameplay.Stats;
using ZD.Gameplay.Waves;
using ZD.Gameplay.Weapons;
using ZD.Gameplay.Zombies;
using ZD.Network.Building;
using ZD.Network.Combat;
using ZD.Network.Defense;
using ZD.Network.Session;
using ZD.Network.Zombies;
using ZD.Presentation.Building;
using ZD.Presentation.Cameras;
using ZD.Presentation.Hud;
using ZD.Presentation.Input;
using ZD.Presentation.Shop;
using ZD.Presentation.Weapons;

namespace ZD.Bootstrap
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerDefinition _playerDefinition;
        [SerializeField] private WaveProfile _waveProfile;
        [SerializeField] private CameraSettings _cameraSettings;
        [SerializeField] private NetworkPrefabCatalog _prefabCatalog;
        [SerializeField] private HitQuerySettings _hitQuerySettings;
        [SerializeField] private WeaponCatalog _weaponCatalog;
        [SerializeField] private InventorySettings _inventorySettings;
        [SerializeField] private ShopCatalog _shopCatalog;
        [SerializeField] private BuildCatalog _buildCatalog;

        protected override LifetimeScope FindParent() => Find<RootLifetimeScope>();

        protected override void Configure(IContainerBuilder builder)
        {
            // 부모(Root)가 없으면 고른 모드, 접속 요청(개발용 자동), 프로필(기본 닉네임), 씬 전환기, 최고 점수 저장소를 여기서 대신 만든다.
            if (Parent == null)
            {
                CommandLineMode.TryRead(out GameModeId mode);
                builder.RegisterInstance(new ModeSelectionModel { Mode = mode });
                builder.Register<SessionRequestModel>(Lifetime.Singleton);
                builder.Register<PlayerProfileModel>(Lifetime.Singleton);
                builder.Register<StandaloneSceneNavigator>(Lifetime.Singleton).As<ISceneNavigator>();
                builder.Register<FileHighScoreRepository>(Lifetime.Singleton).As<IHighScoreRepository>();
            }

            // 설정 (ScriptableObject)
            builder.RegisterInstance(_playerDefinition);
            builder.RegisterInstance(_waveProfile);
            builder.RegisterInstance(_cameraSettings);
            builder.RegisterInstance(_prefabCatalog);
            builder.RegisterInstance(_hitQuerySettings);
            builder.RegisterInstance(_weaponCatalog);
            builder.RegisterInstance(_inventorySettings);
            builder.RegisterInstance(_shopCatalog);
            builder.RegisterInstance(_buildCatalog);

            // 모델
            builder.Register<PlayerRosterModel>(Lifetime.Singleton);
            builder.Register<ZombieRosterModel>(Lifetime.Singleton);
            builder.Register<LocalInputModel>(Lifetime.Singleton);
            builder.Register<CameraModel>(Lifetime.Singleton);
            builder.Register<MatchHudModel>(Lifetime.Singleton);
            builder.Register<LocalPlayerHudModel>(Lifetime.Singleton);
            builder.Register<SessionStatusModel>(Lifetime.Singleton);
            builder.Register<ShopUiModel>(Lifetime.Singleton);
            builder.Register<BarricadeRosterModel>(Lifetime.Singleton);
            builder.Register<BuildModeModel>(Lifetime.Singleton);
            builder.Register<BaseRosterModel>(Lifetime.Singleton);
            builder.Register<BaseHudModel>(Lifetime.Singleton);
            builder.Register<ResultHudModel>(Lifetime.Singleton);
            builder.Register<MatchStateModel>(Lifetime.Singleton);

            // 게임플레이 시스템
            builder.Register<DamageSystem>(Lifetime.Singleton);
            builder.Register<WeaponSystem>(Lifetime.Singleton);
            builder.Register<PlayerMotorSystem>(Lifetime.Singleton);
            builder.Register<PlayerStateSystem>(Lifetime.Singleton);
            builder.Register<LocalInputSystem>(Lifetime.Singleton);
            builder.Register<ZombieAISystem>(Lifetime.Singleton);
            builder.Register<WaveBudgetGenerator>(Lifetime.Singleton);
            builder.Register<WaveSystem>(Lifetime.Singleton);
            builder.Register<GameFlowSystem>(Lifetime.Singleton);
            builder.Register<SystemRandomSource>(Lifetime.Singleton).As<IRandomSource>();
            builder.Register<InventorySystem>(Lifetime.Singleton);
            builder.Register<WalletSystem>(Lifetime.Singleton);
            builder.Register<ShopSystem>(Lifetime.Singleton);
            builder.Register<KillRewardSystem>(Lifetime.Singleton);
            builder.Register<ReviveSystem>(Lifetime.Singleton);
            builder.Register<PhysicsPlacementWorld>(Lifetime.Singleton).As<IPlacementWorld>();
            builder.Register<PlacementSystem>(Lifetime.Singleton);
            builder.Register<BuildSystem>(Lifetime.Singleton);
            builder.Register<BarricadeSystem>(Lifetime.Singleton);
            builder.Register<BaseSystem>(Lifetime.Singleton);
            builder.Register<MatchExitSystem>(Lifetime.Singleton);
            builder.Register<StatsSystem>(Lifetime.Singleton);
            builder.Register<MatchResultSystem>(Lifetime.Singleton);
            builder.Register<HighScoreSystem>(Lifetime.Singleton);
            builder.Register<ResultScreenSystem>(Lifetime.Singleton);

            // 생성자에서 메시지(처치, 접속 끊김, 통계)를 구독하므로, 아무도 주입받지 않아도 반드시 생성해야 한다.
            builder.RegisterBuildCallback(container => container.Resolve<KillRewardSystem>());
            builder.RegisterBuildCallback(container => container.Resolve<MatchExitSystem>());
            builder.RegisterBuildCallback(container => container.Resolve<StatsSystem>());

            // 게임 모드: 패배 조건과 좀비 목표 선정을 모드마다 GameModeSystem이 고른다.
            builder.Register<AllPlayersDownedCondition>(Lifetime.Singleton);
            builder.Register<BaseDestroyedCondition>(Lifetime.Singleton);
            builder.Register<NearestPlayerTargetSelector>(Lifetime.Singleton);
            builder.Register<BaseFirstTargetSelector>(Lifetime.Singleton);
            builder.Register<GameModeSystem>(Lifetime.Singleton).AsSelf().As<IGameModeRules>();

            // 네트워크 어댑터
            builder.Register<NetworkRunnerHolder>(Lifetime.Singleton);
            builder.Register<FusionNetworkClock>(Lifetime.Singleton).As<INetworkClock>();
            builder.Register<LagCompensatedHitQuery>(Lifetime.Singleton).As<IHitQuery>();
            builder.Register<FusionZombieSpawner>(Lifetime.Singleton).As<IZombieSpawner>();
            builder.Register<FusionBarricadeSpawner>(Lifetime.Singleton).As<IBarricadeSpawner>();
            builder.Register<FusionBaseSpawner>(Lifetime.Singleton).As<IBaseSpawner>();
            builder.Register<PlayerSpawnService>(Lifetime.Singleton);
            builder.Register<NetworkPrefabPreloader>(Lifetime.Singleton);
            builder.Register<SessionCallbacks>(Lifetime.Singleton);
            builder.Register<SessionSecurityModel>(Lifetime.Singleton);
            builder.Register<FusionSessionService>(Lifetime.Singleton).As<ISessionService>();

            // 엔트리 포인트
            builder.RegisterEntryPoint<CameraSystem>();
            builder.RegisterEntryPoint<SessionEntryPoint>();
            builder.RegisterEntryPoint<ShopUiSystem>().AsSelf();
            builder.RegisterEntryPoint<BuildModeSystem>().AsSelf();

            // 씬 컴포넌트
            builder.RegisterComponentInHierarchy<SpawnPointSet>();
            builder.RegisterComponentInHierarchy<SupplyBoxSet>().As<ISupplyBoxLocator>();
            builder.RegisterComponentInHierarchy<InputView>();
            builder.RegisterComponentInHierarchy<DevInputBot>();
            builder.RegisterComponentInHierarchy<CameraView>();
            builder.RegisterComponentInHierarchy<PlayerHudView>();
            builder.RegisterComponentInHierarchy<MatchHudView>();
            builder.RegisterComponentInHierarchy<WeaponEffectsView>();
            builder.RegisterComponentInHierarchy<SlotBarView>();
            builder.RegisterComponentInHierarchy<ReviveBarView>();
            builder.RegisterComponentInHierarchy<ShopView>();
            builder.RegisterComponentInHierarchy<BuildPreviewView>();
            builder.RegisterComponentInHierarchy<BuildHudView>();
            builder.RegisterComponentInHierarchy<BaseHudView>();
            builder.RegisterComponentInHierarchy<ResultView>();

            // MessagePipe
            MessagePipeOptions options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<PlayerDownedMessage>(options);
            builder.RegisterMessageBroker<PlayerRevivedMessage>(options);
            builder.RegisterMessageBroker<ZombieKilledMessage>(options);
            builder.RegisterMessageBroker<WaveStartedMessage>(options);
            builder.RegisterMessageBroker<WaveClearedMessage>(options);
            builder.RegisterMessageBroker<GameOverMessage>(options);
            builder.RegisterMessageBroker<MatchRestartedMessage>(options);
            builder.RegisterMessageBroker<WeaponFiredMessage>(options);
            builder.RegisterMessageBroker<BarricadeDestroyedMessage>(options);
        }
    }
}
