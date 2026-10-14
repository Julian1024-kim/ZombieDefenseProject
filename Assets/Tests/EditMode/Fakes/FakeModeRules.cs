// 테스트용 모드 규칙.
// 모드와 관계없이 주어진 패배 조건 하나를 보고, 마지막으로 물어본 모드를 기억한다.

using ZD.Gameplay.GameFlow;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeModeRules : IGameModeRules
    {
        private readonly IDefeatCondition _condition;

        public FakeModeRules(IDefeatCondition condition)
        {
            _condition = condition;
        }

        public GameModeId LastAskedMode { get; private set; }

        public bool IsDefeated(GameModeId mode)
        {
            LastAskedMode = mode;
            return _condition.IsMet();
        }
    }
}
