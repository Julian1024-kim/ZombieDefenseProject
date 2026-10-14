// 플레이어의 네트워크 어댑터.
// IPlayerModel 뒤에 체력을 두고, Gameplay 시스템을 Fusion 틱 안에서 돌려서 예측과 롤백 재실행이 틱 단위로 맞는다.
// 지갑과 인벤토리 상태는 옆의 PlayerLoadoutNetwork에 있다.
// 애니메이션용 표시 상태(IPlayerDisplay)는 모든 피어가 Render에서 복제된 값으로 갱신한다.
// 방어막 설치 요청은 RPC로 호스트에 보내고, 설치하는 동안에는 무기를 쓰지 않는다.

using System;
using Fusion;
using Fusion.Addons.SimpleKCC;
using MessagePipe;
using R3;
using UnityEngine;
using VContainer;
using ZD.Core;
using ZD.Core.Messages;
using ZD.Gameplay.Building;
using ZD.Gameplay.Cameras;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Economy;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Hud;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Lobby;
using ZD.Gameplay.Players;
using ZD.Gameplay.Stats;
using ZD.Gameplay.Weapons;
using ZD.Network.Input;

namespace ZD.Network.Players
{
    [RequireComponent(typeof(SimpleKCC), typeof(PlayerLoadoutNetwork))]
    public sealed class PlayerNetwork : NetworkBehaviour, IPlayerModel, IShopRequestSender, IBuildRequestSender, IReviveModel, IPlayerDisplay
    {
        private SimpleKCC _kcc;
        private KccCharacterMotor _motor;
        private PlayerLoadoutNetwork _loadout;
        private IMuzzlePoint _muzzlePoint;
        private int _lastRenderedShotCount;
        private IDisposable _restartSubscription;

        private readonly ReactiveProperty<Vector2> _displayMove = new(Vector2.zero);
        private readonly ReactiveProperty<bool> _displayDowned = new(false);
        private readonly ReactiveProperty<bool> _displayReviving = new(false);
        private readonly ReactiveProperty<bool> _displayHealing = new(false);
        private readonly ReactiveProperty<bool> _displayReloading = new(false);
        private readonly ReactiveProperty<WeaponPose> _displayPose = new(WeaponPose.None);
        private readonly ReactiveProperty<int> _displayShotCount = new(0);
        private readonly ReactiveProperty<int> _displayVisualIndex = new(-1);
        private readonly ReactiveProperty<float> _displayAimPitch = new(0f);
        private readonly ReactiveProperty<bool> _displayBuilding = new(false);
        private readonly ReactiveProperty<byte> _displayWeaponId = new(0);

        private PlayerDefinition _definition;
        private PlayerProfileModel _profile;
        private PlayerStateSystem _stateSystem;
        private CameraSettings _cameraSettings;
        private InventorySystem _inventorySystem;
        private WalletSystem _walletSystem;
        private ShopSystem _shopSystem;
        private ReviveSystem _reviveSystem;
        private GameFlowSystem _gameFlow;
        private LocalInputSystem _localInput;
        private PlayerRosterModel _roster;
        private CameraModel _cameraModel;
        private LocalPlayerHudModel _hud;
        private ShopUiModel _shopUi;
        private BuildSystem _buildSystem;
        private BuildModeModel _buildMode;
        private BarricadeRosterModel _barricades;
        private IPublisher<WeaponFiredMessage> _weaponFiredPublisher;
        private ISubscriber<MatchRestartedMessage> _matchRestartedSubscriber;

        [Networked] public int Health { get; set; }
        [Networked] private NetworkButtons PreviousButtons { get; set; }
        [Networked] private int ShotCount { get; set; }
        [Networked] private byte LastSlotSequence { get; set; }
        [Networked] public int ReviveTargetId { get; set; }
        [Networked] public int ReviveEndTick { get; set; }
        [Networked] private NetworkString<_16> NicknameValue { get; set; }
        [Networked] public PlayerStateId ActionState { get; set; }

        public int EntityId => Object.InputAuthority.AsIndex;
        public DamageTargetKind Kind => DamageTargetKind.Player;
        public int MaxHealth => _definition.MaxHealth;
        public Vector3 Position => _kcc.Position;
        public IWalletModel Wallet => _loadout;
        public IPlayerStatsModel Stats => _loadout;

        // 문자열을 새로 만들므로 게임 오버 결과처럼 드물게만 읽는다.
        public string Nickname => NicknameValue.ToString();
        public IInventoryModel Inventory => _loadout;
        public IReviveModel Revive => this;
        public ReadOnlyReactiveProperty<Vector2> DisplayMove => _displayMove;
        public ReadOnlyReactiveProperty<bool> DisplayDowned => _displayDowned;
        public ReadOnlyReactiveProperty<bool> DisplayReviving => _displayReviving;
        public ReadOnlyReactiveProperty<bool> DisplayHealing => _displayHealing;
        public ReadOnlyReactiveProperty<bool> DisplayReloading => _displayReloading;
        public ReadOnlyReactiveProperty<WeaponPose> DisplayPose => _displayPose;
        public ReadOnlyReactiveProperty<int> DisplayShotCount => _displayShotCount;
        public ReadOnlyReactiveProperty<int> DisplayVisualIndex => _displayVisualIndex;
        public ReadOnlyReactiveProperty<float> DisplayAimPitch => _displayAimPitch;
        public ReadOnlyReactiveProperty<bool> DisplayBuilding => _displayBuilding;
        public ReadOnlyReactiveProperty<byte> DisplayWeaponId => _displayWeaponId;

        private void Awake()
        {
            _kcc = GetComponent<SimpleKCC>();
            _loadout = GetComponent<PlayerLoadoutNetwork>();
            _muzzlePoint = GetComponent<IMuzzlePoint>();
            _motor = new KccCharacterMotor(_kcc);
        }

        [Inject]
        public void Construct(
            PlayerDefinition definition,
            CameraSettings cameraSettings,
            InventorySystem inventorySystem,
            WalletSystem walletSystem,
            ShopSystem shopSystem,
            ReviveSystem reviveSystem,
            GameFlowSystem gameFlow,
            LocalInputSystem localInput,
            PlayerRosterModel roster,
            CameraModel cameraModel,
            LocalPlayerHudModel hud,
            ShopUiModel shopUi,
            BuildSystem buildSystem,
            BuildModeModel buildMode,
            BarricadeRosterModel barricades,
            PlayerProfileModel profile,
            PlayerStateSystem stateSystem,
            IPublisher<WeaponFiredMessage> weaponFiredPublisher,
            ISubscriber<MatchRestartedMessage> matchRestartedSubscriber)
        {
            _profile = profile;
            _stateSystem = stateSystem;
            _definition = definition;
            _cameraSettings = cameraSettings;
            _inventorySystem = inventorySystem;
            _walletSystem = walletSystem;
            _shopSystem = shopSystem;
            _reviveSystem = reviveSystem;
            _gameFlow = gameFlow;
            _localInput = localInput;
            _roster = roster;
            _cameraModel = cameraModel;
            _hud = hud;
            _shopUi = shopUi;
            _buildSystem = buildSystem;
            _buildMode = buildMode;
            _barricades = barricades;
            _weaponFiredPublisher = weaponFiredPublisher;
            _matchRestartedSubscriber = matchRestartedSubscriber;
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                ResetForMatch();
                _restartSubscription = _matchRestartedSubscriber.Subscribe(_ => ResetForMatch());
            }

            _roster.Register(this);
            _lastRenderedShotCount = ShotCount;

            if (HasInputAuthority)
            {
                _localInput.ResetLook(transform.eulerAngles.y, 0f);
                _cameraModel.HasTarget = true;
                _hud.MaxHealth.Value = _definition.MaxHealth;
                _shopUi.RequestSender = this;
                _buildMode.RequestSender = this;
                RPC_SetNickname(_profile.Nickname.Value);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _restartSubscription?.Dispose();
            _restartSubscription = null;
            _roster.Unregister(this);
            if (HasInputAuthority)
            {
                _cameraModel.HasTarget = false;
                _shopUi.RequestSender = null;
                _buildMode.RequestSender = null;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out NetworkInputData input))
            {
                return;
            }

            NetworkButtons pressed = input.Buttons.GetPressed(PreviousButtons);
            PreviousButtons = input.Buttons;

            // 시작(대기 단계)과 재시작(게임 오버)은 호스트 자기 플레이어의 Enter만 받는다. 클라이언트의 Enter는 무시한다.
            // 호스트가 쓰러져 있어도 게임 오버 화면에서 재시작할 수 있다.
            bool isHostPlayer = Object.InputAuthority == Runner.LocalPlayer;
            if (HasStateAuthority && isHostPlayer && pressed.IsSet(InputButton.Restart))
            {
                DevLog.Info($"Restart pressed by the host at tick {Runner.Tick.Raw}");
                _gameFlow.RequestRestart();
                _gameFlow.RequestStart();
            }

            // 슬롯 키는 누를 때마다 한 번만 적용한다: 순번은 누를 때마다 바뀌지만 슬롯 값은 같을 수 있다.
            bool hasSlotRequest = input.SlotSequence != LastSlotSequence;
            LastSlotSequence = input.SlotSequence;

            float yaw = input.YawDegrees;
            float pitch = input.PitchDegrees;
            ThirdPersonAim.GetAimRay(_cameraSettings, _kcc.Position, pitch, yaw, out Vector3 aimOrigin, out Vector3 aimDirection);
            var tickInput = new PlayerTickInput(
                input.Move,
                yaw,
                pitch,
                input.Buttons.IsSet(InputButton.Fire),
                pressed.IsSet(InputButton.Fire),
                pressed.IsSet(InputButton.Reload),
                input.Buttons.IsSet(InputButton.Revive),
                hasSlotRequest,
                (WeaponSlot)input.RequestedSlot,
                aimOrigin,
                aimDirection);

            if (_stateSystem.Tick(new PlayerTickContext(this, _motor, tickInput, HasStateAuthority)))
            {
                ShotCount++;
            }
        }

        public override void Render()
        {
            RaiseShotEffects();
            RefreshDisplay();

            if (!HasInputAuthority)
            {
                return;
            }

            _cameraModel.TargetPosition = transform.position;
            _hud.Health.Value = Health;
            _hud.Points.Value = _loadout.Points;
            _hud.SelectedSlot.Value = _loadout.SelectedSlot;
            _hud.ActivePrimary.Value = _loadout.ActivePrimary;
            _hud.PrimaryAId.Value = _loadout.GetWeaponId(InventorySlots.PRIMARY_A);
            _hud.PrimaryBId.Value = _loadout.GetWeaponId(InventorySlots.PRIMARY_B);
            _hud.SecondaryId.Value = _loadout.GetWeaponId(InventorySlots.SECONDARY);
            _hud.MeleeId.Value = _loadout.GetWeaponId(InventorySlots.MELEE);
            _hud.HealKits.Value = _loadout.HealKits;
            _hud.IsHealing.Value = _loadout.SelectedSlot == WeaponSlot.Healing;
            RefreshReviveHud();

            int weaponIndex = _inventorySystem.GetActiveWeaponIndex(_loadout);
            WeaponDefinition weapon = _inventorySystem.GetWeapon(_loadout, weaponIndex);
            if (weapon == null)
            {
                _hud.UsesAmmo.Value = false;
                return;
            }

            IWeaponModel state = _loadout.GetWeaponState(weaponIndex);
            _hud.UsesAmmo.Value = weapon.UsesAmmo;
            _hud.AmmoInMagazine.Value = state.AmmoInMagazine;
            _hud.ReserveAmmo.Value = state.ReserveAmmo;
            _hud.IsReloading.Value = state.ReloadEndTick != 0;
        }

        public void SendPurchaseRequest(int itemIndex)
        {
            RPC_RequestPurchase((byte)itemIndex);
        }

        public void SendBuildRequest(int itemIndex, Vector3 target, float yaw)
        {
            RPC_RequestBuild((byte)itemIndex, target, yaw);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetNickname(string nickname)
        {
            NicknameValue = TextRules.SanitizeNickname(nickname);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestBuild(byte itemIndex, Vector3 target, float yaw)
        {
            PlacementResult result = _buildSystem.TryBuild(this, itemIndex, target, yaw);
            DevLog.Info($"Build {itemIndex} by {Object.InputAuthority}: {result}");
            if (result != PlacementResult.Valid)
            {
                RPC_BuildRejected(result);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        private void RPC_BuildRejected(PlacementResult result)
        {
            _buildMode.Rejections.OnNext(result);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestPurchase(byte itemIndex)
        {
            PurchaseResult result = _shopSystem.TryPurchase(this, itemIndex);
            DevLog.Info($"Purchase {itemIndex} by {Object.InputAuthority}: {result}, {Wallet.Points} P left");
            RPC_PurchaseResult(result);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        private void RPC_PurchaseResult(PurchaseResult result)
        {
            _shopUi.PurchaseResults.OnNext(result);
        }

        private void RaiseShotEffects()
        {
            int shotCount = ShotCount;
            if (shotCount <= _lastRenderedShotCount)
            {
                // 롤백으로 예측한 횟수가 줄어들 수 있다. 그 사격은 다시 재생하지 않는다.
                _lastRenderedShotCount = shotCount;
                return;
            }

            _lastRenderedShotCount = shotCount;
            WeaponDefinition weapon = _inventorySystem.GetWeapon(_loadout, _inventorySystem.GetActiveWeaponIndex(_loadout));
            if (weapon == null)
            {
                return;
            }

            Vector2 look = _kcc.GetLookRotation(true, true);
            Vector3 position = transform.position;
            ThirdPersonAim.GetAimRay(_cameraSettings, position, look.x, look.y, out Vector3 aimOrigin, out Vector3 aimDirection);
            if (_muzzlePoint == null || !_muzzlePoint.TryGetMuzzlePosition(out Vector3 muzzle))
            {
                // 무기 모델이 없거나 안 보이면 설정의 고정 위치를 쓴다.
                muzzle = position + Quaternion.Euler(0f, look.y, 0f) * _definition.MuzzleOffset;
            }

            _weaponFiredPublisher.Publish(new WeaponFiredMessage(muzzle, aimOrigin, aimDirection, weapon.Range, !weapon.UsesAmmo));
        }

        private void OnDestroy()
        {
            _displayMove.Dispose();
            _displayDowned.Dispose();
            _displayReviving.Dispose();
            _displayHealing.Dispose();
            _displayReloading.Dispose();
            _displayPose.Dispose();
            _displayShotCount.Dispose();
            _displayVisualIndex.Dispose();
            _displayAimPitch.Dispose();
            _displayBuilding.Dispose();
            _displayWeaponId.Dispose();
        }

        private void RefreshDisplay()
        {
            Vector2 look = _kcc.GetLookRotation(true, true);
            float yaw = look.y;
            _displayAimPitch.Value = look.x;
            Vector3 localVelocity = Quaternion.Euler(0f, -yaw, 0f) * _kcc.RealVelocity;
            _displayMove.Value = Vector2.ClampMagnitude(new Vector2(localVelocity.x, localVelocity.z) / _definition.MoveSpeed, 1f);
            _displayDowned.Value = Health <= 0;
            _displayReviving.Value = ReviveSystem.IsReviving(this);
            _displayBuilding.Value = _barricades.IsInstalling(EntityId);
            _displayHealing.Value = _loadout.SelectedSlot == WeaponSlot.Healing && _loadout.HealEndTick != 0;
            _displayShotCount.Value = ShotCount;
            _displayVisualIndex.Value = EntityId;

            int weaponIndex = _inventorySystem.GetActiveWeaponIndex(_loadout);
            WeaponDefinition weapon = _inventorySystem.GetWeapon(_loadout, weaponIndex);
            _displayPose.Value = WeaponPoses.From(weapon);
            _displayWeaponId.Value = weapon != null ? _loadout.GetWeaponId(weaponIndex) : (byte)0;
            _displayReloading.Value = weapon != null && _loadout.GetWeaponState(weaponIndex).ReloadEndTick != 0;
        }

        private void RefreshReviveHud()
        {
            _hud.ReviveProgress.Value = _reviveSystem.GetProgress(this);
            _hud.CanReviveNearby.Value = Health > 0 && !ReviveSystem.IsReviving(this) && _reviveSystem.FindRevivableTeammate(this) != null;

            IPlayerModel reviver = Health <= 0 ? _reviveSystem.FindReviverOf(this) : null;
            _hud.BeingRevivedProgress.Value = reviver == null ? -1f : _reviveSystem.GetProgress(reviver.Revive);
        }

        private void ResetForMatch()
        {
            Health = _definition.MaxHealth;
            _reviveSystem.Cancel(this);
            _inventorySystem.ResetLoadout(_loadout);
            _walletSystem.Reset(_loadout);
        }
    }
}
