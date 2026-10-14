// 준비 단계.
// 정해진 시간이 지나면 웨이브로 넘어간다.

using ZD.Core;
using ZD.Gameplay.Waves;

namespace ZD.Gameplay.GameFlow
{
    public sealed class PreparationState : IGameFlowState
    {
        private readonly INetworkClock _clock;
        private readonly WaveProfile _profile;

        public PreparationState(INetworkClock clock, WaveProfile profile)
        {
            _clock = clock;
            _profile = profile;
        }

        public void Enter(IGameFlowModel model)
        {
            model.PhaseEndTick = _clock.Tick + _clock.SecondsToTicks(_profile.PreparationSeconds);
        }

        public GamePhase Tick(IGameFlowModel model)
        {
            return _clock.Tick >= model.PhaseEndTick ? GamePhase.Wave : GamePhase.Preparation;
        }
    }
}
