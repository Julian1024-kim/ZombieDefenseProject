// 모드 정의를 모아 둔 한 곳.
// 모드마다 패배 조건 묶음(Composite)과 좀비 목표 선정 방식(Strategy)을 고른다.
// 생존 = 전원 쓰러짐 / 가장 가까운 플레이어, 거점 방어 = 전원 쓰러짐 또는 거점 파괴 / 거점 우선.
// 모드를 추가하면 여기 배열에 한 줄씩만 더한다.

using ZD.Gameplay.Zombies;

namespace ZD.Gameplay.GameFlow
{
    public sealed class GameModeSystem : IGameModeRules
    {
        private readonly IDefeatCondition[] _defeatByMode;
        private readonly ITargetSelector[] _targetByMode;

        public GameModeSystem(
            AllPlayersDownedCondition allPlayersDowned,
            BaseDestroyedCondition baseDestroyed,
            NearestPlayerTargetSelector nearestPlayer,
            BaseFirstTargetSelector baseFirst)
        {
            _defeatByMode = new IDefeatCondition[]
            {
                allPlayersDowned,
                new AnyDefeatCondition(allPlayersDowned, baseDestroyed),
            };
            _targetByMode = new ITargetSelector[]
            {
                nearestPlayer,
                baseFirst,
            };
        }

        public bool IsDefeated(GameModeId mode) => _defeatByMode[(int)mode].IsMet();

        public ITargetSelector GetTargetSelector(GameModeId mode) => _targetByMode[(int)mode];
    }
}
