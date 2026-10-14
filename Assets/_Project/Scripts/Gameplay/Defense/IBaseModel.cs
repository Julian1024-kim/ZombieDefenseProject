// 거점(거점 방어 모드에서 지키는 발전기)의 모델 계약.
// 네트워크 계층이 [Networked] 체력으로 구현한다.
// 거점은 회전하지 않는 상자로 본다(바닥 크기 = Definition.Size의 x, z).

using UnityEngine;
using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Defense
{
    public interface IBaseModel : IDamageable
    {
        BaseDefinition Definition { get; }
        Vector3 Position { get; }
    }
}
