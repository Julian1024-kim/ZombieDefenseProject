// 테스트용 무기 상태 모델.

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
    internal sealed class FakeWeapon : IWeaponModel
    {
        public int AmmoInMagazine { get; set; }
        public int ReserveAmmo { get; set; }
        public int NextFireTick { get; set; }
        public int ReloadEndTick { get; set; }
    }
}
