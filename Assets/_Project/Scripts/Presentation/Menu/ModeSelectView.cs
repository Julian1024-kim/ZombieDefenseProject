// 시작 화면의 처음 화면: 닉네임 입력과 모드 버튼 2개(거점 방어, 생존).
// 모드를 누르면 그 모드의 방 목록이 열린다.
// 게임 중에는 시작 화면 전체(카메라, 이벤트 시스템, 캔버스)를 끄고, 돌아오면 다시 켜며 마우스 커서를 풀어 준다.
// Game 씬에도 이벤트 시스템이 있어서, 불러오기 시작할 때 시작 화면의 이벤트 시스템을 먼저 끈다(둘이 동시에 켜지지 않게).

using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Lobby;

namespace ZD.Presentation.Menu
{
    public sealed class ModeSelectView : MonoBehaviour
    {
        [SerializeField] private GameObject _menuRoot;
        [SerializeField] private GameObject _eventSystem;
        [SerializeField] private CanvasGroup _panel;
        [SerializeField] private TMP_InputField _nicknameInput;
        [SerializeField] private Button _baseDefenseButton;
        [SerializeField] private Button _survivalButton;
        [SerializeField] private TMP_Text _statusText;

        private readonly CompositeDisposable _disposables = new();
        private MenuSystem _menuSystem;
        private MenuModel _menu;
        private ProfileSystem _profileSystem;
        private PlayerProfileModel _profile;

        [Inject]
        public void Construct(MenuSystem menuSystem, MenuModel menu, ProfileSystem profileSystem, PlayerProfileModel profile)
        {
            _menuSystem = menuSystem;
            _menu = menu;
            _profileSystem = profileSystem;
            _profile = profile;
        }

        private void Start()
        {
            _nicknameInput.characterLimit = TextRules.MAX_NICKNAME_LENGTH;
            _nicknameInput.onEndEdit.AddListener(OnNicknameEdited);
            _baseDefenseButton.onClick.AddListener(OnBaseDefenseClicked);
            _survivalButton.onClick.AddListener(OnSurvivalClicked);
            _menu.IsVisible.Subscribe(SetVisible).AddTo(_disposables);
            _menu.Screen.Subscribe(screen => MenuPanels.SetVisible(_panel, screen == MenuScreen.Main)).AddTo(_disposables);
            _menu.IsLoading.Subscribe(SetLoading).AddTo(_disposables);
            _profile.Nickname.Subscribe(nickname => _nicknameInput.SetTextWithoutNotify(nickname)).AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _nicknameInput.onEndEdit.RemoveListener(OnNicknameEdited);
            _baseDefenseButton.onClick.RemoveListener(OnBaseDefenseClicked);
            _survivalButton.onClick.RemoveListener(OnSurvivalClicked);
            _disposables.Dispose();
        }

        private void OnNicknameEdited(string input) => _nicknameInput.SetTextWithoutNotify(_profileSystem.SetNickname(input));

        private void OnBaseDefenseClicked() => _menuSystem.OpenLobby(GameModeId.BaseDefense);

        private void OnSurvivalClicked() => _menuSystem.OpenLobby(GameModeId.Survival);

        private void SetVisible(bool isVisible)
        {
            _menuRoot.SetActive(isVisible);
            if (isVisible)
            {
                _eventSystem.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void SetLoading(bool isLoading)
        {
            if (isLoading)
            {
                _eventSystem.SetActive(false);
            }

            _baseDefenseButton.interactable = !isLoading;
            _survivalButton.interactable = !isLoading;
            _statusText.SetText(isLoading ? "LOADING..." : string.Empty);
        }
    }
}
