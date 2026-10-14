// 각 피어(로컬)에서 모드별 최고 점수를 관리한다.
// 이번 점수가 그 모드의 최고 점수보다 높으면 새 기록으로 저장한다.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ZD.Gameplay.GameFlow;

namespace ZD.Gameplay.Stats
{
    public sealed class HighScoreSystem
    {
        private readonly IHighScoreRepository _repository;

        public HighScoreSystem(IHighScoreRepository repository)
        {
            _repository = repository;
        }

        public async UniTask<HighScoreResult> SubmitAsync(GameModeId mode, int score, CancellationToken token)
        {
            HighScoreSaveData data = await _repository.LoadAsync(token);
            int modeIndex = (int)mode;
            if (data.BestByMode.Length <= modeIndex)
            {
                int[] grown = new int[modeIndex + 1];
                Array.Copy(data.BestByMode, grown, data.BestByMode.Length);
                data.BestByMode = grown;
            }

            int previousBest = data.BestByMode[modeIndex];
            if (score <= previousBest)
            {
                return new HighScoreResult(previousBest, false);
            }

            data.BestByMode[modeIndex] = score;
            await _repository.SaveAsync(data, token);
            return new HighScoreResult(score, true);
        }
    }
}
