// 정해 둔 결과를 돌려주는 테스트용 피격 판정.

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
    internal sealed class FakeHitQuery : IHitQuery
    {
        public HitResult? RaycastHit { get; set; }
        public List<HitResult> OverlapHits { get; } = new();
        public int RaycastCount { get; private set; }

        public bool Raycast(int shooterId, Vector3 origin, Vector3 direction, float range, out HitResult hit)
        {
            RaycastCount++;
            hit = RaycastHit.GetValueOrDefault();
            return RaycastHit.HasValue;
        }

        public int OverlapSphere(int attackerId, Vector3 center, float radius, HitResult[] results)
        {
            int count = Math.Min(OverlapHits.Count, results.Length);
            for (int hitIndex = 0; hitIndex < count; hitIndex++)
            {
                results[hitIndex] = OverlapHits[hitIndex];
            }

            return count;
        }
    }
}
