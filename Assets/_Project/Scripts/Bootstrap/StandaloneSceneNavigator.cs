// Game 씬만 바로 플레이할 때(Bootstrap 씬 없음) 쓰는 씬 전환기.
// 돌아갈 시작 화면이 없으므로 세션만 끝난 채로 남는다.

using Cysharp.Threading.Tasks;
using ZD.Core;

namespace ZD.Bootstrap
{
    internal sealed class StandaloneSceneNavigator : ISceneNavigator
    {
        public UniTask EnterGameAsync() => UniTask.CompletedTask;

        public UniTask ReturnToMenuAsync(MenuReturnReason reason)
        {
            DevLog.Info("Bootstrap 씬 없이 실행 중이라 돌아갈 시작 화면이 없습니다.");
            return UniTask.CompletedTask;
        }
    }
}
