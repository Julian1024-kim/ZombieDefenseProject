// 게임 오버 상태.
// 들어올 때 확정된 결과의 클리어한 웨이브 수를 담아 알린다(결과는 GameFlowSystem이 들어오기 직전에 확정).

using MessagePipe;
using ZD.Core.Messages;

namespace ZD.Gameplay.GameFlow
{
    public sealed class GameOverState : IGameFlowState
    {
        private readonly IPublisher<GameOverMessage> _gameOverPublisher;

        public GameOverState(IPublisher<GameOverMessage> gameOverPublisher)
        {
            _gameOverPublisher = gameOverPublisher;
        }

        public void Enter(IGameFlowModel model)
        {
            model.PhaseEndTick = 0;
            _gameOverPublisher.Publish(new GameOverMessage(model.Result.WavesCleared));
        }

        public GamePhase Tick(IGameFlowModel model) => GamePhase.GameOver;
    }
}
