// 테스트용 방어막 모델.

using UnityEngine;
using ZD.Gameplay.Building;
using ZD.Gameplay.Combat;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeBarricade : IBarricadeModel
    {
        public FakeBarricade(int id, BarricadeDefinition definition, Vector3 position, Quaternion rotation)
        {
            EntityId = id;
            Definition = definition;
            Position = position;
            Rotation = rotation;
        }

        public int EntityId { get; }
        public DamageTargetKind Kind => DamageTargetKind.Barricade;
        public int Health { get; set; }
        public int MaxHealth => Definition.MaxHealth;
        public BarricadeDefinition Definition { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public BarricadeStateId State { get; set; }
        public int InstallEndTick { get; set; }
        public int BuilderId { get; set; }
        public Vector3 BuilderStartPosition { get; set; }
        public int BuilderStartHealth { get; set; }
        public int UsesLeft { get; set; }
        public int BusyUntilTick { get; set; }
    }
}
