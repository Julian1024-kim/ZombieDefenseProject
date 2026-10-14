// 부활시키는 쪽 플레이어의 부활 진행 상태 모델 계약.
// 네트워크 계층이 [Networked] 값으로 구현한다.
// 쓰러진 쪽은 자신을 대상으로 하는 동료를 명단에서 찾아 진행도를 본다.

namespace ZD.Gameplay.Players
{
    public interface IReviveModel
    {
        int ReviveTargetId { get; set; }

        int ReviveEndTick { get; set; }
    }
}
