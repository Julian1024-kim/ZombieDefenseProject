// 게임 오버 창의 결과 부분: 팀 점수, 클리어한 웨이브와 버틴 날, 이 모드 최고 점수(새 기록 표시), 플레이어별 처치·헤드샷·부활·획득 Point 표.
// 창을 보이고 숨기는 것과 진 이유는 MatchHudView가 맡는다.
// 결과가 확정될 때와 최고 점수를 불러왔을 때만 다시 그린다(매 프레임 아님).

using System.Text;
using R3;
using TMPro;
using UnityEngine;
using VContainer;
using ZD.Gameplay.Hud;
using ZD.Gameplay.Stats;

namespace ZD.Presentation.Hud
{
    public sealed class ResultView : MonoBehaviour
    {
        // 표 열 위치(텍스트 폭 기준 %). 고정폭 글꼴이 아니어서 탭 대신 위치 태그로 맞춘다.
        private const string HEADER = "<b>PLAYER<pos=32%>KILLS<pos=46%>HEADSHOTS<pos=66%>REVIVES<pos=82%>POINTS</b>";

        [SerializeField] private TMP_Text _summaryText;
        [SerializeField] private TMP_Text _tableText;

        private readonly CompositeDisposable _disposables = new();
        private readonly StringBuilder _builder = new(512);
        private ResultHudModel _model;

        [Inject]
        public void Construct(ResultHudModel model)
        {
            _model = model;
        }

        private void Start()
        {
            Observable.Merge(
                    _model.Version.AsUnitObservable(),
                    _model.BestScore.AsUnitObservable(),
                    _model.IsNewBest.AsUnitObservable())
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);
        }

        private void OnDestroy() => _disposables.Dispose();

        private void Refresh()
        {
            int waves = _model.WavesCleared.Value;
            _builder.Clear();
            _builder.Append("TEAM SCORE  <b>").Append(_model.TeamScore.Value).Append("</b>\n");
            _builder.Append("WAVES CLEARED ").Append(waves).Append("    DAYS SURVIVED ").Append(ScoreRules.DaysSurvived(waves)).Append('\n');
            _builder.Append("BEST ").Append(_model.BestScore.Value);
            if (_model.IsNewBest.Value)
            {
                _builder.Append("  <color=#F2BF33>NEW BEST!</color>");
            }

            _summaryText.SetText(_builder);

            _builder.Clear();
            _builder.Append(HEADER);
            for (int rowIndex = 0; rowIndex < _model.RowCount; rowIndex++)
            {
                PlayerResult row = _model.Rows[rowIndex];
                // 닉네임이 없으면(아직 못 받았으면) 접속 순서대로 P1, P2, ...로 보인다.
                _builder.Append('\n');
                if (row.Name.Length > 0)
                {
                    _builder.Append(row.Name);
                }
                else
                {
                    _builder.Append('P').Append(rowIndex + 1);
                }

                _builder
                    .Append("<pos=32%>").Append(row.Kills)
                    .Append("<pos=46%>").Append(row.Headshots)
                    .Append("<pos=66%>").Append(row.Revives)
                    .Append("<pos=82%>").Append(row.PointsEarned);
            }

            _tableText.SetText(_builder);
        }
    }
}
