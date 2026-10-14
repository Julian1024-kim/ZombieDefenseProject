// 점수를 낸 결과: 그 모드의 최고 점수(이번에 갱신됐으면 이번 점수)와 새 기록인지.

namespace ZD.Gameplay.Stats
{
    public readonly struct HighScoreResult
    {
        public readonly int Best;
        public readonly bool IsNewBest;

        public HighScoreResult(int best, bool isNewBest)
        {
            Best = best;
            IsNewBest = isNewBest;
        }
    }
}
