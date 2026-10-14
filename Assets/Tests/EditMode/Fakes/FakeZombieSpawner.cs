// 생성한 좀비를 명단에 넣고 기록하는 테스트용 스포너.

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
    internal sealed class FakeZombieSpawner : IZombieSpawner
    {
        private readonly ZombieRosterModel _roster;
        private int _nextId = 1000;

        public FakeZombieSpawner(ZombieRosterModel roster)
        {
            _roster = roster;
        }

        public List<FakeZombie> Spawned { get; } = new();
        public int DespawnAllCount { get; private set; }

        public void Spawn(ZombieDefinition definition)
        {
            var zombie = new FakeZombie(_nextId++, definition);
            Spawned.Add(zombie);
            _roster.Register(zombie, new FakeZombieMotor());
        }

        public void DespawnAll()
        {
            DespawnAllCount++;
            for (int zombieIndex = 0; zombieIndex < Spawned.Count; zombieIndex++)
            {
                _roster.Unregister(Spawned[zombieIndex]);
            }

            Spawned.Clear();
        }

        public void KillAll()
        {
            for (int zombieIndex = 0; zombieIndex < Spawned.Count; zombieIndex++)
            {
                Spawned[zombieIndex].Health = 0;
            }
        }
    }
}
