// 에셋 스토어에서 받은 무기·방어막·지뢰 모델을 게임에 맞게 정리한다: Built-in(Standard) 머티리얼을 URP Lit으로 바꾸고, 쓰는 텍스처를 2048 이하로 줄인다.
// 게임이 쓰는 프리팹 경로도 여기에 모아 둔다.
// 몇 번을 다시 실행해도 결과가 같다.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZD.Editor
{
    internal static class ThirdPartyAssetSetup
    {
        public const string ROOT = "Assets/_Project/Art/ThirdParty/";
        public const string GUNS = ROOT + "Nokobot/Modern Guns v4/_Prefabs/";

        public const string PISTOL = GUNS + "M1911 Handgun/Handgun Black/M1911 Handgun_Black.prefab";
        public const string ASSAULT_RIFLE = GUNS + "Assault Rifles/M16 Assault Rifle/M16 Assault Rifle.prefab";
        public const string SHOTGUN = GUNS + "Mossberg 590 Shotgun/Mossberg 590 Shotgun.prefab";
        public const string SNIPER_RIFLE = GUNS + "L96A1 Sniper Rifle/Sniper Rifle Black/L96A1 Sniper Rifle _Black.prefab";
        public const string RPG = GUNS + "RPG-7 Launcher/RPG-7 Launcher.prefab";
        public const string KNIFE = ROOT + "SciFiKnife/Prefab/Prefab_Knife.prefab";
        // 도끼·마체테(3단계)는 칼 쥐기(Grip.Knife)로 붙인다. 플레이 모드에서 확인한 값:
        // 도끼는 손잡이 (0, -0.05, 0), 모델 축 보정 (90, 0, 0)(자루 +y를 앞으로), 마체테는 손잡이 (-0.12, 0, 0), 보정 (0, -90, 0)(칼날 +x를 앞으로).
        public const string AXE = ROOT + "IceAxe/Prefab/IceAxe_URP.prefab";
        public const string MACHETE = ROOT + "Machete01/Machete01_prefab.prefab";
        public const string BARRICADE = ROOT + "RustyBarricade/Prefabs/URP/Police Barricade 2K T1 U.prefab";
        public const string CONCRETE = ROOT + "SectorWarzone/Concrete Barriers/Prefabs/PRE_Concrete Barrier_1.prefab";
        public const string MINE = ROOT + "M18Claymore/Prefabs/M18_Claymore.prefab";

        private const string URP_LIT = "Universal Render Pipeline/Lit";
        private const int MAX_TEXTURE_SIZE = 2048;

        private static readonly string[] UsedPrefabs = { PISTOL, ASSAULT_RIFLE, SHOTGUN, SNIPER_RIFLE, RPG, KNIFE, AXE, MACHETE, BARRICADE, CONCRETE, MINE };

        [MenuItem("ZD/Setup Third-Party Assets")]
        public static void SetupAll()
        {
            var materials = new HashSet<Material>();
            for (int prefabIndex = 0; prefabIndex < UsedPrefabs.Length; prefabIndex++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(UsedPrefabs[prefabIndex]);
                if (prefab == null)
                {
                    Debug.LogWarning($"에셋이 없습니다(설계 문서의 목록대로 받아 주세요): {UsedPrefabs[prefabIndex]}");
                    continue;
                }

                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
                for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    Material[] shared = renderers[rendererIndex].sharedMaterials;
                    for (int slotIndex = 0; slotIndex < shared.Length; slotIndex++)
                    {
                        if (shared[slotIndex] != null)
                        {
                            materials.Add(shared[slotIndex]);
                        }
                    }
                }
            }

            foreach (Material material in materials)
            {
                ConvertToUrp(material);
                LimitTextures(material);
            }

            AssetDatabase.SaveAssets();
        }

        private static void ConvertToUrp(Material material)
        {
            if (material.shader.name.StartsWith("Universal Render Pipeline"))
            {
                return;
            }

            Texture mainTexture = Read(material, "_MainTex");
            Color color = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
            Texture normal = Read(material, "_BumpMap");
            Texture metallic = Read(material, "_MetallicGlossMap");
            Texture occlusion = Read(material, "_OcclusionMap");
            Texture emission = Read(material, "_EmissionMap");
            Color emissionColor = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black;
            float metallicValue = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : 0f;
            float smoothness = material.HasProperty("_Glossiness") ? material.GetFloat("_Glossiness") : 0.5f;

            material.shader = Shader.Find(URP_LIT);
            material.SetTexture("_BaseMap", mainTexture);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallicValue);
            material.SetFloat("_Smoothness", smoothness);
            SetTextureWithKeyword(material, "_BumpMap", normal, "_NORMALMAP");
            SetTextureWithKeyword(material, "_MetallicGlossMap", metallic, "_METALLICSPECGLOSSMAP");
            SetTextureWithKeyword(material, "_OcclusionMap", occlusion, "_OCCLUSIONMAP");
            if (emission != null || emissionColor.maxColorComponent > 0.01f)
            {
                SetTextureWithKeyword(material, "_EmissionMap", emission, "_EMISSION");
                material.SetColor("_EmissionColor", emissionColor);
                material.EnableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(material);
        }

        private static void LimitTextures(Material material)
        {
            string[] names = material.GetTexturePropertyNames();
            for (int nameIndex = 0; nameIndex < names.Length; nameIndex++)
            {
                Texture texture = material.GetTexture(names[nameIndex]);
                if (texture == null || !(AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) is TextureImporter importer))
                {
                    continue;
                }

                if (importer.maxTextureSize > MAX_TEXTURE_SIZE)
                {
                    importer.maxTextureSize = MAX_TEXTURE_SIZE;
                    importer.SaveAndReimport();
                }
            }
        }

        private static Texture Read(Material material, string property)
        {
            return material.HasProperty(property) ? material.GetTexture(property) : null;
        }

        private static void SetTextureWithKeyword(Material material, string property, Texture texture, string keyword)
        {
            material.SetTexture(property, texture);
            if (texture != null)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }
    }
}
