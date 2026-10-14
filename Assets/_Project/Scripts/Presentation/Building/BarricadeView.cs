// 방어막 하나의 겉모습.
// 설치 중이면 반투명 설치 머티리얼(공유)로, 완성되면 원래 머티리얼로 보인다.
// 맞으면 점멸하고, 지뢰가 터지면 폭발 구체를 잠깐 키웠다가 숨기며, 끝나면(부서짐·취소) 몸을 숨긴다.

using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using ZD.Gameplay.Building;

namespace ZD.Presentation.Building
{
    public sealed class BarricadeView : MonoBehaviour
    {
        private const float FLASH_SECONDS = 0.1f;
        private const float EXPLOSION_SECONDS = 0.35f;
        private const int NO_HEALTH_YET = int.MaxValue;

        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private Material _installingMaterial;
        [SerializeField] private Material _flashMaterial;
        [SerializeField] private Transform _explosion;
        [SerializeField] private float _explosionRadius = 3f;

        private readonly CompositeDisposable _disposables = new();
        private IBarricadeDisplay _display;
        private Material[] _normalMaterials;
        private int _lastHealth = NO_HEALTH_YET;
        private int _flashVersion;

        private void Awake()
        {
            _display = GetComponent<IBarricadeDisplay>();
            _normalMaterials = new Material[_renderers.Length];
            for (int rendererIndex = 0; rendererIndex < _renderers.Length; rendererIndex++)
            {
                _normalMaterials[rendererIndex] = _renderers[rendererIndex].sharedMaterial;
            }

            if (_explosion != null)
            {
                _explosion.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            _display.DisplayState.Subscribe(ApplyState).AddTo(_disposables);
            _display.DisplayHealth.Subscribe(OnHealthChanged).AddTo(_disposables);
        }

        private void OnDestroy() => _disposables.Dispose();

        private void ApplyState(BarricadeStateId state)
        {
            switch (state)
            {
                case BarricadeStateId.Installing:
                    SetAllMaterials(_installingMaterial);
                    break;
                case BarricadeStateId.Active:
                    RestoreMaterials();
                    break;
                default:
                    SetBodyVisible(false);
                    if (state == BarricadeStateId.Destroyed && _display.Definition.Kind == BarricadeKind.Mine)
                    {
                        ExplodeAsync().Forget();
                    }

                    break;
            }
        }

        private void OnHealthChanged(int health)
        {
            bool tookDamage = _lastHealth != NO_HEALTH_YET && health < _lastHealth && health > 0;
            _lastHealth = health;
            if (tookDamage && _display.DisplayState.CurrentValue == BarricadeStateId.Active)
            {
                FlashAsync(++_flashVersion).Forget();
            }
        }

        private async UniTaskVoid FlashAsync(int version)
        {
            SetAllMaterials(_flashMaterial);
            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(FLASH_SECONDS), cancellationToken: destroyCancellationToken)
                .SuppressCancellationThrow();
            if (!canceled && version == _flashVersion && _display.DisplayState.CurrentValue == BarricadeStateId.Active)
            {
                RestoreMaterials();
            }
        }

        private async UniTaskVoid ExplodeAsync()
        {
            if (_explosion == null)
            {
                return;
            }

            _explosion.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < EXPLOSION_SECONDS)
            {
                float scale = Mathf.Lerp(0.3f, _explosionRadius * 2f, elapsed / EXPLOSION_SECONDS);
                _explosion.localScale = Vector3.one * scale;
                bool canceled = await UniTask.Yield(PlayerLoopTiming.Update, destroyCancellationToken).SuppressCancellationThrow();
                if (canceled)
                {
                    return;
                }

                elapsed += Time.deltaTime;
            }

            _explosion.gameObject.SetActive(false);
        }

        private void SetBodyVisible(bool isVisible)
        {
            for (int rendererIndex = 0; rendererIndex < _renderers.Length; rendererIndex++)
            {
                _renderers[rendererIndex].enabled = isVisible;
            }
        }

        private void SetAllMaterials(Material material)
        {
            for (int rendererIndex = 0; rendererIndex < _renderers.Length; rendererIndex++)
            {
                _renderers[rendererIndex].sharedMaterial = material;
            }
        }

        private void RestoreMaterials()
        {
            for (int rendererIndex = 0; rendererIndex < _renderers.Length; rendererIndex++)
            {
                _renderers[rendererIndex].sharedMaterial = _normalMaterials[rendererIndex];
            }
        }
    }
}
