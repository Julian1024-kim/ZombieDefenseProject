// Tick은 매치가 다음에 있어야 할 단계를 돌려주고, 실제 전환은 GameFlowSystem이 한다.

namespace ZD.Gameplay.GameFlow
{
    public interface IGameFlowState
    {
        void Enter(IGameFlowModel model);
        GamePhase Tick(IGameFlowModel model);
    }
}
