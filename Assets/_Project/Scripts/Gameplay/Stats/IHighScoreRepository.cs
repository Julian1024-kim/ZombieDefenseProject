// 최고 점수 저장소.
// 저장 방식(파일, 클라우드, 테스트용 가짜)은 이 계약 뒤에 숨긴다.
// 저장본이 없으면 빈 DTO를 돌려준다.

using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZD.Gameplay.Stats
{
    public interface IHighScoreRepository
    {
        UniTask<HighScoreSaveData> LoadAsync(CancellationToken token);

        UniTask SaveAsync(HighScoreSaveData data, CancellationToken token);
    }
}
