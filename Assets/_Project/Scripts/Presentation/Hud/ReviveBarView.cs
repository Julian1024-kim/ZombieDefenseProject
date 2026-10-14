// 진행 막대.
// 내가 동료를 부활시킬 때, 동료가 나를 부활시킬 때, 내가 방어막을 설치할 때 화면 가운데 아래에 나타난다.

using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using ZD.Gameplay.Hud;

namespace ZD.Presentation.Hud
{
    public sealed class ReviveBarView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _fill;
        [SerializeField] private TMP_Text _label;

        private readonly CompositeDisposable _disposables = new();
        private LocalPlayerHudModel _model;

        [Inject]
        public void Construct(LocalPlayerHudModel model)
        {
            _model = model;
        }

        private void Start()
        {
            SetVisible(false);

            Observable.Merge(_model.ReviveProgress.AsUnitObservable(), _model.BeingRevivedProgress.AsUnitObservable(), _model.BuildProgress.AsUnitObservable())
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);
        }

        private void OnDestroy() => _disposables.Dispose();

        private void Refresh()
        {
            float reviving = _model.ReviveProgress.Value;
            float beingRevived = _model.BeingRevivedProgress.Value;
            float building = _model.BuildProgress.Value;
            if (reviving >= 0f)
            {
                Show("REVIVING", reviving);
            }
            else if (beingRevived >= 0f)
            {
                Show("BEING REVIVED", beingRevived);
            }
            else if (building >= 0f)
            {
                Show("BUILDING", building);
            }
            else
            {
                SetVisible(false);
            }
        }

        private void Show(string label, float progress)
        {
            _label.SetText(label);
            _fill.fillAmount = progress;
            SetVisible(true);
        }

        private void SetVisible(bool isVisible)
        {
            _group.alpha = isVisible ? 1f : 0f;
        }
    }
}
