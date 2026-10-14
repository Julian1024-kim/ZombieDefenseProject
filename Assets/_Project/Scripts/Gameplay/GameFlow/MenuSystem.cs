// 시작 화면 로직.
// 모드를 고르면 그 모드의 방 목록(로비)을 열고, 방 만들기(호스트)·목록에서 참가·방 코드로 참가를 처리한다.
// 잠긴 방은 비밀번호를 물어본다.
// 들어갈 방을 SessionRequestModel에 적고 게임 씬으로 넘어가며, 그때 방 목록 서버는 끊는다.
// 게임에서 돌아오면(시작 화면이 다시 보이면) 방 목록에 다시 붙는다.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using ZD.Core;
using ZD.Gameplay.Lobby;
using ZD.Gameplay.Waves;

namespace ZD.Gameplay.GameFlow
{
    public sealed class MenuSystem : IDisposable
    {
        private readonly MenuModel _menu;
        private readonly ModeSelectionModel _selection;
        private readonly SessionRequestModel _request;
        private readonly LobbyModel _lobby;
        private readonly ILobbyService _lobbyService;
        private readonly PlayerProfileModel _profile;
        private readonly IRandomSource _random;
        private readonly ISceneNavigator _navigator;
        private readonly CancellationTokenSource _cts = new();
        private readonly IDisposable _visibilitySubscription;

        public MenuSystem(
            MenuModel menu,
            ModeSelectionModel selection,
            SessionRequestModel request,
            LobbyModel lobby,
            ILobbyService lobbyService,
            PlayerProfileModel profile,
            IRandomSource random,
            ISceneNavigator navigator)
        {
            _menu = menu;
            _selection = selection;
            _request = request;
            _lobby = lobby;
            _lobbyService = lobbyService;
            _profile = profile;
            _random = random;
            _navigator = navigator;
            _visibilitySubscription = menu.IsVisible
                .Where(isVisible => isVisible)
                .Subscribe(_ => OnMenuShown());
        }

        public void OpenLobby(GameModeId mode)
        {
            if (_menu.IsLoading.Value)
            {
                return;
            }

            _selection.Mode = mode;
            _menu.Notice.Value = MenuNotice.None;
            _menu.Screen.Value = MenuScreen.Lobby;
            ConnectLobbyAsync().Forget();
        }

        public void BackToMain()
        {
            _menu.Screen.Value = MenuScreen.Main;
            _menu.Notice.Value = MenuNotice.None;
            _lobbyService.DisconnectAsync().Forget();
        }

        public void OpenCreateRoom()
        {
            _menu.Notice.Value = MenuNotice.None;
            _menu.Screen.Value = MenuScreen.CreateRoom;
        }

        public void CloseDialog() => _menu.Screen.Value = MenuScreen.Lobby;

        public void CreateRoom(string roomName, string password, bool isPrivate)
        {
            _request.Kind = SessionRequestKind.Host;
            _request.RoomCode = RoomCodes.Generate(_random);
            _request.RoomName = TextRules.SanitizeRoomName(roomName, _profile.Nickname.Value);
            _request.Password = TextRules.SanitizePassword(password);
            _request.IsPrivate = isPrivate;
            EnterGameAsync().Forget();
        }

        public void JoinListed(string code)
        {
            if (!_lobby.TryFind(code, out RoomInfo room))
            {
                _menu.Notice.Value = MenuNotice.RoomNotFound;
                return;
            }

            if (!room.CanJoin)
            {
                _menu.Notice.Value = MenuNotice.RoomFull;
                return;
            }

            if (room.IsLocked)
            {
                _menu.PendingRoom = room;
                _menu.Notice.Value = MenuNotice.None;
                _menu.Screen.Value = MenuScreen.Password;
                return;
            }

            Join(room.Code, string.Empty);
        }

        public void SubmitPassword(string password) => Join(_menu.PendingRoom.Code, password);

        public void JoinWithCode(string codeInput, string password)
        {
            if (!RoomCodes.TryNormalize(codeInput, out string code))
            {
                _menu.Notice.Value = MenuNotice.InvalidCode;
                return;
            }

            Join(code, password);
        }

        public void StartDev(GameModeId mode)
        {
            _selection.Mode = mode;
            _request.Kind = SessionRequestKind.DevAuto;
            EnterGameAsync().Forget();
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _visibilitySubscription.Dispose();
        }

        private void Join(string code, string password)
        {
            _request.Kind = SessionRequestKind.Join;
            _request.RoomCode = code;
            _request.RoomName = string.Empty;
            _request.Password = TextRules.SanitizePassword(password);
            _request.IsPrivate = false;
            EnterGameAsync().Forget();
        }

        private async UniTaskVoid EnterGameAsync()
        {
            if (_menu.IsLoading.Value)
            {
                return;
            }

            _menu.IsLoading.Value = true;
            _menu.Notice.Value = MenuNotice.None;
            await _lobbyService.DisconnectAsync();
            await _navigator.EnterGameAsync();
            _menu.IsLoading.Value = false;
        }

        private async UniTaskVoid ConnectLobbyAsync()
        {
            bool isConnected = await _lobbyService.ConnectAsync(_cts.Token);
            if (!isConnected && _menu.Screen.Value != MenuScreen.Main)
            {
                _menu.Notice.Value = MenuNotice.LobbyFailed;
            }
        }

        private void OnMenuShown()
        {
            if (_menu.Screen.Value != MenuScreen.Main && _lobby.Status.Value != LobbyStatus.Connected)
            {
                _menu.Screen.Value = MenuScreen.Lobby;
                ConnectLobbyAsync().Forget();
            }
        }
    }
}
