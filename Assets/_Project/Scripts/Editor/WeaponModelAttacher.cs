// 에셋 스토어 무기 모델을 캐릭터 모델마다 오른손 뼈 아래에 붙여 플레이어 프리팹에 굽는다.
// 쥐는 각도는 플레이 모드에서 실제 자세를 여러 프레임(서기, 걷기, 뛰기, 사격) 재서 네 캐릭터 평균으로 정했다.
// 소총·샷건은 이 각도가 기본이고, 게임 중에는 PlayerWeaponView가 매 프레임 두 손에 맞춰 돌린다.
// 무기 모델에 딸려 온 충돌체와 Animator(총 자체 애니메이션)는 지운다.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ZD.Gameplay.Weapons;
using ZD.Presentation.Players;

namespace ZD.Editor
{
    internal static class WeaponModelAttacher
    {
        private const string RIGHT_HAND_SUFFIX = ":RightHand";

        // 총 모델 안의 총구 끝 지점 이름(Modern Guns Pack).
        private const string MUZZLE_NAME = "Barrel_Location";

        public enum Grip
        {
            Rifle,
            Pistol,
            Knife,
        }

        public readonly struct Spec
        {
            public readonly WeaponDefinition Weapon;
            public readonly string PrefabPath;
            public readonly Grip Grip;
            public readonly Vector3 GripPoint;
            public readonly Vector3 SupportPoint;
            public readonly Vector3 ModelEuler;

            public Spec(WeaponDefinition weapon, string prefabPath, Grip grip, Vector3 gripPoint, Vector3 supportPoint = default, Vector3 modelEuler = default)
            {
                Weapon = weapon;
                PrefabPath = prefabPath;
                Grip = grip;
                GripPoint = gripPoint;
                SupportPoint = supportPoint;
                ModelEuler = modelEuler;
            }

            public bool IsTwoHanded => Grip == Grip.Rifle;
        }

        public sealed class Result
        {
            public GameObject[] Models;
            public Transform[] Muzzles;
            public byte[] WeaponIds;
            public Vector3[] GripPoints;
            public Vector3[] SupportAxes;
        }

        public static Result Attach(Animator[] characters, Spec[] specs, WeaponCatalog catalog, int layer)
        {
            var available = new List<Spec>(specs.Length);
            for (int specIndex = 0; specIndex < specs.Length; specIndex++)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(specs[specIndex].PrefabPath) != null)
                {
                    available.Add(specs[specIndex]);
                }
                else
                {
                    Debug.LogWarning($"무기 모델이 없어 맨손으로 보입니다: {specs[specIndex].PrefabPath}");
                }
            }

            var result = new Result
            {
                WeaponIds = new byte[available.Count],
                GripPoints = new Vector3[available.Count],
                SupportAxes = new Vector3[available.Count],
                Models = new GameObject[characters.Length * available.Count],
                Muzzles = new Transform[characters.Length * available.Count],
            };

            for (int specIndex = 0; specIndex < available.Count; specIndex++)
            {
                Spec spec = available[specIndex];
                result.WeaponIds[specIndex] = catalog.GetId(spec.Weapon);
                result.GripPoints[specIndex] = spec.GripPoint;
                result.SupportAxes[specIndex] = spec.IsTwoHanded ? (spec.SupportPoint - spec.GripPoint).normalized : Vector3.zero;
            }

            for (int characterIndex = 0; characterIndex < characters.Length; characterIndex++)
            {
                Transform hand = FindRightHand(characters[characterIndex].transform);
                for (int specIndex = 0; specIndex < available.Count; specIndex++)
                {
                    int modelIndex = characterIndex * available.Count + specIndex;
                    result.Models[modelIndex] = CreateModel(hand, available[specIndex], layer);
                    result.Muzzles[modelIndex] = FindChild(result.Models[modelIndex].transform, MUZZLE_NAME);
                }
            }

            return result;
        }

        private static GameObject CreateModel(Transform hand, in Spec spec, int layer)
        {
            var model = (GameObject)Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath), hand, false);
            model.name = $"Weapon_{spec.Weapon.name}";
            DestroyAll<Collider>(model);
            DestroyAll<Animator>(model);

            Quaternion rotation = ModelRotation(spec);
            model.transform.localRotation = rotation;
            model.transform.localPosition = PlayerWeaponView.PalmCenter - rotation * spec.GripPoint;
            model.transform.localScale = Vector3.one;
            MvpAssetFactory.SetLayerRecursively(model.transform, layer);
            model.SetActive(false);
            return model;
        }

        private static Quaternion ModelRotation(in Spec spec)
        {
            if (spec.IsTwoHanded)
            {
                return GripRotation(spec.Grip) * Quaternion.FromToRotation(spec.SupportPoint - spec.GripPoint, Vector3.forward);
            }

            return GripRotation(spec.Grip) * Quaternion.Euler(spec.ModelEuler);
        }

        private static Quaternion GripRotation(Grip grip)
        {
            return grip switch
            {
                // 오른손 손바닥 → 왼손 손바닥 방향을 앞(+z)으로, 위를 세상 위쪽에 가깝게 둔 축(4캐릭터 × 8동작 평균).
                Grip.Rifle => Quaternion.Euler(283.2f, 12.8f, 84.1f),
                Grip.Pistol => Quaternion.Euler(289f, 88f, 0f),
                // 칼날(+z)이 앞을 향하게.
                _ => Quaternion.Euler(294f, 249f, 196f),
            };
        }

        private static void DestroyAll<T>(GameObject model) where T : Component
        {
            T[] components = model.GetComponentsInChildren<T>(true);
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                Object.DestroyImmediate(components[componentIndex]);
            }
        }

        private static Transform FindChild(Transform root, string name)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int childIndex = 0; childIndex < children.Length; childIndex++)
            {
                if (children[childIndex].name == name)
                {
                    return children[childIndex];
                }
            }

            return null;
        }

        private static Transform FindRightHand(Transform root)
        {
            Transform[] bones = root.GetComponentsInChildren<Transform>(true);
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                if (bones[boneIndex].name.EndsWith(RIGHT_HAND_SUFFIX))
                {
                    return bones[boneIndex];
                }
            }

            return root;
        }
    }
}
