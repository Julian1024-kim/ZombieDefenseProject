// 틱을 직접 진행시킬 수 있는 테스트용 시계(60Hz).

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
    internal sealed class FakeClock : INetworkClock
    {
        public const int TICK_RATE = 60;

        public int Tick { get; set; } = 1;
        public float DeltaTime => 1f / TICK_RATE;

        public int SecondsToTicks(float seconds) => Mathf.CeilToInt(seconds * TICK_RATE);

        public void Advance(int ticks) => Tick += ticks;
    }
}
