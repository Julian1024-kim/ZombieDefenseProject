// 호스트 전용 매치 상태 머신: 모드의 패배 조건이 충족될 때까지 준비 → 웨이브 → 준비를 반복한다.
// 게임 오버 후에는 웨이브 1 준비 단계로 재시작을 요청할 수 있다(누가 요청할 수 있는지는 네트워크 쪽이 정한다: 호스트만).
// 방을 만들어 시작하면 먼저 대기 단계에서 동료를 기다리고, 호스트가 시작을 누르면 준비 단계로 넘어간다.

using MessagePipe;
using ZD.Core;
using ZD.Core.Messages;
using ZD.Gameplay.Stats;
using ZD.Gameplay.Waves;

namespace ZD.Gameplay.GameFlow
{
    public sealed class GameFlowSystem
    {
        private readonly WaveSystem _waveSystem;
        private readonly IGameModeRules _modeRules;
        private readonly MatchResultSystem _results;
        private readonly IPublisher<MatchRestartedMessage> _matchRestartedPublisher;
        private readonly IGameFlowState[] _states;
        private bool _isRestartRequested;
        private bool _isStartRequested;

        public GameFlowSystem(
            INetworkClock clock,
            WaveProfile profile,
            WaveSystem waveSystem,
            IGameModeRules modeRules,
            MatchResultSystem results,
            IPublisher<WaveStartedMessage> waveStartedPublisher,
            IPublisher<WaveClearedMessage> waveClearedPublisher,
            IPublisher<GameOverMessage> gameOverPublisher,
            IPublisher<MatchRestartedMessage> matchRestartedPublisher)
        {
            _waveSystem = waveSystem;
            _modeRules = modeRules;
            _results = results;
            _matchRestartedPublisher = matchRestartedPublisher;
            _states = new IGameFlowState[]
            {
                new PreparationState(clock, profile),
                new WaveState(waveSystem, waveStartedPublisher, waveClearedPublisher),
                new GameOverState(gameOverPublisher),
                new WaitingState(),
            };
        }

        public void Begin(IGameFlowModel model, bool waitForStart = false)
        {
            _isRestartRequested = false;
            _isStartRequested = false;
            model.WaveNumber = 0;
            ChangePhase(model, waitForStart ? GamePhase.Waiting : GamePhase.Preparation);
        }

        public void RequestStart()
        {
            _isStartRequested = true;
        }

        public void RequestRestart()
        {
            _isRestartRequested = true;
        }

        public void Tick(IGameFlowModel model)
        {
            if (model.Phase == GamePhase.Waiting)
            {
                if (_isStartRequested)
                {
                    _isStartRequested = false;
                    DevLog.Info("Match starting");
                    ChangePhase(model, GamePhase.Preparation);
                }

                return;
            }

            _isStartRequested = false;
            if (model.Phase == GamePhase.GameOver)
            {
                if (_isRestartRequested)
                {
                    DevLog.Info("Match restarting");
                    _waveSystem.ResetMatch();
                    _matchRestartedPublisher.Publish(new MatchRestartedMessage());
                    Begin(model);
                }

                return;
            }

            _isRestartRequested = false;
            if (_modeRules.IsDefeated(model.Mode))
            {
                _results.Record(model.Result, CountClearedWaves(model));
                ChangePhase(model, GamePhase.GameOver);
                return;
            }

            GamePhase next = _states[(int)model.Phase].Tick(model);
            if (next != model.Phase)
            {
                ChangePhase(model, next);
            }
        }

        private static int CountClearedWaves(IGameFlowModel model)
        {
            return model.Phase == GamePhase.Wave ? model.WaveNumber - 1 : model.WaveNumber;
        }

        private void ChangePhase(IGameFlowModel model, GamePhase next)
        {
            model.Phase = next;
            _states[(int)next].Enter(model);
        }
    }
}
