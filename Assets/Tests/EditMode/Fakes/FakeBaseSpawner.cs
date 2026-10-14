// 거점을 원점에 만들어 명단에 등록하고, 몇 번 불렸는지 센다.

using UnityEngine;
using ZD.Gameplay.Defense;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeBaseSpawner : IBaseSpawner
    {
        private readonly BaseRosterModel _roster;
        private readonly BaseDefinition _definition;

        public FakeBaseSpawner(BaseRosterModel roster, BaseDefinition definition)
        {
            _roster = roster;
            _definition = definition;
        }

        public int SpawnCount { get; private set; }

        public void Spawn()
        {
            SpawnCount++;
            _roster.Register(new FakeBase(_definition, Vector3.zero));
        }
    }
}
