// 웨이브 단계.
// 좀비를 생성하고, 모두 처치되면 준비 단계로 돌아간다.

using MessagePipe;
using ZD.Core.Messages;
using ZD.Gameplay.Waves;

namespace ZD.Gameplay.GameFlow
{
    public sealed class WaveState : IGameFlowState
    {
        private readonly WaveSystem _waveSystem;
        private readonly IPublisher<WaveStartedMessage> _waveStartedPublisher;
        private readonly IPublisher<WaveClearedMessage> _waveClearedPublisher;

        public WaveState(
            WaveSystem waveSystem,
            IPublisher<WaveStartedMessage> waveStartedPublisher,
            IPublisher<WaveClearedMessage> waveClearedPublisher)
        {
            _waveSystem = waveSystem;
            _waveStartedPublisher = waveStartedPublisher;
            _waveClearedPublisher = waveClearedPublisher;
        }

        public void Enter(IGameFlowModel model)
        {
            model.WaveNumber++;
            model.PhaseEndTick = 0;
            int zombieCount = _waveSystem.BeginWave(model.WaveNumber);
            _waveStartedPublisher.Publish(new WaveStartedMessage(model.WaveNumber, zombieCount));
        }

        public GamePhase Tick(IGameFlowModel model)
        {
            _waveSystem.Tick();
            if (!_waveSystem.IsCleared)
            {
                return GamePhase.Wave;
            }

            _waveClearedPublisher.Publish(new WaveClearedMessage(model.WaveNumber));
            return GamePhase.Preparation;
        }
    }
}
