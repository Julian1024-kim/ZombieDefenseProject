// Bootstrap 씬(앱 전체)의 의존성을 연결한다: 고른 모드와 접속 요청, 프로필(닉네임)과 설정 저장소, 방 목록(로비), 시작 화면, 씬 전환, 최고 점수 저장소.
// Game 씬은 이 스코프의 자식으로 만들어져 고른 모드를 받는다.
// 세션 서비스는 네트워크 오브젝트 주입 때문에 Game 씬에 있다.

using VContainer;
using VContainer.Unity;
using ZD.Core;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Lobby;
using ZD.Gameplay.Stats;
using ZD.Gameplay.Waves;
using ZD.Network.Session;
using ZD.Presentation.Menu;

namespace ZD.Bootstrap
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ModeSelectionModel>(Lifetime.Singleton);
            builder.Register<SessionRequestModel>(Lifetime.Singleton);
            builder.Register<PlayerProfileModel>(Lifetime.Singleton);
            builder.Register<LobbyModel>(Lifetime.Singleton);
            builder.Register<MenuModel>(Lifetime.Singleton);
            builder.Register<MenuSystem>(Lifetime.Singleton);
            builder.Register<ProfileSystem>(Lifetime.Singleton);
            builder.Register<SystemRandomSource>(Lifetime.Singleton).As<IRandomSource>();
            builder.Register<FusionLobbyService>(Lifetime.Singleton).As<ILobbyService>();
            builder.Register<FileSettingsRepository>(Lifetime.Singleton).As<ISettingsRepository>();
            builder.Register<UnitySceneNavigator>(Lifetime.Singleton).As<ISceneNavigator>();
            builder.Register<FileHighScoreRepository>(Lifetime.Singleton).As<IHighScoreRepository>();

            builder.RegisterComponentInHierarchy<ModeSelectView>();
            builder.RegisterComponentInHierarchy<LobbyView>();
            builder.RegisterComponentInHierarchy<CreateRoomView>();
            builder.RegisterComponentInHierarchy<PasswordPromptView>();
            builder.RegisterEntryPoint<CommandLineStartEntryPoint>();
        }
    }
}
