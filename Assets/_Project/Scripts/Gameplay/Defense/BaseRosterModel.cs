// 지금 세션의 거점(없으면 null, 생존 모드).
// 거점 네트워크 오브젝트가 스폰될 때 등록하고 사라질 때 뺀다.
// 거점 바닥까지의 거리와 좀비가 다가가 설 지점 계산도 여기 둔다.

using UnityEngine;

namespace ZD.Gameplay.Defense
{
    public sealed class BaseRosterModel
    {
        public IBaseModel Current { get; private set; }

        public void Register(IBaseModel baseModel) => Current = baseModel;

        public void Unregister(IBaseModel baseModel)
        {
            if (Current == baseModel)
            {
                Current = null;
            }
        }

        public static float SqrDistanceToFootprint(IBaseModel baseModel, Vector3 point)
        {
            Vector3 half = baseModel.Definition.Size * 0.5f;
            float dx = Mathf.Max(0f, Mathf.Abs(point.x - baseModel.Position.x) - half.x);
            float dz = Mathf.Max(0f, Mathf.Abs(point.z - baseModel.Position.z) - half.z);
            return dx * dx + dz * dz;
        }

        public static Vector3 ApproachPoint(IBaseModel baseModel, Vector3 point, float margin)
        {
            Vector3 half = baseModel.Definition.Size * 0.5f;
            Vector3 center = baseModel.Position;
            float x = Mathf.Clamp(point.x, center.x - half.x - margin, center.x + half.x + margin);
            float z = Mathf.Clamp(point.z, center.z - half.z - margin, center.z + half.z + margin);
            return new Vector3(x, point.y, z);
        }
    }
}
