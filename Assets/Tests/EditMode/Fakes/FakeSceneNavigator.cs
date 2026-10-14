// 게임 씬에 들어간 횟수와 마지막으로 돌아온 이유만 기억하는 테스트용 씬 전환기.

using Cysharp.Threading.Tasks;
using ZD.Core;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeSceneNavigator : ISceneNavigator
    {
        public int EnterCount { get; private set; }
        public MenuReturnReason? LastReturnReason { get; private set; }

        public UniTask EnterGameAsync()
        {
            EnterCount++;
            return UniTask.CompletedTask;
        }

        public UniTask ReturnToMenuAsync(MenuReturnReason reason)
        {
            LastReturnReason = reason;
            return UniTask.CompletedTask;
        }
    }
}
