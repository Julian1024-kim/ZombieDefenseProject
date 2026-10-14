// 메모리에만 두는 테스트용 최고 점수 저장소.
// 저장 횟수를 센다.

using System.Threading;
using Cysharp.Threading.Tasks;
using ZD.Gameplay.Stats;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeHighScoreRepository : IHighScoreRepository
    {
        public HighScoreSaveData Saved { get; private set; } = new();
        public int SaveCount { get; private set; }

        public UniTask<HighScoreSaveData> LoadAsync(CancellationToken token) => UniTask.FromResult(Saved);

        public UniTask SaveAsync(HighScoreSaveData data, CancellationToken token)
        {
            Saved = data;
            SaveCount++;
            return UniTask.CompletedTask;
        }
    }
}
