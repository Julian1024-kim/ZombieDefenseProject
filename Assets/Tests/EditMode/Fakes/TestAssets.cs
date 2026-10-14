// 비공개 직렬화 필드를 SerializedObject로 채워 ScriptableObject를 만든다.
// 그래서 제품 코드에 테스트용 setter가 필요 없다.

using System;
using System.Collections.Generic;
using MessagePipe;
using UnityEditor;
using UnityEngine;
using ZD.Core;
using ZD.Gameplay.Building;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Defense;
using ZD.Gameplay.Economy;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Players;
using ZD.Gameplay.Waves;
using ZD.Gameplay.Weapons;
using ZD.Gameplay.Zombies;

namespace ZD.Tests.EditMode.Fakes
{
    internal static class TestAssets
    {
        public static T Create<T>(Action<SerializedObject> configure) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            var serialized = new SerializedObject(asset);
            configure(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

        public static ZombieDefinition Zombie(int cost = 1, int firstWave = 1, int health = 100, int damage = 10)
        {
            return Create<ZombieDefinition>(so =>
            {
                so.FindProperty("_spawnCost").intValue = cost;
                so.FindProperty("_firstWave").intValue = firstWave;
                so.FindProperty("_maxHealth").intValue = health;
                so.FindProperty("_attackDamage").intValue = damage;
                so.FindProperty("_attackRange").floatValue = 1.5f;
                so.FindProperty("_attackIntervalSeconds").floatValue = 1f;
                so.FindProperty("_killPoints").intValue = 50;
                so.FindProperty("_headshotBonusPoints").intValue = 20;
            });
        }

        public static WaveProfile Waves(int baseBudget, int perWave, params ZombieDefinition[] types)
        {
            return Create<WaveProfile>(so =>
            {
                SerializedProperty array = so.FindProperty("_zombieTypes");
                array.arraySize = types.Length;
                for (int typeIndex = 0; typeIndex < types.Length; typeIndex++)
                {
                    array.GetArrayElementAtIndex(typeIndex).objectReferenceValue = types[typeIndex];
                }

                so.FindProperty("_baseBudget").intValue = baseBudget;
                so.FindProperty("_budgetPerWave").intValue = perWave;
                so.FindProperty("_preparationSeconds").floatValue = 1f;
                so.FindProperty("_spawnIntervalSeconds").floatValue = 0f;
                so.FindProperty("_maxAliveZombies").intValue = 100;
            });
        }

        public static WeaponDefinition Pistol(int damage = 20, float shotsPerSecond = 4f, int magazine = 2, int reserve = 4)
        {
            return Create<WeaponDefinition>(so =>
            {
                so.FindProperty("_category").enumValueIndex = (int)WeaponCategory.Secondary;
                so.FindProperty("_behaviorType").enumValueIndex = (int)WeaponBehaviorType.Hitscan;
                so.FindProperty("_damage").intValue = damage;
                so.FindProperty("_shotsPerSecond").floatValue = shotsPerSecond;
                so.FindProperty("_isAutomatic").boolValue = false;
                so.FindProperty("_headshotMultiplier").floatValue = 2f;
                so.FindProperty("_magazineSize").intValue = magazine;
                so.FindProperty("_startingReserveAmmo").intValue = reserve;
                so.FindProperty("_reloadSeconds").floatValue = 1f;
            });
        }

        public static WeaponDefinition Knife(int damage = 50, int maxTargets = 2)
        {
            return Create<WeaponDefinition>(so =>
            {
                so.FindProperty("_category").enumValueIndex = (int)WeaponCategory.Melee;
                so.FindProperty("_behaviorType").enumValueIndex = (int)WeaponBehaviorType.Melee;
                so.FindProperty("_damage").intValue = damage;
                so.FindProperty("_shotsPerSecond").floatValue = 2f;
                so.FindProperty("_range").floatValue = 1f;
                so.FindProperty("_maxMeleeTargets").intValue = maxTargets;
            });
        }

        public static WeaponDefinition Primary(string name, WeaponBehaviorType behavior = WeaponBehaviorType.Hitscan, int pellets = 1, float spread = 0f)
        {
            return Create<WeaponDefinition>(so =>
            {
                so.FindProperty("_displayName").stringValue = name;
                so.FindProperty("_category").enumValueIndex = (int)WeaponCategory.Primary;
                so.FindProperty("_behaviorType").enumValueIndex = (int)behavior;
                so.FindProperty("_damage").intValue = 10;
                so.FindProperty("_shotsPerSecond").floatValue = 10f;
                so.FindProperty("_range").floatValue = 50f;
                so.FindProperty("_magazineSize").intValue = 30;
                so.FindProperty("_startingReserveAmmo").intValue = 60;
                so.FindProperty("_reloadSeconds").floatValue = 1f;
                so.FindProperty("_pelletCount").intValue = pellets;
                so.FindProperty("_spreadDegrees").floatValue = spread;
            });
        }

        public static WeaponCatalog Catalog(params WeaponDefinition[] weapons)
        {
            return Create<WeaponCatalog>(so =>
            {
                SerializedProperty array = so.FindProperty("_weapons");
                array.arraySize = weapons.Length;
                for (int weaponIndex = 0; weaponIndex < weapons.Length; weaponIndex++)
                {
                    array.GetArrayElementAtIndex(weaponIndex).objectReferenceValue = weapons[weaponIndex];
                }
            });
        }

        public static InventorySettings Inventory(WeaponDefinition secondary, WeaponDefinition melee, int maxHealKits = 2, int healAmount = 70, float healSeconds = 3f)
        {
            return Create<InventorySettings>(so =>
            {
                so.FindProperty("_startingSecondary").objectReferenceValue = secondary;
                so.FindProperty("_startingMelee").objectReferenceValue = melee;
                so.FindProperty("_maxHealKits").intValue = maxHealKits;
                so.FindProperty("_healAmount").intValue = healAmount;
                so.FindProperty("_healSeconds").floatValue = healSeconds;
            });
        }

        public static ShopCatalog Shop(float interactRange, params (ShopItemKind kind, WeaponDefinition weapon, int price, int amount)[] items)
        {
            return Create<ShopCatalog>(so =>
            {
                so.FindProperty("_interactRange").floatValue = interactRange;
                SerializedProperty array = so.FindProperty("_items");
                array.arraySize = items.Length;
                for (int itemIndex = 0; itemIndex < items.Length; itemIndex++)
                {
                    SerializedProperty item = array.GetArrayElementAtIndex(itemIndex);
                    item.FindPropertyRelative("_kind").enumValueIndex = (int)items[itemIndex].kind;
                    item.FindPropertyRelative("_weapon").objectReferenceValue = items[itemIndex].weapon;
                    item.FindPropertyRelative("_price").intValue = items[itemIndex].price;
                    item.FindPropertyRelative("_amount").intValue = items[itemIndex].amount;
                }
            });
        }

        public static BaseDefinition Base(int maxHealth = 3000, float healFraction = 0.2f, float aggroRadius = 6f)
        {
            return Create<BaseDefinition>(so =>
            {
                so.FindProperty("_maxHealth").intValue = maxHealth;
                so.FindProperty("_waveClearHealFraction").floatValue = healFraction;
                so.FindProperty("_size").vector3Value = new Vector3(3f, 2f, 3f);
                so.FindProperty("_playerAggroRadius").floatValue = aggroRadius;
            });
        }

        public static BarricadeDefinition Barricade(BarricadeKind kind = BarricadeKind.Wall, int price = 300, int health = 300,
            float triggerRadius = 0.8f, float effectRadius = 3f, int trapDamage = 150, float holdSeconds = 3f, int uses = 1)
        {
            return Create<BarricadeDefinition>(so =>
            {
                so.FindProperty("_displayName").stringValue = kind.ToString();
                so.FindProperty("_kind").enumValueIndex = (int)kind;
                so.FindProperty("_price").intValue = price;
                so.FindProperty("_size").vector3Value = new Vector3(2f, 1.2f, 0.4f);
                so.FindProperty("_maxHealth").intValue = health;
                so.FindProperty("_triggerRadius").floatValue = triggerRadius;
                so.FindProperty("_effectRadius").floatValue = effectRadius;
                so.FindProperty("_trapDamage").intValue = trapDamage;
                so.FindProperty("_holdSeconds").floatValue = holdSeconds;
                so.FindProperty("_uses").intValue = uses;
            });
        }

        public static BuildCatalog Build(params BarricadeDefinition[] items)
        {
            return Create<BuildCatalog>(so =>
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
            });
        }
    }
}
