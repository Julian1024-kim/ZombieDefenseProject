// 피격 판정 계약.
// 네트워크 계층이 Fusion Lag Compensation으로 구현해, 쏜 사람이 보던 시점으로 히트박스를 되돌려 판정한다.
// 상태 권한이 없는 쪽에서 호출하면 아무것도 맞지 않는다.

using UnityEngine;

namespace ZD.Gameplay.Weapons
{
    public interface IHitQuery
    {
        bool Raycast(int shooterId, Vector3 origin, Vector3 direction, float range, out HitResult hit);
        int OverlapSphere(int attackerId, Vector3 center, float radius, HitResult[] results);
    }
}
