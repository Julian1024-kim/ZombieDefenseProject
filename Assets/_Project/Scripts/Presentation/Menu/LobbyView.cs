// 방 목록 화면: 고른 모드의 공개된 방(이름, 방장, 인원, 잠금)과 참가 버튼, 방 만들기, 방 코드(+비밀번호)로 참가, 뒤로.
// 줄은 미리 만들어 둔 만큼(_rowRoots)만 보이고, 목록이 바뀔 때만 다시 그린다.
// 방 만들기·비밀번호 창이 떠 있으면 누를 수 없게 한다.

using System.Text;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Lobby;

namespace ZD.Presentation.Menu
{
    public sealed class LobbyView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _panel;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private GameObject[] _rowRoots;
        [SerializeField] private TMP_Text[] _rowNames;
        [SerializeField] private TMP_Text[] _rowInfos;
        [SerializeField] private Button[] _rowJoinButtons;
        [SerializeField] private Button _createButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_InputField _codeInput;
        [SerializeField] private TMP_InputField _codePasswordInput;
        [SerializeField] private Button _joinCodeButton;

        private readonly CompositeDisposable _disposables = new();
        private readonly StringBuilder _builder = new(64);
        private string[] _rowCodes;
        private MenuSystem _menuSystem;
        private MenuModel _menu;
        private LobbyModel _lobby;
        private ModeSelectionModel _selection;
        private int _shownRoomCount;

        [Inject]
        public void Construct(MenuSystem menuSystem, MenuModel menu, LobbyModel lobby, ModeSelectionModel selection)
        {
            _menuSystem = menuSystem;
            _menu = menu;
            _lobby = lobby;
            _selection = selection;
        }

        private void Start()
        {
            _rowCodes = new string[_rowRoots.Length];
            for (int rowIndex = 0; rowIndex < _rowJoinButtons.Length; rowIndex++)
            {
                int row = rowIndex;
                _rowJoinButtons[rowIndex].onClick.AddListener(() => _menuSystem.JoinListed(_rowCodes[row]));
            }

            _codeInput.characterLimit = RoomCodes.LENGTH;
            _codePasswordInput.characterLimit = TextRules.MAX_PASSWORD_LENGTH;
            _createButton.onClick.AddListener(_menuSystem.OpenCreateRoom);
            _backButton.onClick.AddListener(_menuSystem.BackToMain);
            _joinCodeButton.onClick.AddListener(OnJoinCodeClicked);

            _menu.Screen.Subscribe(OnScreenChanged).AddTo(_disposables);
            Observable.Merge(
                    _lobby.RoomsVersion.AsUnitObservable(),
                    _lobby.Status.AsUnitObservable(),
                    _menu.Notice.AsUnitObservable(),
                    _menu.IsLoading.AsUnitObservable())
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            for (int rowIndex = 0; rowIndex < _rowJoinButtons.Length; rowIndex++)
            {
                _rowJoinButtons[rowIndex].onClick.RemoveAllListeners();
            }

            _createButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
            _joinCodeButton.onClick.RemoveAllListeners();
            _disposables.Dispose();
        }

        private void OnScreenChanged(MenuScreen screen)
        {
            bool isShown = screen != MenuScreen.Main;
            MenuPanels.SetVisible(_panel, isShown);
            _panel.interactable = screen == MenuScreen.Lobby;
            if (isShown)
            {
                _titleText.SetText(_selection.Mode == GameModeId.BaseDefense ? "BASE DEFENSE ROOMS" : "SURVIVAL ROOMS");
                Refresh();
            }
        }

        private void Refresh()
        {
            if (_rowCodes == null)
            {
                return;
            }

            _shownRoomCount = 0;
            for (int roomIndex = 0; roomIndex < _lobby.Rooms.Count && _shownRoomCount < _rowRoots.Length; roomIndex++)
            {
                RoomInfo room = _lobby.Rooms[roomIndex];
                if (room.Mode == _selection.Mode)
                {
                    ShowRow(_shownRoomCount, room);
                    _shownRoomCount++;
                }
            }

            for (int rowIndex = _shownRoomCount; rowIndex < _rowRoots.Length; rowIndex++)
            {
                _rowRoots[rowIndex].SetActive(false);
            }

            bool canAct = !_menu.IsLoading.Value;
            _createButton.interactable = canAct;
            _joinCodeButton.interactable = canAct;
            _statusText.SetText(StatusMessage());
        }

        private void ShowRow(int rowIndex, in RoomInfo room)
        {
            _rowRoots[rowIndex].SetActive(true);
            _rowCodes[rowIndex] = room.Code;
            _rowNames[rowIndex].SetText(room.Name);
            _builder.Clear();
            _builder.Append(room.HostName).Append("  ·  ").Append(room.PlayerCount).Append(" / ").Append(room.MaxPlayers);
            if (room.IsLocked)
            {
                _builder.Append("  ·  LOCKED");
            }

            _rowInfos[rowIndex].SetText(_builder);
            _rowJoinButtons[rowIndex].interactable = room.CanJoin && !_menu.IsLoading.Value;
        }

        private string StatusMessage()
        {
            if (_menu.IsLoading.Value)
            {
                return "JOINING...";
            }

            switch (_menu.Notice.Value)
            {
                case MenuNotice.RoomNotFound:
                    return "ROOM NOT FOUND";
                case MenuNotice.RoomFull:
                    return "THE ROOM IS FULL";
                case MenuNotice.WrongPassword:
                    return "WRONG PASSWORD";
                case MenuNotice.ConnectFailed:
                    return "COULD NOT CONNECT TO THE ROOM";
                case MenuNotice.HostLeft:
                    return "THE HOST LEFT THE ROOM";
                case MenuNotice.InvalidCode:
                    return "ENTER A 6-CHARACTER ROOM CODE";
                case MenuNotice.LobbyFailed:
                    return "COULD NOT CONNECT TO THE LOBBY - CHECK YOUR NETWORK";
            }

            if (_lobby.Status.Value != LobbyStatus.Connected)
            {
                return "CONNECTING TO THE LOBBY...";
            }

            return _shownRoomCount == 0 ? "NO ROOMS YET - CREATE ONE!" : string.Empty;
        }

        private void OnJoinCodeClicked() => _menuSystem.JoinWithCode(_codeInput.text, _codePasswordInput.text);
    }
}
