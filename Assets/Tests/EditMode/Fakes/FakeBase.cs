// 테스트용 거점.
// 체력은 처음에 최대 체력이다.

using UnityEngine;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Defense;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeBase : IBaseModel
    {
        public FakeBase(BaseDefinition definition, Vector3 position)
        {
            Definition = definition;
            Position = position;
            Health = definition.MaxHealth;
        }

        public int EntityId => 900;
        public DamageTargetKind Kind => DamageTargetKind.Base;
        public int Health { get; set; }
        public int MaxHealth => Definition.MaxHealth;
        public BaseDefinition Definition { get; }
        public Vector3 Position { get; }
    }
}
