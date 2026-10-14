// 대기 단계.
// 방을 만든 뒤 동료가 들어오기를 기다린다.
// 타이머도 웨이브도 없고, 호스트가 시작하면 GameFlowSystem이 준비 단계로 넘긴다.

namespace ZD.Gameplay.GameFlow
{
    public sealed class WaitingState : IGameFlowState
    {
        public void Enter(IGameFlowModel model)
        {
            model.PhaseEndTick = 0;
        }

        public GamePhase Tick(IGameFlowModel model) => GamePhase.Waiting;
    }
}
