// 좀비가 노릴 대상을 고르는 방식(Strategy).
// 모드마다 다르고, GameModeSystem이 고른다.

namespace ZD.Gameplay.Zombies
{
    public interface ITargetSelector
    {
        ZombieTarget Select(IZombieModel zombie);
    }
}
