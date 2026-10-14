// 보급품 상자들의 위치.
// 씬 컴포넌트가 구현한다.

using UnityEngine;

namespace ZD.Gameplay.Economy
{
    public interface ISupplyBoxLocator
    {
        int Count { get; }
        Vector3 GetPosition(int index);
    }
}
