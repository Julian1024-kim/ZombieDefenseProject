// 거점을 맵의 거점 자리에 스폰한다(호스트 전용).
// 네트워크 계층이 구현한다.

namespace ZD.Gameplay.Defense
{
    public interface IBaseSpawner
    {
        void Spawn();
    }
}
