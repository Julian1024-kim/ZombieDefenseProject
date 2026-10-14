// Addressables 설정과 그룹 4개(Common, Zombies, Weapons, Maps)를 만들고, 에셋을 그룹에 등록한다.
// 플레이어 빌드 때 Addressables 콘텐츠도 같이 빌드하도록 설정해, 콘텐츠 빌드를 빼먹는 일이 없게 한다.

using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace ZD.Editor
{
    internal static class AddressablesSetup
    {
        public const string COMMON_GROUP = "Common";
        public const string ZOMBIES_GROUP = "Zombies";
        public const string WEAPONS_GROUP = "Weapons";
        public const string MAPS_GROUP = "Maps";

        private const string TEMPLATE_DEFAULT_GROUP = "Default Local Group";
        private static readonly string[] GroupNames = { COMMON_GROUP, ZOMBIES_GROUP, WEAPONS_GROUP, MAPS_GROUP };

        [MenuItem("ZD/Setup Addressables Groups")]
        public static AddressableAssetSettings EnsureGroups()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            for (int groupIndex = 0; groupIndex < GroupNames.Length; groupIndex++)
            {
                if (settings.FindGroup(GroupNames[groupIndex]) == null)
                {
                    settings.CreateGroup(GroupNames[groupIndex], false, false, true, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
                }
            }

            settings.DefaultGroup = settings.FindGroup(COMMON_GROUP);
            AddressableAssetGroup templateGroup = settings.FindGroup(TEMPLATE_DEFAULT_GROUP);
            if (templateGroup != null && templateGroup.entries.Count == 0)
            {
                settings.RemoveGroup(templateGroup);
            }

            settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;
            EditorUtility.SetDirty(settings);
            return settings;
        }

        public static void MarkAddressable(string assetPath, string groupName, string address)
        {
            AddressableAssetSettings settings = EnsureGroups();
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, settings.FindGroup(groupName), false, true);
            entry.address = address;
            EditorUtility.SetDirty(settings);
        }
    }
}
