// 플레이어 애니메이션.
// 플레이어 순번으로 캐릭터 모델 하나를 고르고, 표시 상태를 Animator 값으로 옮긴다.
// 하체는 이동 블렌드, 상체 레이어는 소총을 그냥 들고 있을 때는 꺼서 이동 애니메이션 자체의 상체를 쓰고, 사격·재장전·회복·권총·칼일 때만 부드럽게 켜서 덮어쓴다.
// 쓰러짐·부활·방어막 설치는 전신 동작이다.
// 위아래 조준은 척추를 나눠 숙이거나 젖혀 맞추고, 권총 반동은 팔을 잠깐 들어 올려 흉내 낸다.

using R3;
using UnityEngine;
using ZD.Gameplay.Players;
using ZD.Gameplay.Weapons;

namespace ZD.Presentation.Players
{
    [DisallowMultipleComponent]
    public sealed class PlayerAnimatorView : MonoBehaviour
    {
        private const int UPPER_LAYER = 1;
        private const float FIRE_HOLD_SECONDS = 0.15f;
        private const float MELEE_SECONDS = 0.6f;
        private const float RECOIL_SECONDS = 0.12f;
        private const float RECOIL_DEGREES = 14f;
        private const float UPPER_WEIGHT_SPEED = 8f;
        private const float MAX_AIM_PITCH = 60f;

        // 위아래 조준 각도를 척추 세 마디에 나눠 준다(합이 1). UpperChest가 없는 모델은 그 몫을 Chest가 받는다.
        private const float SPINE_PITCH_SHARE = 0.3f;
        private const float CHEST_PITCH_SHARE = 0.35f;
        private const float UPPER_CHEST_PITCH_SHARE = 0.35f;

        // 상체 상태 번호. Animator 컨트롤러의 Upper 값과 같다.
        private const int UPPER_RIFLE = 0;
        private const int UPPER_PISTOL = 1;
        private const int UPPER_KNIFE = 2;
        private const int UPPER_HEAL = 3;
        private const int UPPER_RELOAD = 4;
        private const int UPPER_RIFLE_FIRE = 5;
        private const int UPPER_UNARMED = 6;
        private const int UPPER_KNIFE_ATTACK = 7;

        private static readonly int MoveXId = Animator.StringToHash("MoveX");
        private static readonly int MoveYId = Animator.StringToHash("MoveY");
        private static readonly int SpeedId = Animator.StringToHash("Speed");
        private static readonly int DownedId = Animator.StringToHash("Downed");
        private static readonly int RevivingId = Animator.StringToHash("Reviving");
        private static readonly int BuildingId = Animator.StringToHash("Building");
        private static readonly int UpperId = Animator.StringToHash("Upper");

        [SerializeField] private Animator[] _variants;

        private readonly CompositeDisposable _disposables = new();
        private IPlayerDisplay _display;
        private Animator _animator;
        private Transform _rightUpperArm;
        private Transform _spine;
        private Transform _chest;
        private Transform _upperChest;
        private float _chestPitchShare;
        private int _lastShotCount = -1;
        private float _firingUntil;
        private float _meleeUntil;
        private float _recoilUntil;
        private bool _wasFiring;
        private bool _wasMeleeing;
        private float _upperWeight;
        private float _targetUpperWeight;
        private bool _isFullBodyAction;

        private void Awake()
        {
            _display = GetComponent<IPlayerDisplay>();
            for (int variantIndex = 0; variantIndex < _variants.Length; variantIndex++)
            {
                _variants[variantIndex].gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            _display.DisplayVisualIndex.Subscribe(SelectVariant).AddTo(_disposables);
            _display.DisplayMove.Subscribe(ApplyMove).AddTo(_disposables);
            _display.DisplayShotCount.Subscribe(OnShotCountChanged).AddTo(_disposables);
            Observable.Merge(
                    _display.DisplayDowned.AsUnitObservable(),
                    _display.DisplayReviving.AsUnitObservable(),
                    _display.DisplayHealing.AsUnitObservable(),
                    _display.DisplayReloading.AsUnitObservable(),
                    _display.DisplayBuilding.AsUnitObservable(),
                    _display.DisplayPose.AsUnitObservable())
                .Subscribe(_ => RefreshStates())
                .AddTo(_disposables);
        }

        private void LateUpdate()
        {
            if (_animator == null)
            {
                return;
            }

            float now = Time.time;
            bool isFiring = now < _firingUntil;
            bool isMeleeing = now < _meleeUntil;
            if (isFiring != _wasFiring || isMeleeing != _wasMeleeing)
            {
                _wasFiring = isFiring;
                _wasMeleeing = isMeleeing;
                RefreshStates();
            }

            if (!Mathf.Approximately(_upperWeight, _targetUpperWeight))
            {
                _upperWeight = Mathf.MoveTowards(_upperWeight, _targetUpperWeight, UPPER_WEIGHT_SPEED * Time.deltaTime);
                _animator.SetLayerWeight(UPPER_LAYER, _upperWeight);
            }

            if (!_isFullBodyAction)
            {
                ApplyAimPitch(Mathf.Clamp(_display.DisplayAimPitch.CurrentValue, -MAX_AIM_PITCH, MAX_AIM_PITCH));
            }

            if (now < _recoilUntil && _rightUpperArm != null)
            {
                float strength = (_recoilUntil - now) / RECOIL_SECONDS;
                _rightUpperArm.rotation = Quaternion.AngleAxis(-RECOIL_DEGREES * strength, transform.right) * _rightUpperArm.rotation;
            }
        }

        private void OnDestroy() => _disposables.Dispose();

        private void SelectVariant(int visualIndex)
        {
            if (visualIndex < 0 || _variants.Length == 0)
            {
                return;
            }

            int selected = visualIndex % _variants.Length;
            for (int variantIndex = 0; variantIndex < _variants.Length; variantIndex++)
            {
                _variants[variantIndex].gameObject.SetActive(variantIndex == selected);
            }

            _animator = _variants[selected];
            _rightUpperArm = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            _spine = _animator.GetBoneTransform(HumanBodyBones.Spine);
            _chest = _animator.GetBoneTransform(HumanBodyBones.Chest);
            _upperChest = _animator.GetBoneTransform(HumanBodyBones.UpperChest);
            _chestPitchShare = _upperChest == null ? CHEST_PITCH_SHARE + UPPER_CHEST_PITCH_SHARE : CHEST_PITCH_SHARE;
            ApplyMove(_display.DisplayMove.CurrentValue);
            RefreshStates();
            _upperWeight = _targetUpperWeight;
            _animator.SetLayerWeight(UPPER_LAYER, _upperWeight);
        }

        private void ApplyMove(Vector2 move)
        {
            if (_animator == null)
            {
                return;
            }

            _animator.SetFloat(MoveXId, move.x);
            _animator.SetFloat(MoveYId, move.y);
            _animator.SetFloat(SpeedId, move.magnitude);
        }

        private void OnShotCountChanged(int shotCount)
        {
            bool isNewShot = _lastShotCount >= 0 && shotCount > _lastShotCount;
            _lastShotCount = shotCount;
            if (!isNewShot)
            {
                return;
            }

            float now = Time.time;
            switch (_display.DisplayPose.CurrentValue)
            {
                case WeaponPose.Rifle:
                    _firingUntil = now + FIRE_HOLD_SECONDS;
                    break;
                case WeaponPose.Pistol:
                    _recoilUntil = now + RECOIL_SECONDS;
                    break;
                case WeaponPose.Knife:
                    _meleeUntil = now + MELEE_SECONDS;
                    break;
            }

            RefreshStates();
        }

        private void RefreshStates()
        {
            if (_animator == null)
            {
                return;
            }

            bool isDowned = _display.DisplayDowned.CurrentValue;
            bool isReviving = _display.DisplayReviving.CurrentValue;
            bool isBuilding = _display.DisplayBuilding.CurrentValue;
            _isFullBodyAction = isDowned || isReviving || isBuilding;
            _animator.SetBool(DownedId, isDowned);
            _animator.SetBool(RevivingId, isReviving && !isDowned);
            _animator.SetBool(BuildingId, isBuilding && !isDowned);

            int upperState = SelectUpperState();
            _animator.SetInteger(UpperId, upperState);
            _targetUpperWeight = _isFullBodyAction || upperState == UPPER_RIFLE ? 0f : 1f;
        }

        private int SelectUpperState()
        {
            if (_display.DisplayHealing.CurrentValue)
            {
                return UPPER_HEAL;
            }

            if (_display.DisplayReloading.CurrentValue)
            {
                return UPPER_RELOAD;
            }

            float now = Time.time;
            return _display.DisplayPose.CurrentValue switch
            {
                WeaponPose.Knife => now < _meleeUntil ? UPPER_KNIFE_ATTACK : UPPER_KNIFE,
                WeaponPose.Rifle => now < _firingUntil ? UPPER_RIFLE_FIRE : UPPER_RIFLE,
                WeaponPose.Pistol => UPPER_PISTOL,
                _ => UPPER_UNARMED,
            };
        }

        private void ApplyAimPitch(float pitch)
        {
            Vector3 axis = transform.right;
            RotateBone(_spine, axis, pitch * SPINE_PITCH_SHARE);
            RotateBone(_chest, axis, pitch * _chestPitchShare);
            RotateBone(_upperChest, axis, pitch * UPPER_CHEST_PITCH_SHARE);
        }

        private static void RotateBone(Transform bone, Vector3 axis, float degrees)
        {
            if (bone != null)
            {
                bone.rotation = Quaternion.AngleAxis(degrees, axis) * bone.rotation;
            }
        }
    }
}
