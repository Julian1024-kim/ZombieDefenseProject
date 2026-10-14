// MVP 데이터 에셋, 임시 머티리얼, 네트워크 프리팹을 만들거나 그 자리에서 갱신한다.
// 다시 실행해도 에셋 GUID가 유지되어 씬 참조가 끊기지 않는다.

using System;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;
using ZD.Gameplay.Building;
using ZD.Gameplay.Cameras;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Economy;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Players;
using ZD.Gameplay.Waves;
using ZD.Gameplay.Weapons;
using ZD.Gameplay.Zombies;
using ZD.Network.Combat;
using ZD.Network.GameFlow;
using ZD.Network.Players;
using ZD.Network.Session;
using ZD.Network.Zombies;
using ZD.Presentation.Players;
using ZD.Presentation.Zombies;

namespace ZD.Editor
{
    internal static class MvpAssetFactory
    {
        public const string DATA_FOLDER = "Assets/_Project/Data";
        public const string MATERIAL_FOLDER = "Assets/_Project/Art/Materials";
        public const string PREFAB_FOLDER = "Assets/_Project/Prefabs";

        private const string LIT_SHADER = "Universal Render Pipeline/Lit";
        private const string UNLIT_SHADER = "Universal Render Pipeline/Unlit";
        private const int GUID_BYTE_COUNT = 16;

        // 좀비 모델(약 2.0m)을 사람 키(약 1.8m)로 줄인다.
        private const float NORMAL_ZOMBIE_SCALE = 0.9f;

        // 4인까지 플레이하므로 플레이어 순번마다 다른 모델을 쓴다. 일반 좀비는 두 모델 중 호스트가 무작위로 고른다.
        private static readonly string[] PlayerModels = { "Player1", "Player2", "Player3", "Player4" };
        private static readonly string[] NormalZombieModels = { "Zombie1", "Zombie2" };

        public static Material GroundMaterial { get; private set; }
        public static Material WallMaterial { get; private set; }
        public static Material TracerMaterial { get; private set; }
        public static Material SparkMaterial { get; private set; }
        public static Material CrateMaterial { get; private set; }

        public static MvpAssets CreateAll()
        {
            EnsureFolder(DATA_FOLDER);
            EnsureFolder(MATERIAL_FOLDER);
            EnsureFolder(PREFAB_FOLDER);

            Material rockMaterial = CreateMaterial("M_ThrowRock", new Color(0.4f, 0.37f, 0.33f));
            Material flashMaterial = CreateMaterial("M_ZombieFlash", Color.white);
            GroundMaterial = CreateMaterial("M_Ground", new Color(0.35f, 0.35f, 0.33f));
            WallMaterial = CreateMaterial("M_Wall", new Color(0.55f, 0.5f, 0.45f));
            TracerMaterial = CreateMaterial("M_Tracer", new Color(1f, 0.85f, 0.45f), UNLIT_SHADER);
            SparkMaterial = CreateMaterial("M_Spark", new Color(1f, 0.75f, 0.3f), UNLIT_SHADER);
            CrateMaterial = CreateMaterial("M_SupplyBox", new Color(0.25f, 0.45f, 0.25f));

            WeaponDefinition pistol = CreateAsset<WeaponDefinition>("WD_Pistol", so =>
            {
                so.FindProperty("_displayName").stringValue = "Pistol";
                so.FindProperty("_category").enumValueIndex = (int)WeaponCategory.Secondary;
                so.FindProperty("_behaviorType").enumValueIndex = (int)WeaponBehaviorType.Hitscan;
                so.FindProperty("_damage").intValue = 25;
                so.FindProperty("_shotsPerSecond").floatValue = 4f;
                so.FindProperty("_isAutomatic").boolValue = false;
                so.FindProperty("_range").floatValue = 80f;
                so.FindProperty("_allowsHeadshot").boolValue = true;
                so.FindProperty("_headshotMultiplier").floatValue = 2f;
                so.FindProperty("_magazineSize").intValue = 12;
                so.FindProperty("_startingReserveAmmo").intValue = 48;
                so.FindProperty("_reloadSeconds").floatValue = 1.4f;
            });

            WeaponDefinition knife = CreateAsset<WeaponDefinition>("WD_Knife", so =>
            {
                so.FindProperty("_displayName").stringValue = "Knife";
                so.FindProperty("_category").enumValueIndex = (int)WeaponCategory.Melee;
                so.FindProperty("_behaviorType").enumValueIndex = (int)WeaponBehaviorType.Melee;
                so.FindProperty("_damage").intValue = 50;
                so.FindProperty("_shotsPerSecond").floatValue = 1.6f;
                so.FindProperty("_range").floatValue = 1f;
                so.FindProperty("_allowsHeadshot").boolValue = true;
                so.FindProperty("_headshotMultiplier").floatValue = 1.5f;
                so.FindProperty("_meleeRadius").floatValue = 0.9f;
                so.FindProperty("_maxMeleeTargets").intValue = 3;
            });

            WeaponDefinition rifle = CreateAsset<WeaponDefinition>("WD_AssaultRifle", so =>
            {
                so.FindProperty("_displayName").stringValue = "Assault Rifle";
                so.FindProperty("_category").enumValueIndex = (int)WeaponCategory.Primary;
                so.FindProperty("_behaviorType").enumValueIndex = (int)WeaponBehaviorType.Hitscan;
                so.FindProperty("_damage").intValue = 22;
                so.FindProperty("_shotsPerSecond").floatValue = 9f;
                so.FindProperty("_isAutomatic").boolValue = true;
                so.FindProperty("_range").floatValue = 80f;
                so.FindProperty("_allowsHeadshot").boolValue = true;
                so.FindProperty("_headshotMultiplier").floatValue = 2f;
                so.FindProperty("_magazineSize").intValue = 30;
                so.FindProperty("_startingReserveAmmo").intValue = 60;
                so.FindProperty("_reloadSeconds").floatValue = 2f;
            });

            WeaponDefinition shotgun = CreateAsset<WeaponDefinition>("WD_Shotgun", so =>
            {
                so.FindProperty("_displayName").stringValue = "Shotgun";
                so.FindProperty("_category").enumValueIndex = (int)WeaponCategory.Primary;
                so.FindProperty("_behaviorType").enumValueIndex = (int)WeaponBehaviorType.Spread;
                so.FindProperty("_damage").intValue = 14;
                so.FindProperty("_shotsPerSecond").floatValue = 1.2f;
                so.FindProperty("_isAutomatic").boolValue = false;
                so.FindProperty("_range").floatValue = 25f;
                so.FindProperty("_allowsHeadshot").boolValue = true;
                so.FindProperty("_headshotMultiplier").floatValue = 1.5f;
                so.FindProperty("_magazineSize").intValue = 8;
                so.FindProperty("_startingReserveAmmo").intValue = 16;
                so.FindProperty("_reloadSeconds").floatValue = 2.5f;
                so.FindProperty("_pelletCount").intValue = 8;
                so.FindProperty("_spreadDegrees").floatValue = 6f;
            });

            // ID는 목록의 순서이므로 무기는 항상 뒤에만 추가한다.
            WeaponCatalog weaponCatalog = CreateAsset<WeaponCatalog>("WeaponCatalog", so =>
                AssignObjects(so.FindProperty("_weapons"), pistol, knife, rifle, shotgun));

            InventorySettings inventorySettings = CreateAsset<InventorySettings>("InventorySettings", so =>
            {
                so.FindProperty("_startingSecondary").objectReferenceValue = pistol;
                so.FindProperty("_startingMelee").objectReferenceValue = knife;
                so.FindProperty("_maxHealKits").intValue = 2;
                so.FindProperty("_healAmount").intValue = 70;
                so.FindProperty("_healSeconds").floatValue = 3f;
            });

            ShopCatalog shopCatalog = CreateAsset<ShopCatalog>("ShopCatalog", so =>
            {
                so.FindProperty("_interactRange").floatValue = 2.5f;
                SerializedProperty items = so.FindProperty("_items");
                items.arraySize = 6;
                SetShopItem(items.GetArrayElementAtIndex(0), "Shotgun", ShopItemKind.Weapon, shotgun, 1000, 0);
                SetShopItem(items.GetArrayElementAtIndex(1), "Assault Rifle", ShopItemKind.Weapon, rifle, 1500, 0);
                SetShopItem(items.GetArrayElementAtIndex(2), "Pistol Ammo (2 mags)", ShopItemKind.Ammo, pistol, 100, 24);
                SetShopItem(items.GetArrayElementAtIndex(3), "Rifle Ammo (2 mags)", ShopItemKind.Ammo, rifle, 300, 60);
                SetShopItem(items.GetArrayElementAtIndex(4), "Shotgun Shells (16)", ShopItemKind.Ammo, shotgun, 300, 16);
                SetShopItem(items.GetArrayElementAtIndex(5), "Health Kit", ShopItemKind.HealKit, null, 600, 0);
            });

            ZombieDefinition normalZombie = CreateAsset<ZombieDefinition>("ZD_Normal", so =>
            {
                so.FindProperty("_displayName").stringValue = "Normal";
                so.FindProperty("_maxHealth").intValue = 100;
                so.FindProperty("_moveSpeed").floatValue = 2.2f;
                so.FindProperty("_attackDamage").intValue = 10;
                so.FindProperty("_attackRange").floatValue = 1.4f;
                so.FindProperty("_attackIntervalSeconds").floatValue = 1.2f;
                so.FindProperty("_killPoints").intValue = 50;
                so.FindProperty("_headshotBonusPoints").intValue = 20;
                so.FindProperty("_spawnCost").intValue = 1;
                so.FindProperty("_firstWave").intValue = 1;
            });

            WaveProfile waveProfile = CreateAsset<WaveProfile>("WaveProfile", so =>
            {
                SerializedProperty types = so.FindProperty("_zombieTypes");
                types.arraySize = 1;
                types.GetArrayElementAtIndex(0).objectReferenceValue = normalZombie;
                so.FindProperty("_baseBudget").intValue = 6;
                so.FindProperty("_budgetPerWave").intValue = 3;
                so.FindProperty("_preparationSeconds").floatValue = 30f;
                so.FindProperty("_spawnIntervalSeconds").floatValue = 0.75f;
                so.FindProperty("_maxAliveZombies").intValue = 40;
            });

            PlayerDefinition playerDefinition = CreateAsset<PlayerDefinition>("PlayerDefinition", so =>
            {
                so.FindProperty("_maxHealth").intValue = 100;
                so.FindProperty("_moveSpeed").floatValue = 5f;
                so.FindProperty("_muzzleOffset").vector3Value = new Vector3(0.3f, 1.35f, 0.5f);
                so.FindProperty("_lookSensitivity").floatValue = 0.12f;
            });

            int environmentMask = LayerMask.GetMask("Default", ProjectSettingsSetup.ENVIRONMENT_LAYER, ProjectSettingsSetup.BARRICADE_LAYER);
            CameraSettings cameraSettings = CreateAsset<CameraSettings>("CameraSettings", so =>
            {
                so.FindProperty("_pivotOffset").vector3Value = new Vector3(0f, 1.65f, 0f);
                so.FindProperty("_shoulderOffset").vector3Value = new Vector3(0.75f, 0.15f, -3.4f);
                so.FindProperty("_collisionMask").intValue = environmentMask;
            });

            HitQuerySettings hitQuerySettings = CreateAsset<HitQuerySettings>("HitQuerySettings", so =>
            {
                so.FindProperty("_hitMask").intValue = environmentMask | LayerMask.GetMask("Hitbox");
            });

            CharacterAnimatorBuilder.BuildAll();
            var playerController = AssetDatabase.LoadAssetAtPath<AnimatorController>(CharacterAnimatorBuilder.PLAYER_CONTROLLER);
            var zombieController = AssetDatabase.LoadAssetAtPath<AnimatorController>(CharacterAnimatorBuilder.ZOMBIE_CONTROLLER);
            var weaponModels = new[]
            {
                new WeaponModelAttacher.Spec(pistol, ThirdPartyAssetSetup.PISTOL, WeaponModelAttacher.Grip.Pistol, new Vector3(0f, 0.02f, 0f)),
                new WeaponModelAttacher.Spec(rifle, ThirdPartyAssetSetup.ASSAULT_RIFLE, WeaponModelAttacher.Grip.Rifle, new Vector3(0f, -0.03f, 0f), new Vector3(0f, 0.045f, 0.30f)),
                new WeaponModelAttacher.Spec(shotgun, ThirdPartyAssetSetup.SHOTGUN, WeaponModelAttacher.Grip.Rifle, new Vector3(0f, -0.06f, -0.05f), new Vector3(0f, -0.03f, 0.36f)),
                new WeaponModelAttacher.Spec(knife, ThirdPartyAssetSetup.KNIFE, WeaponModelAttacher.Grip.Knife, new Vector3(0f, 0f, -0.03f)),
            };
            NetworkObject playerPrefab = CreatePlayerPrefab(playerController, weaponModels, weaponCatalog);
            NetworkObject zombiePrefab = CreateZombiePrefab(normalZombie, zombieController, flashMaterial);
            CreateThrowRockPrefab(rockMaterial);
            NetworkObject directorPrefab = CreateDirectorPrefab();
            NetworkObject basePrefab = BaseAssetFactory.CreateAll();

            AddressablesSetup.MarkAddressable(AssetDatabase.GetAssetPath(playerPrefab), AddressablesSetup.COMMON_GROUP, "Prefabs/Player");
            AddressablesSetup.MarkAddressable(AssetDatabase.GetAssetPath(directorPrefab), AddressablesSetup.COMMON_GROUP, "Prefabs/GameDirector");
            AddressablesSetup.MarkAddressable(AssetDatabase.GetAssetPath(basePrefab), AddressablesSetup.COMMON_GROUP, "Prefabs/" + BaseAssetFactory.PREFAB_NAME);
            AddressablesSetup.MarkAddressable(AssetDatabase.GetAssetPath(zombiePrefab), AddressablesSetup.ZOMBIES_GROUP, "Prefabs/Zombie_Normal");

            BuildCatalog buildCatalog = BarricadeAssetFactory.CreateAll(flashMaterial, out BarricadeDefinition[] barricades, out NetworkObject[] barricadePrefabs);
            for (int barricadeIndex = 0; barricadeIndex < barricadePrefabs.Length; barricadeIndex++)
            {
                string address = $"Prefabs/{BarricadeAssetFactory.PrefabName((BarricadeAssetFactory.Shape)barricadeIndex)}";
                AddressablesSetup.MarkAddressable(AssetDatabase.GetAssetPath(barricadePrefabs[barricadeIndex]), AddressablesSetup.COMMON_GROUP, address);
            }

            // 카탈로그는 프리팹을 직접 참조하지 않고 GUID만 가진다. 그래야 프리팹이 Addressables 번들에만 들어간다.
            NetworkPrefabCatalog catalog = CreateAsset<NetworkPrefabCatalog>("NetworkPrefabCatalog", so =>
            {
                SetPrefabRef(so.FindProperty("_playerPrefab"), playerPrefab);
                SetPrefabRef(so.FindProperty("_gameDirectorPrefab"), directorPrefab);
                SetPrefabRef(so.FindProperty("_basePrefab"), basePrefab);
                SerializedProperty zombies = so.FindProperty("_zombiePrefabs");
                zombies.arraySize = 1;
                SerializedProperty entry = zombies.GetArrayElementAtIndex(0);
                entry.FindPropertyRelative("_definition").objectReferenceValue = normalZombie;
                SetPrefabRef(entry.FindPropertyRelative("_prefab"), zombiePrefab);

                // 배열을 늘린 직후의 새 항목은 고정 크기 배열(GUID)을 쓸 수 없어서, 크기를 먼저 적용하고 다시 읽는다.
                so.FindProperty("_barricadePrefabs").arraySize = barricades.Length;
                so.ApplyModifiedPropertiesWithoutUndo();
                so.Update();
                SerializedProperty barricadeEntries = so.FindProperty("_barricadePrefabs");
                for (int barricadeIndex = 0; barricadeIndex < barricades.Length; barricadeIndex++)
                {
                    SerializedProperty barricadeEntry = barricadeEntries.GetArrayElementAtIndex(barricadeIndex);
                    barricadeEntry.FindPropertyRelative("_definition").objectReferenceValue = barricades[barricadeIndex];
                    SetPrefabRef(barricadeEntry.FindPropertyRelative("_prefab"), barricadePrefabs[barricadeIndex]);
                }
            });

            AssetDatabase.SaveAssets();
            return new MvpAssets(playerDefinition, waveProfile, cameraSettings, catalog, hitQuerySettings, weaponCatalog, inventorySettings, shopCatalog, buildCatalog);
        }

        private static NetworkObject CreatePlayerPrefab(RuntimeAnimatorController controller, WeaponModelAttacher.Spec[] weaponModels, WeaponCatalog weaponCatalog)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            var root = new GameObject("Player") { layer = playerLayer };
            root.AddComponent<NetworkObject>();
            SimpleKCC kcc = root.AddComponent<SimpleKCC>();
            kcc.Settings.Radius = 0.35f;
            kcc.Settings.Height = 1.8f;
            kcc.Settings.ColliderLayer = playerLayer;
            kcc.Settings.CollisionLayerMask = LayerMask.GetMask("Default", ProjectSettingsSetup.ENVIRONMENT_LAYER, ProjectSettingsSetup.BARRICADE_LAYER);
            root.AddComponent<PlayerLoadoutNetwork>();
            root.AddComponent<PlayerNetwork>();

            Animator[] models = CreateCharacterModels(root.transform, PlayerModels, playerLayer, controller, 1f);
            PlayerAnimatorView view = root.AddComponent<PlayerAnimatorView>();
            SetObjectArray(view, "_variants", models);

            WeaponModelAttacher.Result weapons = WeaponModelAttacher.Attach(models, weaponModels, weaponCatalog, playerLayer);
            PlayerWeaponView weaponView = root.AddComponent<PlayerWeaponView>();
            SetObjectArray(weaponView, "_models", weapons.Models);
            SetObjectArray(weaponView, "_muzzles", weapons.Muzzles);
            var weaponSettings = new SerializedObject(weaponView);
            SerializedProperty ids = weaponSettings.FindProperty("_weaponIds");
            ids.arraySize = weapons.WeaponIds.Length;
            for (int idIndex = 0; idIndex < weapons.WeaponIds.Length; idIndex++)
            {
                ids.GetArrayElementAtIndex(idIndex).intValue = weapons.WeaponIds[idIndex];
            }

            SetVectorArray(weaponSettings.FindProperty("_gripPoints"), weapons.GripPoints);
            SetVectorArray(weaponSettings.FindProperty("_supportAxes"), weapons.SupportAxes);
            weaponSettings.ApplyModifiedPropertiesWithoutUndo();

            return SavePrefab(root, "Player");
        }

        private static NetworkObject CreateZombiePrefab(ZombieDefinition definition, RuntimeAnimatorController controller, Material flashMaterial)
        {
            int zombieLayer = LayerMask.NameToLayer("Zombie");
            int hitboxLayer = LayerMask.NameToLayer("Hitbox");

            var root = new GameObject("Zombie_Normal") { layer = zombieLayer };
            root.AddComponent<NetworkObject>();
            root.AddComponent<NetworkTransform>();

            NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.35f;
            agent.height = 1.8f;
            agent.speed = definition.MoveSpeed;
            agent.angularSpeed = 540f;
            agent.acceleration = 30f;
            agent.stoppingDistance = 0f;

            CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.9f, 0f);
            collider.height = 1.8f;
            collider.radius = 0.35f;

            HitboxRoot hitboxRoot = root.AddComponent<HitboxRoot>();
            ZombieNetwork zombie = root.AddComponent<ZombieNetwork>();
            SetObjectField(zombie, "_definition", definition);
            var zombieSettings = new SerializedObject(zombie);
            zombieSettings.FindProperty("_visualVariantCount").intValue = NormalZombieModels.Length;
            zombieSettings.ApplyModifiedPropertiesWithoutUndo();

            Animator[] models = CreateCharacterModels(root.transform, NormalZombieModels, zombieLayer, controller, NORMAL_ZOMBIE_SCALE);

            CreateHitbox(root.transform, "Hitbox_Body", hitboxLayer, HitZone.Body, HitboxTypes.Box, new Vector3(0f, 0.95f, 0f), new Vector3(0.3f, 0.55f, 0.25f), 0f);
            CreateHitbox(root.transform, "Hitbox_Head", hitboxLayer, HitZone.Head, HitboxTypes.Sphere, new Vector3(0f, 1.63f, 0.05f), Vector3.zero, 0.2f);
            CreateHitbox(root.transform, "Hitbox_Legs", hitboxLayer, HitZone.Legs, HitboxTypes.Box, new Vector3(0f, 0.2f, 0f), new Vector3(0.25f, 0.2f, 0.2f), 0f);
            hitboxRoot.InitHitboxes();

            ZombieView view = root.AddComponent<ZombieView>();
            SetObjectArray(view, "_variants", models);
            SetObjectField(view, "_flashMaterial", flashMaterial);

            return SavePrefab(root, "Zombie_Normal");
        }

        private static Animator[] CreateCharacterModels(Transform root, string[] modelNames, int layer, RuntimeAnimatorController controller, float scale)
        {
            var visual = new GameObject("Visual") { layer = layer };
            visual.transform.SetParent(root, false);
            visual.transform.localScale = Vector3.one * scale;

            var animators = new Animator[modelNames.Length];
            for (int modelIndex = 0; modelIndex < modelNames.Length; modelIndex++)
            {
                string path = $"{MixamoAssetSetup.CHARACTER_FOLDER}/{modelNames[modelIndex]}.fbx";
                var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                model.transform.SetParent(visual.transform, false);
                SetLayerRecursively(model.transform, layer);

                Animator animator = model.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = model.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = controller;
                animator.avatar = AssetDatabase.LoadAssetAtPath<Avatar>(path);
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                animators[modelIndex] = animator;
            }

            return animators;
        }

        private static void CreateThrowRockPrefab(Material material)
        {
            var root = new GameObject("ThrowRock");
            CreateVisual(PrimitiveType.Sphere, "Rock", root.transform, Vector3.zero, new Vector3(0.28f, 0.2f, 0.24f), material);
            GameObject chip = CreateVisual(PrimitiveType.Cube, "Chip", root.transform, new Vector3(0.06f, 0.05f, 0f), new Vector3(0.12f, 0.1f, 0.14f), material);
            chip.transform.localRotation = Quaternion.Euler(20f, 35f, 10f);
            PrefabUtility.SaveAsPrefabAsset(root, $"{PREFAB_FOLDER}/ThrowRock.prefab");
            UnityEngine.Object.DestroyImmediate(root);
        }

        internal static void SetLayerRecursively(Transform target, int layer)
        {
            target.gameObject.layer = layer;
            foreach (Transform child in target)
            {
                SetLayerRecursively(child, layer);
            }
        }

        private static NetworkObject CreateDirectorPrefab()
        {
            var root = new GameObject("GameDirector");
            root.AddComponent<NetworkObject>();
            root.AddComponent<GameDirectorNetwork>();
            return SavePrefab(root, "GameDirector");
        }

        private static void CreateHitbox(Transform parent, string name, int layer, HitZone zone, HitboxTypes type, Vector3 position, Vector3 boxExtents, float sphereRadius)
        {
            var hitboxObject = new GameObject(name) { layer = layer };
            hitboxObject.transform.SetParent(parent, false);
            hitboxObject.transform.localPosition = position;
            ZoneHitbox hitbox = hitboxObject.AddComponent<ZoneHitbox>();
            hitbox.Type = type;
            hitbox.BoxExtents = boxExtents;
            hitbox.SphereRadius = sphereRadius;
            var serialized = new SerializedObject(hitbox);
            serialized.FindProperty("_zone").enumValueIndex = (int)zone;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static GameObject CreateVisual(PrimitiveType primitive, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(primitive);
            visual.name = name;
            UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = position;
            visual.transform.localScale = scale;
            visual.layer = parent.gameObject.layer;
            visual.GetComponent<Renderer>().sharedMaterial = material;
            return visual;
        }

        internal static NetworkObject SavePrefab(GameObject root, string name)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PREFAB_FOLDER}/{name}.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return prefab.GetComponent<NetworkObject>();
        }

        internal static void SetPrefabRef(SerializedProperty property, NetworkObject prefab)
        {
            string hex = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab));
            var bytes = new byte[GUID_BYTE_COUNT];
            for (int byteIndex = 0; byteIndex < GUID_BYTE_COUNT; byteIndex++)
            {
                bytes[byteIndex] = Convert.ToByte(hex.Substring(byteIndex * 2, 2), 16);
            }

            SerializedProperty raw = property.FindPropertyRelative("RawGuidValue");
            raw.GetFixedBufferElementAtIndex(0).longValue = BitConverter.ToInt64(bytes, 0);
            raw.GetFixedBufferElementAtIndex(1).longValue = BitConverter.ToInt64(bytes, 8);
        }

        internal static Material CreateMaterial(string name, Color color, string shaderName = LIT_SHADER)
        {
            string path = $"{MATERIAL_FOLDER}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find(shaderName));
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        internal static T CreateAsset<T>(string name, Action<SerializedObject> configure) where T : ScriptableObject
        {
            string path = $"{DATA_FOLDER}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var serialized = new SerializedObject(asset);
            configure(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void SetShopItem(SerializedProperty item, string name, ShopItemKind kind, WeaponDefinition weapon, int price, int amount)
        {
            item.FindPropertyRelative("_displayName").stringValue = name;
            item.FindPropertyRelative("_kind").enumValueIndex = (int)kind;
            item.FindPropertyRelative("_weapon").objectReferenceValue = weapon;
            item.FindPropertyRelative("_price").intValue = price;
            item.FindPropertyRelative("_amount").intValue = amount;
        }

        private static void AssignObjects(SerializedProperty array, params UnityEngine.Object[] values)
        {
            array.arraySize = values.Length;
            for (int valueIndex = 0; valueIndex < values.Length; valueIndex++)
            {
                array.GetArrayElementAtIndex(valueIndex).objectReferenceValue = values[valueIndex];
            }
        }

        internal static void SetObjectArray(UnityEngine.Object target, string field, params UnityEngine.Object[] values)
        {
            var serialized = new SerializedObject(target);
            AssignObjects(serialized.FindProperty(field), values);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetVectorArray(SerializedProperty property, Vector3[] values)
        {
            property.arraySize = values.Length;
            for (int valueIndex = 0; valueIndex < values.Length; valueIndex++)
            {
                property.GetArrayElementAtIndex(valueIndex).vector3Value = values[valueIndex];
            }
        }

        internal static void SetObjectField(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            int slash = path.LastIndexOf('/');
            EnsureFolder(path.Substring(0, slash));
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
