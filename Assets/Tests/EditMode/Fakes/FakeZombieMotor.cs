// 마지막 목적지와 멈춤 여부를 기록하는 테스트용 좀비 이동기.

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
    internal sealed class FakeZombieMotor : IZombieMotor
    {
        public Vector3? Destination { get; private set; }
        public bool IsStopped { get; private set; }
        public bool IsPathBlocked { get; set; }

        public void MoveTowards(Vector3 destination, float speed)
        {
            Destination = destination;
            IsStopped = false;
        }

        public void Stop()
        {
            Destination = null;
            IsStopped = true;
        }

        public void FaceTowards(Vector3 point) { }
    }
}
