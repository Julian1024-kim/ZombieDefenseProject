// 좀비 행동 상태 하나.
// 인스턴스를 모든 좀비가 공유하므로 좀비별 데이터는 모두 모델에 있다.
// Tick은 좀비가 다음에 있어야 할 상태를 돌려주고, 전환은 AI 시스템이 한다.

namespace ZD.Gameplay.Zombies
{
    public interface IZombieAIState
    {
        void Enter(in ZombieTickContext context);
        ZombieStateId Tick(in ZombieTickContext context);
    }
}
