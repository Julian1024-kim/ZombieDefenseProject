// 충족 여부를 직접 정할 수 있는 테스트용 패배 조건.

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
    internal sealed class FakeDefeatCondition : IDefeatCondition
    {
        public bool Met { get; set; }

        public bool IsMet() => Met;
    }
}
