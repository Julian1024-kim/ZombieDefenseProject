// 거점 체력 막대(거점 방어 모드에서만 보이고, 게임 오버 창이 뜨면 제목과 겹치지 않게 숨김).
// 체력이 줄면 막대를 잠깐 빨갛게 깜빡여 거점이 맞고 있음을 알린다.

using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Hud;

namespace ZD.Presentation.Hud
{
    public sealed class BaseHudView : MonoBehaviour
    {
        private const float FLASH_SECONDS = 0.25f;

        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _fill;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Color _normalColor = new(0.95f, 0.75f, 0.2f);
        [SerializeField] private Color _hitColor = new(1f, 0.2f, 0.15f);

        private readonly CompositeDisposable _disposables = new();
        private BaseHudModel _model;
        private MatchHudModel _match;
        private int _lastHealth = -1;
        private float _flashLeft;

        [Inject]
        public void Construct(BaseHudModel model, MatchHudModel match)
        {
            _model = model;
            _match = match;
        }

        private void Start()
        {
            Observable.CombineLatest(_model.IsVisible, _match.Phase, (isVisible, phase) => isVisible && phase != GamePhase.GameOver)
                .Subscribe(SetVisible)
                .AddTo(_disposables);
            Observable.Merge(_model.Health.AsUnitObservable(), _model.MaxHealth.AsUnitObservable())
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);
        }

        private void Update()
        {
            if (_flashLeft <= 0f)
            {
                return;
            }

            _flashLeft -= Time.deltaTime;
            _fill.color = Color.Lerp(_normalColor, _hitColor, Mathf.Clamp01(_flashLeft / FLASH_SECONDS));
        }

        private void OnDestroy() => _disposables.Dispose();

        private void Refresh()
        {
            int health = _model.Health.Value;
            int maxHealth = Mathf.Max(1, _model.MaxHealth.Value);
            _fill.fillAmount = (float)health / maxHealth;
            _label.SetText("BASE {0} / {1}", health, maxHealth);
            if (_lastHealth >= 0 && health < _lastHealth)
            {
                _flashLeft = FLASH_SECONDS;
            }

            _lastHealth = health;
        }

        private void SetVisible(bool isVisible)
        {
            _group.alpha = isVisible ? 1f : 0f;
            if (!isVisible)
            {
                _lastHealth = -1;
            }
        }
    }
}
