// 위치 목록으로 만든 테스트용 보급품 상자.

using System.Collections.Generic;
using UnityEngine;
using ZD.Gameplay.Economy;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeSupplyBoxes : ISupplyBoxLocator
    {
        public List<Vector3> Positions { get; } = new();

        public int Count => Positions.Count;

        public Vector3 GetPosition(int index) => Positions[index];
    }
}
