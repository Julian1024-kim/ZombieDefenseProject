// 체력을 가진 모든 대상의 모델 계약.
// 네트워크 계층이 [Networked] 값으로 구현한다.
// 체력이 0이면 플레이어는 쓰러진 상태, 좀비는 죽은 상태다.

namespace ZD.Gameplay.Combat
{
    public interface IDamageable
    {
        int EntityId { get; }
        DamageTargetKind Kind { get; }
        int Health { get; set; }
        int MaxHealth { get; }
    }
}
