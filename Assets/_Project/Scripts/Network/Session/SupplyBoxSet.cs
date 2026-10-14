// 씬에 놓인 보급품 상자들.
// 움직이지 않으므로 동기화 없이 모든 피어가 위치를 안다.

using UnityEngine;
using ZD.Gameplay.Economy;

namespace ZD.Network.Session
{
    [DisallowMultipleComponent]
    public sealed class SupplyBoxSet : MonoBehaviour, ISupplyBoxLocator
    {
        [SerializeField] private Transform[] _boxes;

        public int Count => _boxes.Length;

        public Vector3 GetPosition(int index) => _boxes[index].position;
    }
}
