// 호스트 전용 방어막 스폰 계약.
// 네트워크 계층이 Fusion으로 구현해서 Gameplay는 Fusion을 모른다.

using UnityEngine;

namespace ZD.Gameplay.Building
{
    public interface IBarricadeSpawner
    {
        IBarricadeModel Spawn(BarricadeDefinition definition, Vector3 position, Quaternion rotation);

        void DespawnAll();
    }
}
