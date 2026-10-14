// 스폰한 방어막을 명단에 등록하고 기록하는 테스트용 스폰 담당.

using System.Collections.Generic;
using UnityEngine;
using ZD.Gameplay.Building;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeBarricadeSpawner : IBarricadeSpawner
    {
        private readonly BarricadeRosterModel _roster;
        private int _nextId = 1000;

        public FakeBarricadeSpawner(BarricadeRosterModel roster)
        {
            _roster = roster;
        }

        public List<FakeBarricade> Spawned { get; } = new();
        public int DespawnAllCount { get; private set; }

        public IBarricadeModel Spawn(BarricadeDefinition definition, Vector3 position, Quaternion rotation)
        {
            var barricade = new FakeBarricade(_nextId++, definition, position, rotation);
            Spawned.Add(barricade);
            _roster.Register(barricade);
            return barricade;
        }

        public void DespawnAll()
        {
            DespawnAllCount++;
            for (int barricadeIndex = _roster.Count - 1; barricadeIndex >= 0; barricadeIndex--)
            {
                _roster.Unregister(_roster.Get(barricadeIndex));
            }
        }
    }
}
