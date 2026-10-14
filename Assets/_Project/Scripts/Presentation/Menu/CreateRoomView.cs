// 방 만들기 창: 방 이름(비우면 "{닉네임}'s Room"), 비밀번호(비우면 잠그지 않음), 비공개(목록에 안 보임, 코드로만 참가) 선택.

using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Lobby;

namespace ZD.Presentation.Menu
{
    public sealed class CreateRoomView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _panel;
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_InputField _passwordInput;
        [SerializeField] private Toggle _privateToggle;
        [SerializeField] private Button _createButton;
        [SerializeField] private Button _cancelButton;

        private readonly CompositeDisposable _disposables = new();
        private MenuSystem _menuSystem;
        private MenuModel _menu;
        private PlayerProfileModel _profile;

        [Inject]
        public void Construct(MenuSystem menuSystem, MenuModel menu, PlayerProfileModel profile)
        {
            _menuSystem = menuSystem;
            _menu = menu;
            _profile = profile;
        }

        private void Start()
        {
            _nameInput.characterLimit = TextRules.MAX_ROOM_NAME_LENGTH;
            _passwordInput.characterLimit = TextRules.MAX_PASSWORD_LENGTH;
            _createButton.onClick.AddListener(OnCreateClicked);
            _cancelButton.onClick.AddListener(_menuSystem.CloseDialog);
            _menu.Screen.Subscribe(OnScreenChanged).AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _createButton.onClick.RemoveAllListeners();
            _cancelButton.onClick.RemoveAllListeners();
            _disposables.Dispose();
        }

        private void OnScreenChanged(MenuScreen screen)
        {
            bool isShown = screen == MenuScreen.CreateRoom;
            MenuPanels.SetVisible(_panel, isShown);
            if (!isShown)
            {
                return;
            }

            _nameInput.SetTextWithoutNotify(string.Empty);
            _passwordInput.SetTextWithoutNotify(string.Empty);
            _privateToggle.SetIsOnWithoutNotify(false);
            if (_nameInput.placeholder is TMP_Text placeholder)
            {
                placeholder.SetText(TextRules.SanitizeRoomName(string.Empty, _profile.Nickname.Value));
            }
        }

        private void OnCreateClicked() => _menuSystem.CreateRoom(_nameInput.text, _passwordInput.text, _privateToggle.isOn);
    }
}
