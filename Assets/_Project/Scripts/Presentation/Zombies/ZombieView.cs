// 좀비 하나의 겉모습: 동기화된 Variant로 모델 하나를 고르고, 상태와 이동 속도를 Animator에 넣고, 피격 때 점멸한다.
// 점멸은 MaterialPropertyBlock 대신 공유 점멸 머티리얼로 잠깐 바꿔서 SRP Batcher 배칭을 유지한다.

using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using ZD.Gameplay.Zombies;

namespace ZD.Presentation.Zombies
{
    public sealed class ZombieView : MonoBehaviour
    {
        private const float FLASH_SECONDS = 0.1f;
        private const int NO_HEALTH_YET = int.MaxValue;

        private static readonly int StateId = Animator.StringToHash("State");
        private static readonly int SpeedId = Animator.StringToHash("Speed");

        [SerializeField] private Animator[] _variants;
        [SerializeField] private Material _flashMaterial;

        private readonly CompositeDisposable _disposables = new();
        private IZombieDisplay _display;
        private Animator _animator;
        private Renderer[] _renderers = Array.Empty<Renderer>();
        private Material[][] _normalMaterials = Array.Empty<Material[]>();
        private Material[][] _flashMaterials = Array.Empty<Material[]>();
        private int _lastHealth = NO_HEALTH_YET;

        // 피격마다 버전을 올린다. 늦게 끝난 이전 점멸은 머티리얼을 되돌리지 않는다.
        private int _flashVersion;

        private void Awake()
        {
            _display = GetComponent<IZombieDisplay>();
            for (int variantIndex = 0; variantIndex < _variants.Length; variantIndex++)
            {
                _variants[variantIndex].gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            _display.DisplayVariant.Subscribe(SelectVariant).AddTo(_disposables);
            _display.DisplayState.Subscribe(ApplyState).AddTo(_disposables);
            _display.DisplaySpeed.Subscribe(ApplySpeed).AddTo(_disposables);
            _display.DisplayHealth.Subscribe(OnHealthChanged).AddTo(_disposables);
        }

        private void OnDestroy() => _disposables.Dispose();

        private void SelectVariant(int variant)
        {
            if (variant < 0 || _variants.Length == 0)
            {
                return;
            }

            int selected = variant % _variants.Length;
            for (int variantIndex = 0; variantIndex < _variants.Length; variantIndex++)
            {
                _variants[variantIndex].gameObject.SetActive(variantIndex == selected);
            }

            _animator = _variants[selected];
            CacheMaterials(_animator.GetComponentsInChildren<Renderer>(true));
            ApplyState(_display.DisplayState.CurrentValue);
            ApplySpeed(_display.DisplaySpeed.CurrentValue);
        }

        private void CacheMaterials(Renderer[] renderers)
        {
            _renderers = renderers;
            _normalMaterials = new Material[renderers.Length][];
            _flashMaterials = new Material[renderers.Length][];
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Material[] normal = renderers[rendererIndex].sharedMaterials;
                var flash = new Material[normal.Length];
                for (int slotIndex = 0; slotIndex < flash.Length; slotIndex++)
                {
                    flash[slotIndex] = _flashMaterial;
                }

                _normalMaterials[rendererIndex] = normal;
                _flashMaterials[rendererIndex] = flash;
            }
        }

        private void ApplyState(ZombieStateId state)
        {
            if (_animator != null)
            {
                _animator.SetInteger(StateId, (int)state);
            }
        }

        private void ApplySpeed(float speed)
        {
            if (_animator != null)
            {
                _animator.SetFloat(SpeedId, speed);
            }
        }

        private void OnHealthChanged(int health)
        {
            bool tookDamage = _lastHealth != NO_HEALTH_YET && health < _lastHealth;
            _lastHealth = health;
            if (tookDamage)
            {
                FlashAsync(++_flashVersion).Forget();
            }
        }

        private async UniTaskVoid FlashAsync(int version)
        {
            SetMaterials(_flashMaterials);
            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(FLASH_SECONDS), cancellationToken: destroyCancellationToken)
                .SuppressCancellationThrow();
            if (!canceled && version == _flashVersion)
            {
                SetMaterials(_normalMaterials);
            }
        }

        private void SetMaterials(Material[][] materials)
        {
            for (int rendererIndex = 0; rendererIndex < _renderers.Length; rendererIndex++)
            {
                _renderers[rendererIndex].sharedMaterials = materials[rendererIndex];
            }
        }
    }
}
