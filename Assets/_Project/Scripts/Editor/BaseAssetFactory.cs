// 거점(거점 방어 모드의 발전기) 정의와 네트워크 프리팹을 만든다.
// 모델이 없어 기본 도형으로 발전기를 흉내 낸다.
// 바리케이드처럼 Barricade 레이어(플레이어가 통과 못 함, 방어막 겹침 검사에 걸림)이고, NavMeshObstacle로 좀비 길을 깎는다.

using Fusion;
using UnityEngine;
using UnityEngine.AI;
using ZD.Gameplay.Defense;
using ZD.Network.Defense;

namespace ZD.Editor
{
    internal static class BaseAssetFactory
    {
        public const string PREFAB_NAME = "Base";

        public static NetworkObject CreateAll()
        {
            BaseDefinition definition = MvpAssetFactory.CreateAsset<BaseDefinition>("BaseDefinition", so =>
            {
                so.FindProperty("_maxHealth").intValue = 3000;
                so.FindProperty("_waveClearHealFraction").floatValue = 0.2f;
                so.FindProperty("_size").vector3Value = new Vector3(3f, 2f, 3f);
                so.FindProperty("_playerAggroRadius").floatValue = 6f;
            });
            return CreatePrefab(definition);
        }

        private static NetworkObject CreatePrefab(BaseDefinition definition)
        {
            int layer = LayerMask.NameToLayer(ProjectSettingsSetup.BARRICADE_LAYER);
            var root = new GameObject(PREFAB_NAME) { layer = layer };
            root.AddComponent<NetworkObject>();
            root.AddComponent<NetworkTransform>();
            BaseNetwork baseNetwork = root.AddComponent<BaseNetwork>();
            MvpAssetFactory.SetObjectField(baseNetwork, "_definition", definition);

            Vector3 size = definition.Size;
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, size.y * 0.5f, 0f);
            collider.size = size;

            NavMeshObstacle obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = collider.center;
            obstacle.size = size;
            obstacle.carving = true;
            obstacle.carveOnlyStationary = true;

            var visual = new GameObject("Visual") { layer = layer };
            visual.transform.SetParent(root.transform, false);
            CreateGenerator(visual.transform, size);
            return MvpAssetFactory.SavePrefab(root, PREFAB_NAME);
        }

        private static void CreateGenerator(Transform parent, Vector3 size)
        {
            Material dark = MvpAssetFactory.CreateMaterial("M_BaseDark", new Color(0.16f, 0.17f, 0.18f));
            Material body = MvpAssetFactory.CreateMaterial("M_BaseBody", new Color(0.3f, 0.35f, 0.26f));
            Material accent = MvpAssetFactory.CreateMaterial("M_BaseAccent", new Color(0.95f, 0.75f, 0.2f));
            Material lamp = MvpAssetFactory.CreateMaterial("M_BaseLamp", new Color(0.9f, 0.15f, 0.1f));

            MvpAssetFactory.CreateVisual(PrimitiveType.Cube, "Pad", parent, new Vector3(0f, 0.1f, 0f), new Vector3(size.x, 0.2f, size.z), dark);
            MvpAssetFactory.CreateVisual(PrimitiveType.Cube, "Body", parent, new Vector3(0f, 0.95f, 0f), new Vector3(size.x * 0.8f, 1.5f, size.z * 0.6f), body);
            MvpAssetFactory.CreateVisual(PrimitiveType.Cube, "Hood", parent, new Vector3(0f, 1.8f, 0f), new Vector3(size.x * 0.7f, 0.2f, size.z * 0.5f), dark);
            for (int side = -1; side <= 1; side += 2)
            {
                MvpAssetFactory.CreateVisual(PrimitiveType.Cube, "Radiator", parent, new Vector3(side * size.x * 0.41f, 0.95f, 0f), new Vector3(0.06f, 1.1f, size.z * 0.45f), dark);
                MvpAssetFactory.CreateVisual(PrimitiveType.Cylinder, "Exhaust", parent, new Vector3(side * 0.55f, 2.05f, -0.2f), new Vector3(0.22f, 0.25f, 0.22f), dark);
            }

            for (int face = -1; face <= 1; face += 2)
            {
                MvpAssetFactory.CreateVisual(PrimitiveType.Cube, "Stripe", parent, new Vector3(0f, 0.45f, face * size.z * 0.305f), new Vector3(size.x * 0.81f, 0.14f, 0.02f), accent);
                MvpAssetFactory.CreateVisual(PrimitiveType.Cube, "Panel", parent, new Vector3(0.5f, 1.1f, face * size.z * 0.305f), new Vector3(0.6f, 0.45f, 0.02f), accent);
            }

            MvpAssetFactory.CreateVisual(PrimitiveType.Sphere, "Lamp", parent, new Vector3(0.3f, 2.0f, 0.2f), new Vector3(0.22f, 0.22f, 0.22f), lamp);
        }
    }
}
