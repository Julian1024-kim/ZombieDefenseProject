// 테스트용 좀비 모델.

using System;
using System.Collections.Generic;
using MessagePipe;
using UnityEditor;
using UnityEngine;
using ZD.Core;
using ZD.Gameplay.Combat;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Players;
using ZD.Gameplay.Waves;
using ZD.Gameplay.Weapons;
using ZD.Gameplay.Zombies;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeZombie : IZombieModel
    {
        public FakeZombie(int id, ZombieDefinition definition, Vector3 position = default)
        {
            EntityId = id;
            Definition = definition;
            MaxHealth = definition.MaxHealth;
            Health = definition.MaxHealth;
            Position = position;
        }

        public int EntityId { get; }
        public DamageTargetKind Kind => DamageTargetKind.Zombie;
        public int Health { get; set; }
        public int MaxHealth { get; }
        public ZombieDefinition Definition { get; }
        public Vector3 Position { get; set; }
        public ZombieStateId State { get; set; }
        public int NextAttackTick { get; set; }
        public int HeldUntilTick { get; set; }
    }
}
