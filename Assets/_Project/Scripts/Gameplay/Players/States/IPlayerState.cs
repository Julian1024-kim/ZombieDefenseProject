// 플레이어 행동 상태 하나.
// 상태끼리 직접 넘어가지 않고, PlayerStateSystem이 매 틱 다음 상태를 정해 Exit/Enter를 부른다.
// 상태 객체는 미리 만들어 두고 다시 쓴다(전환에 할당 없음).
// 변하는 값은 모두 플레이어 모델(복제 값)에 있다.

namespace ZD.Gameplay.Players
{
    public interface IPlayerState
    {
        void Enter(in PlayerTickContext context);

        bool Tick(in PlayerTickContext context);

        void Exit(in PlayerTickContext context);
    }
}
