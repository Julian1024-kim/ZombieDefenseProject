// PlayerControls를 들고 있는 유일한 곳. 입력을 읽어서 시스템에 넘기기만 하고 로직은 없다.
// 상점을 열고 닫을 때 Gameplay/UI 액션 맵을 바꾼다.
// V로 설치 모드를 켜면 숫자키는 방어막 종류, 클릭은 설치 확정이 된다(Esc나 V로 끔).
// Esc는 커서를 풀고, 다음 클릭은 사격 없이 커서만 다시 잡는다.
// 게임 오버 화면에서는 Esc가 시작 화면으로 나가기다.

using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using ZD.Gameplay.Building;
using ZD.Gameplay.Economy;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Players;

namespace ZD.Presentation.Input
{
    [DisallowMultipleComponent]
    public sealed class InputView : MonoBehaviour
    {
        private readonly CompositeDisposable _disposables = new();
        private PlayerControls _controls;
        private LocalInputSystem _inputSystem;
        private ShopUiSystem _shopUiSystem;
        private ShopUiModel _shopUi;
        private BuildModeSystem _buildModeSystem;
        private BuildModeModel _buildMode;
        private MatchExitSystem _matchExit;

        private static bool IsCursorCaptured => Cursor.lockState == CursorLockMode.Locked;

        private void Awake()
        {
            _controls = new PlayerControls();
        }

        private bool IsBuilding => _buildMode.IsActive.Value;

        [Inject]
        public void Construct(
            LocalInputSystem inputSystem,
            ShopUiSystem shopUiSystem,
            ShopUiModel shopUi,
            BuildModeSystem buildModeSystem,
            BuildModeModel buildMode,
            MatchExitSystem matchExit)
        {
            _matchExit = matchExit;
            _inputSystem = inputSystem;
            _shopUiSystem = shopUiSystem;
            _shopUi = shopUi;
            _buildModeSystem = buildModeSystem;
            _buildMode = buildMode;
        }

        private void OnEnable()
        {
            _controls.Gameplay.Fire.performed += OnFirePerformed;
            _controls.Gameplay.Fire.canceled += OnFireCanceled;
            _controls.Gameplay.Reload.performed += OnReload;
            _controls.Gameplay.SlotPrimary.performed += OnSlotPrimary;
            _controls.Gameplay.SlotSecondary.performed += OnSlotSecondary;
            _controls.Gameplay.SlotMelee.performed += OnSlotMelee;
            _controls.Gameplay.SlotHealing.performed += OnSlotHealing;
            _controls.Gameplay.Interact.performed += OnInteract;
            _controls.Gameplay.Revive.performed += OnRevivePerformed;
            _controls.Gameplay.Revive.canceled += OnReviveCanceled;
            _controls.Gameplay.ToggleBuild.performed += OnToggleBuild;
            _controls.Gameplay.Restart.performed += OnRestart;
            _controls.Gameplay.ReleaseCursor.performed += OnReleaseCursor;
            _controls.UI.CloseMenu.performed += OnCloseMenu;
            SwitchToGameplay();
        }

        private void Start()
        {
            _shopUi.IsOpen
                .Subscribe(isOpen =>
                {
                    if (isOpen)
                    {
                        SwitchToUi();
                    }
                    else if (isActiveAndEnabled)
                    {
                        SwitchToGameplay();
                    }
                })
                .AddTo(_disposables);
        }

        private void Update()
        {
            _inputSystem.SetMoveInput(_controls.Gameplay.Move.ReadValue<Vector2>());
            if (IsCursorCaptured)
            {
                _inputSystem.AddLookInput(_controls.Gameplay.Look.ReadValue<Vector2>());
            }
        }

        private void OnDisable()
        {
            _controls.Gameplay.Fire.performed -= OnFirePerformed;
            _controls.Gameplay.Fire.canceled -= OnFireCanceled;
            _controls.Gameplay.Reload.performed -= OnReload;
            _controls.Gameplay.SlotPrimary.performed -= OnSlotPrimary;
            _controls.Gameplay.SlotSecondary.performed -= OnSlotSecondary;
            _controls.Gameplay.SlotMelee.performed -= OnSlotMelee;
            _controls.Gameplay.SlotHealing.performed -= OnSlotHealing;
            _controls.Gameplay.Interact.performed -= OnInteract;
            _controls.Gameplay.Revive.performed -= OnRevivePerformed;
            _controls.Gameplay.Revive.canceled -= OnReviveCanceled;
            _controls.Gameplay.ToggleBuild.performed -= OnToggleBuild;
            _controls.Gameplay.Restart.performed -= OnRestart;
            _controls.Gameplay.ReleaseCursor.performed -= OnReleaseCursor;
            _controls.UI.CloseMenu.performed -= OnCloseMenu;
            _controls.Gameplay.Disable();
            _controls.UI.Disable();
            SetCursorCaptured(false);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _controls.Dispose();
        }

        private void SwitchToGameplay()
        {
            _controls.UI.Disable();
            _controls.Gameplay.Enable();
            SetCursorCaptured(true);
        }

        private void SwitchToUi()
        {
            _buildModeSystem.Exit();
            _inputSystem.SetFireHeld(false);
            _inputSystem.SetReviveHeld(false);
            _inputSystem.SetMoveInput(Vector2.zero);
            _controls.Gameplay.Disable();
            _controls.UI.Enable();
            SetCursorCaptured(false);
        }

        private static void SetCursorCaptured(bool isCaptured)
        {
            Cursor.lockState = isCaptured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isCaptured;
        }

        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            if (!IsCursorCaptured)
            {
                SetCursorCaptured(true);
                return;
            }

            if (IsBuilding)
            {
                _buildModeSystem.Confirm();
                return;
            }

            _inputSystem.SetFireHeld(true);
        }

        private void OnFireCanceled(InputAction.CallbackContext context) => _inputSystem.SetFireHeld(false);
        private void OnReload(InputAction.CallbackContext context) => _inputSystem.RequestReload();
        private void OnSlotPrimary(InputAction.CallbackContext context) => OnNumberKey(0, WeaponSlot.Primary);
        private void OnSlotSecondary(InputAction.CallbackContext context) => OnNumberKey(1, WeaponSlot.Secondary);
        private void OnSlotMelee(InputAction.CallbackContext context) => OnNumberKey(2, WeaponSlot.Melee);
        private void OnSlotHealing(InputAction.CallbackContext context) => OnNumberKey(3, WeaponSlot.Healing);

        private void OnNumberKey(int buildIndex, WeaponSlot slot)
        {
            if (IsBuilding)
            {
                _buildModeSystem.Select(buildIndex);
            }
            else
            {
                _inputSystem.RequestSlot(slot);
            }
        }

        private void OnToggleBuild(InputAction.CallbackContext context)
        {
            _inputSystem.SetFireHeld(false);
            _buildModeSystem.Toggle();
        }
        private void OnInteract(InputAction.CallbackContext context) => _shopUiSystem.Toggle();
        private void OnRevivePerformed(InputAction.CallbackContext context) => _inputSystem.SetReviveHeld(true);

        private void OnReviveCanceled(InputAction.CallbackContext context) => _inputSystem.SetReviveHeld(false);

        private void OnCloseMenu(InputAction.CallbackContext context) => _shopUiSystem.Close();
        private void OnRestart(InputAction.CallbackContext context) => _inputSystem.RequestRestart();

        private void OnReleaseCursor(InputAction.CallbackContext context)
        {
            if (_matchExit.TryLeaveAfterGameOver())
            {
                return;
            }

            if (IsBuilding)
            {
                _buildModeSystem.Exit();
                return;
            }

            _inputSystem.SetFireHeld(false);
            SetCursorCaptured(false);
        }
    }
}
