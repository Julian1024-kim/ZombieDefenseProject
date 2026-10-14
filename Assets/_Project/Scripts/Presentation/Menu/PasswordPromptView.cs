// 잠긴 방에 참가할 때 비밀번호를 묻는 창.
// 어느 방인지 보여 주고, 입력한 비밀번호로 참가하거나 취소한다.

using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Lobby;

namespace ZD.Presentation.Menu
{
    public sealed class PasswordPromptView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _panel;
        [SerializeField] private TMP_Text _roomText;
        [SerializeField] private TMP_InputField _passwordInput;
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _cancelButton;

        private readonly CompositeDisposable _disposables = new();
        private MenuSystem _menuSystem;
        private MenuModel _menu;

        [Inject]
        public void Construct(MenuSystem menuSystem, MenuModel menu)
        {
            _menuSystem = menuSystem;
            _menu = menu;
        }

        private void Start()
        {
            _passwordInput.characterLimit = TextRules.MAX_PASSWORD_LENGTH;
            _joinButton.onClick.AddListener(OnJoinClicked);
            _cancelButton.onClick.AddListener(_menuSystem.CloseDialog);
            _menu.Screen.Subscribe(OnScreenChanged).AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _joinButton.onClick.RemoveAllListeners();
            _cancelButton.onClick.RemoveAllListeners();
            _disposables.Dispose();
        }

        private void OnScreenChanged(MenuScreen screen)
        {
            bool isShown = screen == MenuScreen.Password;
            MenuPanels.SetVisible(_panel, isShown);
            if (isShown)
            {
                _roomText.SetText(_menu.PendingRoom.Name);
                _passwordInput.SetTextWithoutNotify(string.Empty);
            }
        }

        private void OnJoinClicked() => _menuSystem.SubmitPassword(_passwordInput.text);
    }
}
