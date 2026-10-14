// 난수 공급 계약.
// 테스트에서 고정된 값으로 바꿀 수 있다.

namespace ZD.Gameplay.Waves
{
    public interface IRandomSource
    {
        int Next(int maxExclusive);
    }
}
