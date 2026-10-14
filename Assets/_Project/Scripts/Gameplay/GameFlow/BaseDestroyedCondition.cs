// 거점 체력이 0이 되면 충족되는 패배 조건(거점 방어 모드).

using ZD.Gameplay.Defense;

namespace ZD.Gameplay.GameFlow
{
    public sealed class BaseDestroyedCondition : IDefeatCondition
    {
        private readonly BaseRosterModel _bases;

        public BaseDestroyedCondition(BaseRosterModel bases)
        {
            _bases = bases;
        }

        public bool IsMet()
        {
            IBaseModel current = _bases.Current;
            return current != null && current.Health <= 0;
        }
    }
}
