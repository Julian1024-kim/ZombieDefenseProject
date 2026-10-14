// 설치 모드 안내.
// 설치할 수 있는 방어막과 가격(고른 것 강조), 지금 위치에 설치할 수 없는 이유, 호스트의 거절 이유를 보여준다.
// 문구는 값이 바뀔 때만 다시 만든다.

using System;
using System.Text;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using VContainer;
using ZD.Gameplay.Building;

namespace ZD.Presentation.Building
{
    public sealed class BuildHudView : MonoBehaviour
    {
        private const float REJECTION_SECONDS = 2f;

        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TMP_Text _itemsText;
        [SerializeField] private TMP_Text _statusText;

        private readonly CompositeDisposable _disposables = new();
        private readonly StringBuilder _builder = new();
        private BuildModeModel _model;
        private BuildCatalog _catalog;
        private int _rejectionVersion;

        [Inject]
        public void Construct(BuildModeModel model, BuildCatalog catalog)
        {
            _model = model;
            _catalog = catalog;
        }

        private void Start()
        {
            _group.alpha = 0f;
            _model.IsActive.Subscribe(isActive => _group.alpha = isActive ? 1f : 0f).AddTo(_disposables);
            _model.SelectedIndex.Subscribe(_ => RefreshItems()).AddTo(_disposables);
            _model.PreviewResult.Subscribe(result => _statusText.SetText(Describe(result))).AddTo(_disposables);
            _model.Rejections.Subscribe(OnRejected).AddTo(_disposables);
        }

        private void OnDestroy() => _disposables.Dispose();

        private void RefreshItems()
        {
            _builder.Clear();
            int selected = _model.SelectedIndex.Value;
            for (int itemIndex = 0; itemIndex < _catalog.Count; itemIndex++)
            {
                BarricadeDefinition item = _catalog.Get(itemIndex);
                _builder.Append(itemIndex == selected ? "<color=#FFD24A>> " : "<color=#FFFFFF>  ");
                _builder.Append('[').Append(itemIndex + 1).Append("] ").Append(item.DisplayName).Append("  ").Append(item.Price).Append(" P</color>\n");
            }

            _builder.Append("<size=80%>[Click] PLACE   [V/Esc] EXIT</size>");
            _itemsText.SetText(_builder);
        }

        private void OnRejected(PlacementResult result)
        {
            ShowRejectionAsync(++_rejectionVersion, result).Forget();
        }

        private async UniTaskVoid ShowRejectionAsync(int version, PlacementResult result)
        {
            _group.alpha = 1f;
            _statusText.SetText(string.Concat("<color=#FF6B6B>REJECTED: ", Describe(result), "</color>"));
            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(REJECTION_SECONDS), cancellationToken: destroyCancellationToken)
                .SuppressCancellationThrow();
            if (canceled || version != _rejectionVersion)
            {
                return;
            }

            _group.alpha = _model.IsActive.Value ? 1f : 0f;
            _statusText.SetText(Describe(_model.PreviewResult.Value));
        }

        private static string Describe(PlacementResult result)
        {
            return result switch
            {
                PlacementResult.Valid => "<color=#7CFF7C>READY</color>",
                PlacementResult.NoGround => "NO GROUND",
                PlacementResult.TooFar => "TOO FAR",
                PlacementResult.TooSteep => "TOO STEEP",
                PlacementResult.Blocked => "BLOCKED",
                PlacementResult.NotEnoughPoints => "NOT ENOUGH POINTS",
                _ => "UNAVAILABLE",
            };
        }
    }
}
