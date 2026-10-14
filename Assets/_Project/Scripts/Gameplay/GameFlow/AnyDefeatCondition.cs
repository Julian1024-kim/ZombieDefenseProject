// 패배 조건 여러 개를 하나로 묶는다(Composite).
// 하나라도 충족되면 충족이다.
// 모드 하나가 이 묶음 하나다.

namespace ZD.Gameplay.GameFlow
{
    public sealed class AnyDefeatCondition : IDefeatCondition
    {
        private readonly IDefeatCondition[] _conditions;

        public AnyDefeatCondition(params IDefeatCondition[] conditions)
        {
            _conditions = conditions;
        }

        public bool IsMet()
        {
            for (int conditionIndex = 0; conditionIndex < _conditions.Length; conditionIndex++)
            {
                if (_conditions[conditionIndex].IsMet())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
