// 스폰된 방어막 명단(모든 피어).
// 호스트는 방어막 시스템과 좀비 AI가, 각 피어는 설치 동작 표시가 쓴다.

using System.Collections.Generic;
using UnityEngine;

namespace ZD.Gameplay.Building
{
    public sealed class BarricadeRosterModel
    {
        private const int INITIAL_CAPACITY = 32;

        private readonly List<IBarricadeModel> _barricades = new(INITIAL_CAPACITY);

        public int Count => _barricades.Count;

        public IBarricadeModel Get(int index) => _barricades[index];

        public void Register(IBarricadeModel barricade)
        {
            _barricades.Add(barricade);
        }

        public void Unregister(IBarricadeModel barricade)
        {
            int index = _barricades.IndexOf(barricade);
            if (index < 0)
            {
                return;
            }

            int lastIndex = _barricades.Count - 1;
            _barricades[index] = _barricades[lastIndex];
            _barricades.RemoveAt(lastIndex);
        }

        public IBarricadeModel FindNearestActiveWall(Vector3 point, out float sqrDistance)
        {
            IBarricadeModel nearest = null;
            sqrDistance = float.MaxValue;
            for (int barricadeIndex = 0; barricadeIndex < _barricades.Count; barricadeIndex++)
            {
                IBarricadeModel barricade = _barricades[barricadeIndex];
                if (barricade.State != BarricadeStateId.Active || !barricade.Definition.BlocksPath)
                {
                    continue;
                }

                float distance = SqrDistanceToFootprint(barricade, point);
                if (distance < sqrDistance)
                {
                    sqrDistance = distance;
                    nearest = barricade;
                }
            }

            return nearest;
        }

        public IBarricadeModel FindWallBetween(Vector3 from, Vector3 to, out float sqrDistance)
        {
            IBarricadeModel nearest = null;
            sqrDistance = float.MaxValue;
            for (int barricadeIndex = 0; barricadeIndex < _barricades.Count; barricadeIndex++)
            {
                IBarricadeModel barricade = _barricades[barricadeIndex];
                if (barricade.State != BarricadeStateId.Active || !barricade.Definition.BlocksPath || !SegmentCrossesFootprint(barricade, from, to))
                {
                    continue;
                }

                float distance = SqrDistanceToFootprint(barricade, from);
                if (distance < sqrDistance)
                {
                    sqrDistance = distance;
                    nearest = barricade;
                }
            }

            return nearest;
        }

        public bool IsInstalling(int builderId)
        {
            for (int barricadeIndex = 0; barricadeIndex < _barricades.Count; barricadeIndex++)
            {
                IBarricadeModel barricade = _barricades[barricadeIndex];
                if (barricade.State == BarricadeStateId.Installing && barricade.BuilderId == builderId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SegmentCrossesFootprint(IBarricadeModel barricade, Vector3 from, Vector3 to)
        {
            Quaternion inverse = Quaternion.Inverse(barricade.Rotation);
            Vector3 start = inverse * (from - barricade.Position);
            Vector3 delta = inverse * (to - barricade.Position) - start;
            Vector3 half = barricade.Definition.Size * 0.5f;
            float enter = 0f;
            float exit = 1f;
            return ClipAxis(start.x, delta.x, half.x, ref enter, ref exit) && ClipAxis(start.z, delta.z, half.z, ref enter, ref exit);
        }

        private static bool ClipAxis(float start, float delta, float half, ref float enter, ref float exit)
        {
            if (Mathf.Abs(delta) < 1e-6f)
            {
                return Mathf.Abs(start) <= half;
            }

            float t0 = (-half - start) / delta;
            float t1 = (half - start) / delta;
            if (t0 > t1)
            {
                (t0, t1) = (t1, t0);
            }

            enter = Mathf.Max(enter, t0);
            exit = Mathf.Min(exit, t1);
            return enter <= exit;
        }

        public static float SqrDistanceToFootprint(IBarricadeModel barricade, Vector3 point)
        {
            Vector3 local = Quaternion.Inverse(barricade.Rotation) * (point - barricade.Position);
            Vector3 half = barricade.Definition.Size * 0.5f;
            float dx = Mathf.Max(0f, Mathf.Abs(local.x) - half.x);
            float dz = Mathf.Max(0f, Mathf.Abs(local.z) - half.z);
            return dx * dx + dz * dz;
        }
    }
}
