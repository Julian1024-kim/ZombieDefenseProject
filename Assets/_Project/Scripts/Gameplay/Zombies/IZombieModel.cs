// 좀비 하나의 모델 계약.
// 네트워크 계층이 압축된 [Networked] 값으로 구현한다.

using UnityEngine;
using ZD.Gameplay.Combat;

namespace ZD.Gameplay.Zombies
{
    public interface IZombieModel : IDamageable
    {
        ZombieDefinition Definition { get; }
        Vector3 Position { get; }
        ZombieStateId State { get; set; }
        int NextAttackTick { get; set; }

        int HeldUntilTick { get; set; }
    }
}
