// 방어막 4종(바리케이드, 콘크리트, 지뢰, 곰덫)의 정의 에셋, 설치 카탈로그, 머티리얼, 네트워크 프리팹을 만든다.
// 바리케이드·콘크리트·지뢰는 에셋 스토어 모델(없으면 기본 도형)을, 곰덫은 기본 도형으로 만든 모양을 쓴다.
// 판정 크기(충돌, 길막기, 겹침)는 모델을 재서 정하고, 같은 모양을 설치 미리보기 고스트에도 쓴다.

using System.Collections.Generic;
using Fusion;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using ZD.Gameplay.Building;
using ZD.Network.Building;
using ZD.Presentation.Building;

namespace ZD.Editor
{
    internal static class BarricadeAssetFactory
    {
        private const string UNLIT_SHADER = "Universal Render Pipeline/Unlit";
        private const string BARRICADE_LAYER = ProjectSettingsSetup.BARRICADE_LAYER;

        public static Material GhostValidMaterial { get; private set; }
        public static Material GhostInvalidMaterial { get; private set; }

        public enum Shape
        {
            WoodBarricade = 0,
            Concrete = 1,
            Mine = 2,
            BearTrap = 3,
        }

        public static BuildCatalog CreateAll(Material flashMaterial, out BarricadeDefinition[] definitions, out NetworkObject[] prefabs)
        {
            Material wood = MvpAssetFactory.CreateMaterial("M_Wood", new Color(0.45f, 0.3f, 0.18f));
            Material concrete = MvpAssetFactory.CreateMaterial("M_Concrete", new Color(0.45f, 0.45f, 0.43f));
            Material mine = MvpAssetFactory.CreateMaterial("M_Mine", new Color(0.28f, 0.32f, 0.2f));
            Material trap = MvpAssetFactory.CreateMaterial("M_BearTrap", new Color(0.32f, 0.3f, 0.28f));
            trap.SetFloat("_Metallic", 0.8f);
            trap.SetFloat("_Smoothness", 0.35f);
            Material installing = CreateTransparentMaterial("M_Installing", new Color(0.45f, 0.75f, 1f, 0.45f));
            Material explosion = CreateTransparentMaterial("M_Explosion", new Color(1f, 0.55f, 0.15f, 0.6f));
            GhostValidMaterial = CreateTransparentMaterial("M_GhostValid", new Color(0.3f, 1f, 0.4f, 0.35f));
            GhostInvalidMaterial = CreateTransparentMaterial("M_GhostInvalid", new Color(1f, 0.25f, 0.25f, 0.35f));

            definitions = new[]
            {
                CreateDefinition("BD_Barricade", "Barricade", BarricadeKind.Wall, 300, MeasureModel(Shape.WoodBarricade, new Vector3(2f, 1.2f, 0.4f)), 300, 0f, 0f, 0, 0f, 0),
                CreateDefinition("BD_Concrete", "Concrete", BarricadeKind.Wall, 800, MeasureModel(Shape.Concrete, new Vector3(2f, 1f, 0.6f)), 1000, 0f, 0f, 0, 0f, 0),
                CreateDefinition("BD_Mine", "Mine", BarricadeKind.Mine, 500, MeasureModel(Shape.Mine, new Vector3(0.6f, 0.15f, 0.6f)), 1, 0.8f, 3f, 150, 0f, 1),
                CreateDefinition("BD_BearTrap", "Bear Trap", BarricadeKind.BearTrap, 400, new Vector3(0.7f, 0.15f, 0.7f), 1, 0.8f, 0f, 40, 3f, 3),
            };

            Material[] materials = { wood, concrete, mine, trap };
            prefabs = new NetworkObject[definitions.Length];
            for (int itemIndex = 0; itemIndex < definitions.Length; itemIndex++)
            {
                prefabs[itemIndex] = CreatePrefab(definitions[itemIndex], (Shape)itemIndex, materials[itemIndex], installing, flashMaterial, explosion);
            }

            int environmentMask = LayerMask.GetMask(ProjectSettingsSetup.ENVIRONMENT_LAYER);
            BarricadeDefinition[] items = definitions;
            return MvpAssetFactory.CreateAsset<BuildCatalog>("BuildCatalog", so =>
            {
                SerializedProperty array = so.FindProperty("_items");
                array.arraySize = items.Length;
                for (int itemIndex = 0; itemIndex < items.Length; itemIndex++)
                {
                    array.GetArrayElementAtIndex(itemIndex).objectReferenceValue = items[itemIndex];
                }

                so.FindProperty("_maxPlaceDistance").floatValue = 4f;
                so.FindProperty("_maxSlopeDegrees").floatValue = 20f;
                so.FindProperty("_installSeconds").floatValue = 3f;
                so.FindProperty("_cancelMoveDistance").floatValue = 0.3f;
                so.FindProperty("_groundMask").intValue = LayerMask.GetMask("Default") | environmentMask;
                so.FindProperty("_blockingMask").intValue = environmentMask | LayerMask.GetMask(BARRICADE_LAYER, "Player", "Zombie");
            });
        }

        public static Renderer[] CreateVisual(Shape shape, BarricadeDefinition definition, Transform parent, Material material, Material ghostMaterial = null)
        {
            string modelPath = ModelPath(shape);
            if (modelPath != null && AssetDatabase.LoadAssetAtPath<GameObject>(modelPath) != null)
            {
                return CreateModelVisual(modelPath, ModelRotation(shape), parent, ghostMaterial);
            }

            return CreatePrimitiveVisual(shape, definition, parent, ghostMaterial != null ? ghostMaterial : material);
        }

        private static Renderer[] CreateModelVisual(string path, Quaternion rotation, Transform parent, Material ghostMaterial)
        {
            var model = (GameObject)Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path), parent, false);
            model.name = "Model";
            model.transform.localRotation = rotation;
            Collider[] colliders = model.GetComponentsInChildren<Collider>(true);
            for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
            {
                Object.DestroyImmediate(colliders[colliderIndex]);
            }

            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = WorldBounds(renderers);
            model.transform.localPosition = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
            MvpAssetFactory.SetLayerRecursively(model.transform, parent.gameObject.layer);
            if (ghostMaterial != null)
            {
                for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    var slots = new Material[renderers[rendererIndex].sharedMaterials.Length];
                    for (int slotIndex = 0; slotIndex < slots.Length; slotIndex++)
                    {
                        slots[slotIndex] = ghostMaterial;
                    }

                    renderers[rendererIndex].sharedMaterials = slots;
                    renderers[rendererIndex].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }

            return renderers;
        }

        private static Vector3 MeasureModel(Shape shape, Vector3 fallback)
        {
            string path = ModelPath(shape);
            if (path == null || AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                return fallback;
            }

            var probe = new GameObject("SizeProbe");
            CreateModelVisual(path, ModelRotation(shape), probe.transform, null);
            Vector3 size = WorldBounds(probe.GetComponentsInChildren<Renderer>(true)).size;
            Object.DestroyImmediate(probe);
            return size;
        }

        private static Bounds WorldBounds(Renderer[] renderers)
        {
            Bounds bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds();
            for (int rendererIndex = 1; rendererIndex < renderers.Length; rendererIndex++)
            {
                bounds.Encapsulate(renderers[rendererIndex].bounds);
            }

            return bounds;
        }

        private static string ModelPath(Shape shape)
        {
            return shape switch
            {
                Shape.WoodBarricade => ThirdPartyAssetSetup.BARRICADE,
                Shape.Concrete => ThirdPartyAssetSetup.CONCRETE,
                Shape.Mine => ThirdPartyAssetSetup.MINE,
                _ => null,
            };
        }

        private static Quaternion ModelRotation(Shape shape)
        {
            return shape == Shape.Mine ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
        }

        private static Renderer[] CreatePrimitiveVisual(Shape shape, BarricadeDefinition definition, Transform parent, Material material)
        {
            Vector3 size = definition.Size;
            switch (shape)
            {
                case Shape.WoodBarricade:
                    return new[]
                    {
                        Part(PrimitiveType.Cube, "Post_L", parent, new Vector3(-size.x * 0.45f, size.y * 0.5f, 0f), new Vector3(0.12f, size.y, 0.12f), material),
                        Part(PrimitiveType.Cube, "Post_R", parent, new Vector3(size.x * 0.45f, size.y * 0.5f, 0f), new Vector3(0.12f, size.y, 0.12f), material),
                        Part(PrimitiveType.Cube, "Plank_Low", parent, new Vector3(0f, size.y * 0.25f, size.z * 0.25f), new Vector3(size.x, 0.22f, 0.06f), material),
                        Part(PrimitiveType.Cube, "Plank_Mid", parent, new Vector3(0f, size.y * 0.55f, size.z * 0.25f), new Vector3(size.x, 0.22f, 0.06f), material),
                        Part(PrimitiveType.Cube, "Plank_High", parent, new Vector3(0f, size.y * 0.85f, size.z * 0.25f), new Vector3(size.x, 0.22f, 0.06f), material),
                    };
                case Shape.Concrete:
                    return new[]
                    {
                        Part(PrimitiveType.Cube, "Block", parent, new Vector3(0f, size.y * 0.5f, 0f), size, material),
                    };
                case Shape.Mine:
                    return new[]
                    {
                        Part(PrimitiveType.Cylinder, "Body", parent, new Vector3(0f, 0.04f, 0f), new Vector3(size.x * 0.8f, 0.04f, size.z * 0.8f), material),
                        Part(PrimitiveType.Cylinder, "Trigger", parent, new Vector3(0f, 0.1f, 0f), new Vector3(0.12f, 0.03f, 0.12f), material),
                    };
                default:
                    return CreateBearTrap(size, parent, material);
            }
        }

        private static Renderer[] CreateBearTrap(Vector3 size, Transform parent, Material material)
        {
            const int TEETH_PER_JAW = 7;
            float radius = size.x * 0.42f;
            var parts = new List<Renderer>
            {
                Part(PrimitiveType.Cylinder, "Ring", parent, new Vector3(0f, 0.012f, 0f), new Vector3(size.x * 0.95f, 0.012f, size.z * 0.95f), material),
                Part(PrimitiveType.Cylinder, "Plate", parent, new Vector3(0f, 0.03f, 0f), new Vector3(size.x * 0.35f, 0.012f, size.z * 0.35f), material),
                Part(PrimitiveType.Cube, "Spring_L", parent, new Vector3(-size.x * 0.62f, 0.025f, 0f), new Vector3(size.x * 0.35f, 0.04f, 0.05f), material),
                Part(PrimitiveType.Cube, "Spring_R", parent, new Vector3(size.x * 0.62f, 0.025f, 0f), new Vector3(size.x * 0.35f, 0.04f, 0.05f), material),
                Part(PrimitiveType.Cube, "Chain", parent, new Vector3(0f, 0.01f, -size.z * 0.7f), new Vector3(0.03f, 0.02f, size.z * 0.45f), material),
            };

            // 두 턱: 앞뒤 반원을 따라 톱니(작은 상자)를 세운다. 살짝 바깥으로 벌어진 모습.
            for (int jawIndex = 0; jawIndex < 2; jawIndex++)
            {
                float side = jawIndex == 0 ? 1f : -1f;
                for (int toothIndex = 0; toothIndex < TEETH_PER_JAW; toothIndex++)
                {
                    float angle = Mathf.Lerp(20f, 160f, toothIndex / (float)(TEETH_PER_JAW - 1)) * Mathf.Deg2Rad;
                    var position = new Vector3(Mathf.Cos(angle) * radius, 0.06f, Mathf.Sin(angle) * radius * side);
                    Renderer tooth = Part(PrimitiveType.Cube, $"Tooth_{jawIndex}_{toothIndex}", parent, position, new Vector3(0.035f, 0.09f, 0.02f), material);
                    tooth.transform.localRotation = Quaternion.Euler(-25f * side, -angle * Mathf.Rad2Deg * side + 90f, 0f);
                    parts.Add(tooth);
                }
            }

            return parts.ToArray();
        }

        private static NetworkObject CreatePrefab(BarricadeDefinition definition, Shape shape, Material material, Material installing, Material flash, Material explosion)
        {
            int layer = LayerMask.NameToLayer(BARRICADE_LAYER);
            var root = new GameObject(PrefabName(shape)) { layer = layer };
            root.AddComponent<NetworkObject>();
            root.AddComponent<NetworkTransform>();
            BarricadeNetwork barricade = root.AddComponent<BarricadeNetwork>();
            MvpAssetFactory.SetObjectField(barricade, "_definition", definition);

            Vector3 size = definition.Size;
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, size.y * 0.5f, 0f);
            collider.size = size;
            collider.isTrigger = !definition.BlocksPath;

            if (definition.BlocksPath)
            {
                NavMeshObstacle obstacle = root.AddComponent<NavMeshObstacle>();
                obstacle.shape = NavMeshObstacleShape.Box;
                obstacle.center = collider.center;
                obstacle.size = size;
                obstacle.carving = true;
                obstacle.carveOnlyStationary = true;
            }

            var visual = new GameObject("Visual") { layer = layer };
            visual.transform.SetParent(root.transform, false);
            Renderer[] renderers = CreateVisual(shape, definition, visual.transform, material);

            Transform explosionSphere = null;
            if (definition.Kind == BarricadeKind.Mine)
            {
                explosionSphere = Part(PrimitiveType.Sphere, "Explosion", root.transform, new Vector3(0f, 0.3f, 0f), Vector3.one, explosion).transform;
            }

            BarricadeView view = root.AddComponent<BarricadeView>();
            MvpAssetFactory.SetObjectArray(view, "_renderers", renderers);
            MvpAssetFactory.SetObjectField(view, "_installingMaterial", installing);
            MvpAssetFactory.SetObjectField(view, "_flashMaterial", flash);
            if (explosionSphere != null)
            {
                MvpAssetFactory.SetObjectField(view, "_explosion", explosionSphere);
                var serialized = new SerializedObject(view);
                serialized.FindProperty("_explosionRadius").floatValue = definition.EffectRadius;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            return MvpAssetFactory.SavePrefab(root, PrefabName(shape));
        }

        public static string PrefabName(Shape shape)
        {
            return shape switch
            {
                Shape.WoodBarricade => "Barricade_Wood",
                Shape.Concrete => "Barricade_Concrete",
                Shape.Mine => "Trap_Mine",
                _ => "Trap_BearTrap",
            };
        }

        private static BarricadeDefinition CreateDefinition(string assetName, string displayName, BarricadeKind kind, int price, Vector3 size, int maxHealth,
            float triggerRadius, float effectRadius, int trapDamage, float holdSeconds, int uses)
        {
            return MvpAssetFactory.CreateAsset<BarricadeDefinition>(assetName, so =>
            {
                so.FindProperty("_displayName").stringValue = displayName;
                so.FindProperty("_kind").enumValueIndex = (int)kind;
                so.FindProperty("_price").intValue = price;
                so.FindProperty("_size").vector3Value = size;
                so.FindProperty("_maxHealth").intValue = maxHealth;
                so.FindProperty("_triggerRadius").floatValue = triggerRadius;
                so.FindProperty("_effectRadius").floatValue = effectRadius;
                so.FindProperty("_trapDamage").intValue = trapDamage;
                so.FindProperty("_holdSeconds").floatValue = holdSeconds;
                so.FindProperty("_uses").intValue = uses;
            });
        }

        private static Renderer Part(PrimitiveType primitive, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            return MvpAssetFactory.CreateVisual(primitive, name, parent, position, scale, material).GetComponent<Renderer>();
        }

        private static Material CreateTransparentMaterial(string name, Color color)
        {
            Material material = MvpAssetFactory.CreateMaterial(name, color, UNLIT_SHADER);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetOverrideTag("RenderType", "Transparent");
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
