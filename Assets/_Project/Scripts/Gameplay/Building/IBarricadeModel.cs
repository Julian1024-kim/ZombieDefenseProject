// 방어막 하나의 모델 계약.
// 네트워크 계층이 [Networked] 값으로 구현한다.
// 설치자 정보(시작 위치, 시작 체력)는 설치 중 취소 판정에, 남은 횟수와 쉬는 시간은 곰덫에 쓴다.

using UnityEngine;
using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Building
{
    public interface IBarricadeModel : IDamageable
    {
        BarricadeDefinition Definition { get; }
        Vector3 Position { get; }
        Quaternion Rotation { get; }
        BarricadeStateId State { get; set; }
        int InstallEndTick { get; set; }
        int BuilderId { get; set; }
        Vector3 BuilderStartPosition { get; set; }
        int BuilderStartHealth { get; set; }
        int UsesLeft { get; set; }
        int BusyUntilTick { get; set; }
    }
}
