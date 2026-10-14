// 방 만들기·참가 테스트: 방 코드 규칙, 닉네임·방 이름 정리, 시작 화면 흐름(로비 열기, 잠긴 방, 꽉 찬 방, 코드 참가), 비밀번호 확인.

using NUnit.Framework;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Lobby;
using ZD.Network.Session;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class LobbyTests
    {
        private MenuModel _menu;
        private ModeSelectionModel _selection;
        private SessionRequestModel _request;
        private LobbyModel _lobby;
        private FakeLobbyService _lobbyService;
        private PlayerProfileModel _profile;
        private FakeSceneNavigator _navigator;
        private MenuSystem _sut;

        [SetUp]
        public void SetUp()
        {
            _menu = new MenuModel();
            _selection = new ModeSelectionModel();
            _request = new SessionRequestModel();
            _lobby = new LobbyModel();
            _lobbyService = new FakeLobbyService();
            _profile = new PlayerProfileModel();
            _profile.Nickname.Value = "철수";
            _navigator = new FakeSceneNavigator();
            _sut = new MenuSystem(_menu, _selection, _request, _lobby, _lobbyService, _profile, new FixedRandomSource(), _navigator);
        }

        [TearDown]
        public void TearDown() => _sut.Dispose();

        [Test]
        public void RoomCodes_GenerateAndNormalize()
        {
            string code = RoomCodes.Generate(new FixedRandomSource());

            Assert.That(code.Length, Is.EqualTo(RoomCodes.LENGTH));
            Assert.That(RoomCodes.TryNormalize(" abc234 ", out string normalized), Is.True);
            Assert.That(normalized, Is.EqualTo("ABC234"));
            Assert.That(RoomCodes.TryNormalize("ABCO10", out _), Is.False);
            Assert.That(RoomCodes.TryNormalize("ABC", out _), Is.False);
        }

        [Test]
        public void TextRules_SanitizeNickname()
        {
            Assert.That(TextRules.SanitizeNickname("  <b>영희</b>  "), Is.EqualTo("b영희/b"));
            Assert.That(TextRules.SanitizeNickname("가나다라마바사아자차카타파하"), Has.Length.EqualTo(TextRules.MAX_NICKNAME_LENGTH));
            Assert.That(TextRules.SanitizeNickname("   "), Is.EqualTo(TextRules.DEFAULT_NICKNAME));
            Assert.That(TextRules.SanitizeRoomName("", "철수"), Is.EqualTo("철수's Room"));
        }

        [Test]
        public void OpenLobby_SetsModeAndConnects()
        {
            _sut.OpenLobby(GameModeId.BaseDefense);

            Assert.That(_selection.Mode, Is.EqualTo(GameModeId.BaseDefense));
            Assert.That(_menu.Screen.Value, Is.EqualTo(MenuScreen.Lobby));
            Assert.That(_lobbyService.ConnectCount, Is.EqualTo(1));
        }

        [Test]
        public void CreateRoom_WritesHostRequestAndEntersGame()
        {
            _sut.OpenLobby(GameModeId.Survival);

            _sut.CreateRoom("", " secret ", isPrivate: true);

            Assert.That(_request.Kind, Is.EqualTo(SessionRequestKind.Host));
            Assert.That(_request.RoomCode, Has.Length.EqualTo(RoomCodes.LENGTH));
            Assert.That(_request.RoomName, Is.EqualTo("철수's Room"));
            Assert.That(_request.Password, Is.EqualTo("secret"));
            Assert.That(_request.IsPrivate, Is.True);
            Assert.That(_lobbyService.DisconnectCount, Is.EqualTo(1));
            Assert.That(_navigator.EnterCount, Is.EqualTo(1));
        }

        [Test]
        public void JoinListed_LockedRoom_AsksPasswordThenJoins()
        {
            _lobby.Rooms.Add(new RoomInfo("ABC234", "영희네 방", "영희", GameModeId.Survival, 1, 4, isLocked: true, isOpen: true));

            _sut.JoinListed("ABC234");
            Assert.That(_menu.Screen.Value, Is.EqualTo(MenuScreen.Password));
            Assert.That(_navigator.EnterCount, Is.EqualTo(0));

            _sut.SubmitPassword("1234");

            Assert.That(_request.Kind, Is.EqualTo(SessionRequestKind.Join));
            Assert.That(_request.RoomCode, Is.EqualTo("ABC234"));
            Assert.That(_request.Password, Is.EqualTo("1234"));
            Assert.That(_navigator.EnterCount, Is.EqualTo(1));
        }

        [Test]
        public void JoinListed_FullRoom_ShowsNotice()
        {
            _lobby.Rooms.Add(new RoomInfo("ABC234", "방", "영희", GameModeId.Survival, 4, 4, isLocked: false, isOpen: true));

            _sut.JoinListed("ABC234");

            Assert.That(_menu.Notice.Value, Is.EqualTo(MenuNotice.RoomFull));
            Assert.That(_navigator.EnterCount, Is.EqualTo(0));
        }

        [Test]
        public void JoinWithCode_ValidatesThenJoins()
        {
            _sut.JoinWithCode("12", "");
            Assert.That(_menu.Notice.Value, Is.EqualTo(MenuNotice.InvalidCode));
            Assert.That(_navigator.EnterCount, Is.EqualTo(0));

            _sut.JoinWithCode("xyz789", "pw");

            Assert.That(_request.Kind, Is.EqualTo(SessionRequestKind.Join));
            Assert.That(_request.RoomCode, Is.EqualTo("XYZ789"));
            Assert.That(_request.Password, Is.EqualTo("pw"));
            Assert.That(_navigator.EnterCount, Is.EqualTo(1));
        }

        [Test]
        public void SessionSecurity_AcceptsOnlyMatchingPassword()
        {
            var open = new SessionSecurityModel();
            open.SetPassword("ABC234", string.Empty);
            var locked = new SessionSecurityModel();
            locked.SetPassword("ABC234", "1234");

            Assert.That(open.Accepts(null), Is.True);
            Assert.That(locked.Accepts(SessionProperties.ToToken("ABC234", "1234")), Is.True);
            Assert.That(locked.Accepts(SessionProperties.ToToken("ABC234", "0000")), Is.False);
            Assert.That(locked.Accepts(null), Is.False);
            // 같은 비밀번호라도 방 코드가 다르면 토큰이 다르다(다른 방에서 엿본 토큰을 재사용할 수 없음).
            Assert.That(locked.Accepts(SessionProperties.ToToken("XYZ789", "1234")), Is.False);
        }
    }
}
