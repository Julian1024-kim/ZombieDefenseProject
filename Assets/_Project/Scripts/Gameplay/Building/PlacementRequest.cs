// 규칙 하나가 판정에 쓰는 값: 설치자 위치, 바닥에 붙인 설치 위치와 회전, 바닥 법선, 방어막 종류, 설치자의 Point.

using UnityEngine;

namespace ZD.Gameplay.Building
{
    public readonly struct PlacementRequest
    {
        public readonly Vector3 BuilderPosition;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 GroundNormal;
        public readonly BarricadeDefinition Definition;
        public readonly int Points;

        public PlacementRequest(Vector3 builderPosition, Vector3 position, Quaternion rotation, Vector3 groundNormal, BarricadeDefinition definition, int points)
        {
            BuilderPosition = builderPosition;
            Position = position;
            Rotation = rotation;
            GroundNormal = groundNormal;
            Definition = definition;
            Points = points;
        }
    }
}
