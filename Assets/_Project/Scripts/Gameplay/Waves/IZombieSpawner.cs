// 스폰 지점에 좀비를 생성한다.
// 네트워크 계층이 NetworkObject로 구현하고, 생성된 좀비는 ZombieRosterModel에 스스로 등록한다.

using ZD.Gameplay.Zombies;

namespace ZD.Gameplay.Waves
{
    public interface IZombieSpawner
    {
        void Spawn(ZombieDefinition definition);

        void DespawnAll();
    }
}
