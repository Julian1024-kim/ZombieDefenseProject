// 로컬 플레이어의 체력, Point, 탄약, 보급품 상자 안내를 표시한다.
// 자주 바뀌는 HUD 캔버스에 있다.

using R3;
using TMPro;
using UnityEngine;
using VContainer;
using ZD.Gameplay.Economy;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Hud;

namespace ZD.Presentation.Hud
{
    public sealed class PlayerHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private TMP_Text _pointsText;
        [SerializeField] private TMP_Text _ammoText;
        [SerializeField] private TMP_Text _promptText;
        [SerializeField] private GameObject _downedBanner;

        private readonly CompositeDisposable _disposables = new();
        private LocalPlayerHudModel _model;
        private ShopUiModel _shopUi;
        private MatchHudModel _match;

        [Inject]
        public void Construct(LocalPlayerHudModel model, ShopUiModel shopUi, MatchHudModel match)
        {
            _model = model;
            _shopUi = shopUi;
            _match = match;
        }

        private void Start()
        {
            _model.Health
                .Subscribe(health => _healthText.SetText("HP {0}", health))
                .AddTo(_disposables);

            // 게임 오버 화면과 겹치지 않도록, 쓰러짐 안내는 게임 오버가 아닐 때만 보인다.
            Observable.Merge(_model.Health.AsUnitObservable(), _match.Phase.AsUnitObservable())
                .Subscribe(_ => _downedBanner.SetActive(_model.Health.Value <= 0 && _match.Phase.Value != GamePhase.GameOver))
                .AddTo(_disposables);

            _model.Points
                .Subscribe(points => _pointsText.SetText("{0} P", points))
                .AddTo(_disposables);

            Observable.Merge(
                    _model.AmmoInMagazine.AsUnitObservable(),
                    _model.ReserveAmmo.AsUnitObservable(),
                    _model.IsReloading.AsUnitObservable(),
                    _model.UsesAmmo.AsUnitObservable(),
                    _model.IsHealing.AsUnitObservable())
                .Subscribe(_ => RefreshAmmo())
                .AddTo(_disposables);

            Observable.Merge(
                    _shopUi.IsNearSupplyBox.AsUnitObservable(),
                    _shopUi.IsOpen.AsUnitObservable(),
                    _model.CanReviveNearby.AsUnitObservable(),
                    _model.ReviveProgress.AsUnitObservable())
                .Subscribe(_ => RefreshPrompt())
                .AddTo(_disposables);
        }

        private void OnDestroy() => _disposables.Dispose();

        private void RefreshPrompt()
        {
            if (_model.ReviveProgress.Value >= 0f)
            {
                _promptText.SetText(string.Empty);
            }
            else if (_model.CanReviveNearby.Value)
            {
                _promptText.SetText("[F] HOLD TO REVIVE");
            }
            else if (_shopUi.IsNearSupplyBox.Value && !_shopUi.IsOpen.Value)
            {
                _promptText.SetText("[E] SUPPLY BOX");
            }
            else
            {
                _promptText.SetText(string.Empty);
            }
        }

        private void RefreshAmmo()
        {
            if (_model.IsHealing.Value)
            {
                _ammoText.SetText("HEALING...");
            }
            else if (!_model.UsesAmmo.Value)
            {
                _ammoText.SetText(string.Empty);
            }
            else if (_model.IsReloading.Value)
            {
                _ammoText.SetText("RELOADING");
            }
            else
            {
                _ammoText.SetText("{0} / {1}", _model.AmmoInMagazine.Value, _model.ReserveAmmo.Value);
            }
        }
    }
}
