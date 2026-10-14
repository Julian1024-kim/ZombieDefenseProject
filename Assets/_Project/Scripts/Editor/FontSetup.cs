// 메뉴 ZD/Setup Korean Font: Noto Sans KR(OFL)로 동적 TMP 글꼴을 만들고 TMP 기본 대체 글꼴에 넣는다.
// 그래서 기존 텍스트(기본 글꼴 LiberationSans)를 바꾸지 않아도 닉네임·방 이름의 한글이 보인다.
// 동적 글꼴이라 쓰인 글자만 그때그때 아틀라스에 넣는다(한글 전체를 미리 굽지 않음).

using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace ZD.Editor
{
    internal static class FontSetup
    {
        private const string SOURCE_FONT_PATH = "Assets/_Project/Art/Fonts/NotoSansKR.ttf";
        private const string FONT_ASSET_PATH = "Assets/_Project/Art/Fonts/NotoSansKR SDF.asset";
        private const int SAMPLING_POINT_SIZE = 64;
        private const int ATLAS_PADDING = 6;
        private const int ATLAS_SIZE = 1024;

        [MenuItem("ZD/Setup Korean Font")]
        public static void Apply()
        {
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);
            if (fontAsset == null)
            {
                fontAsset = CreateFontAsset();
                if (fontAsset == null)
                {
                    Debug.LogWarning($"한글 글꼴 파일이 없습니다: {SOURCE_FONT_PATH}");
                    return;
                }
            }

            TMP_Settings settings = TMP_Settings.instance;
            if (!TMP_Settings.fallbackFontAssets.Contains(fontAsset))
            {
                TMP_Settings.fallbackFontAssets.Add(fontAsset);
                EditorUtility.SetDirty(settings);
            }

            AssetDatabase.SaveAssets();
        }

        private static TMP_FontAsset CreateFontAsset()
        {
            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SOURCE_FONT_PATH);
            if (sourceFont == null)
            {
                return null;
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont, SAMPLING_POINT_SIZE, ATLAS_PADDING, GlyphRenderMode.SDFAA, ATLAS_SIZE, ATLAS_SIZE, AtlasPopulationMode.Dynamic, true);
            fontAsset.name = "NotoSansKR SDF";
            AssetDatabase.CreateAsset(fontAsset, FONT_ASSET_PATH);

            Texture2D atlas = fontAsset.atlasTextures[0];
            atlas.name = "NotoSansKR Atlas";
            AssetDatabase.AddObjectToAsset(atlas, fontAsset);
            fontAsset.material.name = "NotoSansKR Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            EditorUtility.SetDirty(fontAsset);
            return fontAsset;
        }
    }
}
