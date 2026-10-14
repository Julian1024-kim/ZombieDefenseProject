// 플레이어 행동 상태.
// 값은 PlayerStateSystem의 상태 배열 순서와 같다(복제되는 바이트로 저장).
// 활동(이동·사격·슬롯), 회복 중, 동료 부활 중, 방어막 설치 중, 쓰러짐, 정지(게임 오버).

namespace ZD.Gameplay.Players
{
    public enum PlayerStateId : byte
    {
        Active = 0,
        Healing = 1,
        Reviving = 2,
        Building = 3,
        Downed = 4,
        Frozen = 5,
    }
}
