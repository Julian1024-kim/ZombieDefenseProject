// 규칙 묶음(Composite).
// 순서대로 검사해서 처음 실패한 이유를 돌려준다.
// 비싼 물리 검사(겹침)는 마지막에 둔다.

namespace ZD.Gameplay.Building
{
    public sealed class CompositePlacementRule : IPlacementRule
    {
        private readonly IPlacementRule[] _rules;

        public CompositePlacementRule(params IPlacementRule[] rules)
        {
            _rules = rules;
        }

        public PlacementResult Check(in PlacementRequest request, IPlacementWorld world)
        {
            for (int ruleIndex = 0; ruleIndex < _rules.Length; ruleIndex++)
            {
                PlacementResult result = _rules[ruleIndex].Check(request, world);
                if (result != PlacementResult.Valid)
                {
                    return result;
                }
            }

            return PlacementResult.Valid;
        }
    }
}
