// 방어막 상태와 행동이 한 틱에 필요한 것: 현재 틱과 좀비 명단(함정 발동 판정).

using ZD.Gameplay.Zombies;

namespace ZD.Gameplay.Building
{
    public readonly struct BarricadeTickContext
    {
        public readonly int Tick;
        public readonly ZombieRosterModel Zombies;

        public BarricadeTickContext(int tick, ZombieRosterModel zombies)
        {
            Tick = tick;
            Zombies = zombies;
        }
    }
}
