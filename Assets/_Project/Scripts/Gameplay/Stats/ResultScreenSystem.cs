// 각 피어(로컬)에서 확정된 매치 결과를 결과 화면 모델에 옮기고, 이 모드의 최고 점수와 비교해 저장한다.
// 같은 결과(Version)는 한 번만 처리한다.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Hud;

namespace ZD.Gameplay.Stats
{
    public sealed class ResultScreenSystem : IDisposable
    {
        private readonly ResultHudModel _hud;
        private readonly HighScoreSystem _highScores;
        private readonly CancellationTokenSource _cts = new();
        private int _shownVersion;

        public ResultScreenSystem(ResultHudModel hud, HighScoreSystem highScores)
        {
            _hud = hud;
            _highScores = highScores;
        }

        public void Show(IMatchResultModel result, GameModeId mode)
        {
            if (result.Version == 0 || result.Version == _shownVersion)
            {
                return;
            }

            _shownVersion = result.Version;
            _hud.RowCount = result.PlayerCount;
            for (int rowIndex = 0; rowIndex < result.PlayerCount; rowIndex++)
            {
                _hud.Rows[rowIndex] = result.GetPlayer(rowIndex);
            }

            _hud.TeamScore.Value = result.TeamScore;
            _hud.WavesCleared.Value = result.WavesCleared;
            _hud.IsBaseDestroyed.Value = result.IsBaseDestroyed;
            _hud.IsNewBest.Value = false;
            _hud.Version.Value = result.Version;
            SubmitAsync(mode, result.TeamScore).Forget();
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private async UniTaskVoid SubmitAsync(GameModeId mode, int score)
        {
            HighScoreResult best = await _highScores.SubmitAsync(mode, score, _cts.Token);
            _hud.BestScore.Value = best.Best;
            _hud.IsNewBest.Value = best.IsNewBest;
        }
    }
}
