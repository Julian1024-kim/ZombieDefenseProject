// 손에 든 무기 모델.
// 무기 모델은 캐릭터 모델마다 오른손 뼈 아래에 미리 붙여 두었고(프리팹에 구워 둠), 지금 보이는 캐릭터의, 지금 든 무기 하나만 켠다.
// 쓰러짐·부활·회복·설치 중에는 숨긴다.
// 두 손 무기(소총, 샷건)는 매 프레임 오른손 손바닥에서 왼손 손바닥 쪽으로 총 축을 돌려 두 손에 딱 맞추고(재장전 중에는 풀어 둠), 총구 위치를 기억해 사격 이펙트가 실제 총구에서 나가게 한다.
// 애니메이션 View가 위아래 조준과 반동을 뼈에 얹은 뒤에 돌아야 해서 실행 순서를 늦춘다.

using R3;
using UnityEngine;
using ZD.Gameplay.Players;

namespace ZD.Presentation.Players
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public sealed class PlayerWeaponView : MonoBehaviour, IMuzzlePoint
    {
        private const float TWO_HAND_BLEND_SPEED = 8f;
        private const float MIN_HAND_GAP_SQR = 0.0001f;

        public static readonly Vector3 PalmCenter = new(0f, 0.055f, 0f);

        // _models와 _muzzles는 캐릭터 순서대로 무기 수만큼씩 이어 붙인 배열이다: [캐릭터0 무기0, 캐릭터0 무기1, ..., 캐릭터1 무기0, ...].
        // _gripPoints, _supportAxes는 무기 순서(_weaponIds와 같은 순서)이고, 무기 모델 안의 좌표다. 받침 축이 0이면 한 손 무기다.
        [SerializeField] private GameObject[] _models;
        [SerializeField] private byte[] _weaponIds;
        [SerializeField] private Transform[] _muzzles;
        [SerializeField] private Vector3[] _gripPoints;
        [SerializeField] private Vector3[] _supportAxes;

        private readonly CompositeDisposable _disposables = new();
        private IPlayerDisplay _display;
        private int _variantCount;
        private Transform[] _rightHands;
        private Transform[] _leftHands;
        private Transform[] _leftFingers;
        private Quaternion[] _baseRotations;
        private int _shownModel = -1;
        private int _shownSlot = -1;
        private int _shownVariant = -1;
        private float _twoHandWeight;
        private bool _hasMuzzle;
        private Vector3 _localMuzzle;

        private void Awake()
        {
            _display = GetComponent<IPlayerDisplay>();
            int weaponCount = _weaponIds.Length;
            _variantCount = weaponCount == 0 ? 0 : _models.Length / weaponCount;
            _rightHands = new Transform[_variantCount];
            _leftHands = new Transform[_variantCount];
            _leftFingers = new Transform[_variantCount];
            for (int variantIndex = 0; variantIndex < _variantCount; variantIndex++)
            {
                Transform rightHand = _models[variantIndex * weaponCount].transform.parent;
                Animator animator = rightHand.GetComponentInParent<Animator>(true);
                _rightHands[variantIndex] = rightHand;
                _leftHands[variantIndex] = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                _leftFingers[variantIndex] = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
            }

            _baseRotations = new Quaternion[_models.Length];
            for (int modelIndex = 0; modelIndex < _models.Length; modelIndex++)
            {
                _baseRotations[modelIndex] = _models[modelIndex].transform.localRotation;
                _models[modelIndex].SetActive(false);
            }
        }

        private void Start()
        {
            Observable.Merge(
                    _display.DisplayVisualIndex.AsUnitObservable(),
                    _display.DisplayWeaponId.AsUnitObservable(),
                    _display.DisplayDowned.AsUnitObservable(),
                    _display.DisplayReviving.AsUnitObservable(),
                    _display.DisplayHealing.AsUnitObservable(),
                    _display.DisplayBuilding.AsUnitObservable())
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);
        }

        private void LateUpdate()
        {
            if (_shownModel < 0)
            {
                return;
            }

            bool isTwoHanded = _supportAxes[_shownSlot] != Vector3.zero;
            if (isTwoHanded)
            {
                float target = _display.DisplayReloading.CurrentValue ? 0f : 1f;
                _twoHandWeight = Mathf.MoveTowards(_twoHandWeight, target, TWO_HAND_BLEND_SPEED * Time.deltaTime);
                AlignToBothHands();
            }

            Transform muzzle = _muzzles[_shownModel];
            _hasMuzzle = muzzle != null;
            if (_hasMuzzle)
            {
                _localMuzzle = transform.InverseTransformPoint(muzzle.position);
            }
        }

        private void OnDestroy() => _disposables.Dispose();

        public bool TryGetMuzzlePosition(out Vector3 position)
        {
            bool isAvailable = _shownModel >= 0 && _hasMuzzle;
            position = isAvailable ? transform.TransformPoint(_localMuzzle) : default;
            return isAvailable;
        }

        private void Refresh()
        {
            int visualIndex = _display.DisplayVisualIndex.CurrentValue;
            bool handsBusy = _display.DisplayDowned.CurrentValue || _display.DisplayReviving.CurrentValue
                || _display.DisplayHealing.CurrentValue || _display.DisplayBuilding.CurrentValue;
            int variant = visualIndex < 0 || _variantCount == 0 ? -1 : visualIndex % _variantCount;
            int weaponSlot = handsBusy || variant < 0 ? -1 : FindWeaponSlot(_display.DisplayWeaponId.CurrentValue);
            int shownModel = weaponSlot < 0 ? -1 : variant * _weaponIds.Length + weaponSlot;

            for (int modelIndex = 0; modelIndex < _models.Length; modelIndex++)
            {
                bool isShown = modelIndex == shownModel;
                if (_models[modelIndex].activeSelf != isShown)
                {
                    _models[modelIndex].SetActive(isShown);
                }
            }

            if (shownModel != _shownModel)
            {
                // 새로 든 무기는 두 손에 바로 맞춘 상태로 시작한다(재장전 중이면 다음 프레임부터 풀린다).
                _twoHandWeight = 1f;
                _hasMuzzle = false;
            }

            _shownModel = shownModel;
            _shownSlot = weaponSlot;
            _shownVariant = variant;
        }

        private void AlignToBothHands()
        {
            Transform rightHand = _rightHands[_shownVariant];
            Quaternion baseRotation = rightHand.rotation * _baseRotations[_shownModel];
            Quaternion rotation = baseRotation;
            Vector3 palm = rightHand.TransformPoint(PalmCenter);
            Vector3 leftPalm = (_leftHands[_shownVariant].position + _leftFingers[_shownVariant].position) * 0.5f;
            Vector3 toLeftPalm = leftPalm - palm;
            if (_twoHandWeight > 0f && toLeftPalm.sqrMagnitude > MIN_HAND_GAP_SQR)
            {
                Vector3 axis = baseRotation * _supportAxes[_shownSlot];
                Quaternion aligned = Quaternion.FromToRotation(axis, toLeftPalm) * baseRotation;
                rotation = Quaternion.Slerp(baseRotation, aligned, _twoHandWeight);
            }

            _models[_shownModel].transform.SetPositionAndRotation(palm - rotation * _gripPoints[_shownSlot], rotation);
        }

        private int FindWeaponSlot(byte weaponId)
        {
            for (int slotIndex = 0; slotIndex < _weaponIds.Length; slotIndex++)
            {
                if (_weaponIds[slotIndex] == weaponId)
                {
                    return slotIndex;
                }
            }

            return -1;
        }
    }
}
